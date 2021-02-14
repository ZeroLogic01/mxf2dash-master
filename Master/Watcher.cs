using Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Master
{
    class Watcher
    {
        private readonly FileSystemWatcher WatchFolderWatcher;
        /// <summary>
        /// File watcher to watch <see cref="CheckFolder"/> for XML files from the 3rd party application.
        /// </summary>
        private readonly FileSystemWatcher CheckFolderFSWatcher;
        private readonly Timer retryTimer;
        private const int timerDuration = 1 * 1000;
        private readonly Queue<string> UnproccessedFiles = new Queue<string>(15);
        private readonly List<string> ProcessedOnChangedFiles = new List<string>();

        private void PutSlaveToWork(string filePath)
        {
            List<Slave> slaves = Settings.Instance.Slaves;

            Slave idleSlave = slaves.Find(slave => !slave.IsBusy());

            if (!(idleSlave is null) && idleSlave.SendWork(filePath))
            {
                Console.WriteLine("File {0} successfully sent to slave {1} for transcoding", filePath, idleSlave.IP);
            }
            else
            {
                UnproccessedFiles.Enqueue(filePath);
            }
        }

        private void WatchFolderWatcher_OnCreated(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(Settings.Instance.SharedSettings.Delay * 1000);
            string filePath = e.FullPath;
            PutSlaveToWork(filePath);
        }


        static readonly object padlock = new object();
        private void WatchFolderWatcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            lock (padlock)
            {
                if (ProcessedOnChangedFiles.Contains(e.FullPath))
                {
                    return;
                }

                ProcessedOnChangedFiles.Add(filePath);
            }

            Thread.Sleep(Settings.Instance.SharedSettings.Delay * 1000);
            PutSlaveToWork(filePath);
        }

        public Watcher()
        {
            #region Watch Folder File System Watcher

            WatchFolderWatcher = new FileSystemWatcher
            {
                Path = Settings.Instance.SharedSettings.Watchfolder,

                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.mxf"
            };

            WatchFolderWatcher.Created += WatchFolderWatcher_OnCreated;
            WatchFolderWatcher.Changed += WatchFolderWatcher_OnChanged;
            retryTimer = new Timer(RetryCallback, null, Timeout.Infinite, Timeout.Infinite);
            #endregion

            #region  Check folder (XML) file system watcher

            CheckFolderFSWatcher = new FileSystemWatcher()
            {
                Path = Settings.Instance.SharedSettings.Checkfolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.xml"
            };

            CheckFolderFSWatcher.Created += CheckFolderFSWatcher_OnCreated;

            #endregion
        }

        private void PerformTranscoderOverwriteRequest(string fileName)
        {
            var overWriteRequest = OverwriteRequestFileHelper.Read(fileName);
            if (overWriteRequest != null)
            {
                bool isProcessFound = false;

                string xmlOutputFile = Path.Combine(Settings.Instance.SharedSettings.OverwriteRequestOutputFolder,
                           $"{Path.GetFileName(fileName)}_{overWriteRequest.Status.ToLower()}");

                foreach (var slave in Settings.Instance.Slaves)
                {
                    var process = slave.FilesBeingProcessed
                        .FirstOrDefault(o => Path.GetFileName(o.FileName)
                        .Equals(overWriteRequest.FileName, StringComparison.OrdinalIgnoreCase));

                    if (process != null)
                    {
                        isProcessFound = true;
                        overWriteRequest.Status = slave.KillProcess(process) ? "Done" : "Failed";
                        break;
                    }
                }

                // if this is false, this means no slave currently processing this file, 
                // we can set it's status as 'Done' in case 3rd party app don't have to wait indefinitely 
                if (!isProcessFound)
                {
                    overWriteRequest.Status = "Done";
                }

                // change the name with status as a suffix
                overWriteRequest.TimeofProcessed = string
                            .Format("{0}T{1}", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm"));

                // update the XML file
                if (OverwriteRequestFileHelper.Write(xmlOutputFile, overWriteRequest))
                {
                    ProcessedOnChangedFiles
                        .Remove(Path.Combine(Settings.Instance.SharedSettings.Watchfolder, overWriteRequest.FileName));
                }
            }
        }

        private void CheckFolderFSWatcher_OnCreated(object sender, FileSystemEventArgs e)
        {
            PerformTranscoderOverwriteRequest(e.FullPath);

        }

        private void RetryCallback(object state)
        {
            if (UnproccessedFiles.Count == 0)
            {
                return;
            }

            string filename = UnproccessedFiles.Dequeue();

            PutSlaveToWork(filename);

        }

        public void Start()
        {
            WatchFolderWatcher.EnableRaisingEvents = CheckFolderFSWatcher.EnableRaisingEvents = true;
            retryTimer.Change(timerDuration, timerDuration);
        }

        public void Stop()
        {
            WatchFolderWatcher.EnableRaisingEvents = CheckFolderFSWatcher.EnableRaisingEvents = false;
            retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

    }
}
