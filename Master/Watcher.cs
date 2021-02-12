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
        private FileSystemWatcher FSWatcher;
        private Timer retryTimer;
        private const int timerDuration = 1 * 1000;

        Queue<string> UnproccessedFiles = new Queue<string>(15);

        List<string> ProcessedOnChangedFiles = new List<string>();

        private void PutSlaveToWork(string filePath)
        {
            List<Slave> slaves = Settings.Instance.Slaves;

            Slave idleSlave = slaves.Find(slave => !slave.IsBusy());

            if (!(idleSlave is null) /*&& idleSlave.SendWork(filePath)*/)
            {
                Console.WriteLine("File {0} succesfully sent to slave {1} for transcoding", filePath, idleSlave.IP);
            }
            else
            {
                UnproccessedFiles.Enqueue(filePath);
            }
        }

        private void OnCreate(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(Settings.Instance.SharedSettings.Delay * 1000);
            string filePath = e.FullPath;
            PutSlaveToWork(filePath);
        }


        static readonly object padlock = new object();
        private void OnChanged(object sender, FileSystemEventArgs e)
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
            FSWatcher = new FileSystemWatcher
            {
                Path = Settings.Instance.SharedSettings.Watchfolder,

                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.mxf"
            };

            FSWatcher.Created += OnCreate;
            FSWatcher.Changed += OnChanged;
            retryTimer = new Timer(RetryCallback, null, Timeout.Infinite, Timeout.Infinite);
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
            FSWatcher.EnableRaisingEvents = true;
            retryTimer.Change(timerDuration, timerDuration);
        }

        public void Stop()
        {
            FSWatcher.EnableRaisingEvents = false;
            retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

    }
}
