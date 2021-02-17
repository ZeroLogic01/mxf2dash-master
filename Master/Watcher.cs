﻿using Commons;
using Master.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
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
        private ConcurrentQueue<string> UnproccessedFiles = new ConcurrentQueue<string>();
        private readonly List<string> FilesBeingProcessed = new List<string>();


        #region Memory Cache Logic
        private readonly MemoryCache _memCache;
        private readonly CacheItemPolicy _cacheItemPolicy;
        private const int CacheTimeMilliseconds = 1000;
        #endregion

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
            string filePath = e.FullPath;

            if (AddToFilesBeingProcessed(filePath))
            {
                Thread.Sleep(Settings.Instance.SharedSettings.Delay * 1000);
                PutSlaveToWork(filePath);
            }
        }


        private readonly object padlock = new object();
        private bool AddToFilesBeingProcessed(string filePath)
        {
            lock (padlock)
            {
                if (!FilesBeingProcessed.Contains(filePath))
                {
                    FilesBeingProcessed.Add(filePath);
                    return true;
                }

                return false;
            }
        }

        public Watcher()
        {
            #region Memory Cache

            _memCache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy()
            {
                RemovedCallback = OnRemovedFromCache
            };

            #endregion

            #region Watch Folder File System Watcher

            WatchFolderWatcher = new FileSystemWatcher
            {
                Path = Settings.Instance.SharedSettings.Watchfolder,

                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.mxf"
            };

            WatchFolderWatcher.Created += WatchFolderWatcher_OnCreated;
            WatchFolderWatcher.Changed += WatchFolderWatcher_OnCreated;
            WatchFolderWatcher.Error += FileSystemWatcher_Error;
            retryTimer = new Timer(RetryCallback, null, Timeout.Infinite, Timeout.Infinite);
            #endregion

            #region  Check folder (XML) file system watcher

            CheckFolderFSWatcher = new FileSystemWatcher()
            {
                Path = Settings.Instance.SharedSettings.Checkfolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "*.xml"
            };

            CheckFolderFSWatcher.Created += CheckFolderFSWatcher_OnChanged;
            CheckFolderFSWatcher.Changed += CheckFolderFSWatcher_OnChanged;
            WatchFolderWatcher.Error += FileSystemWatcher_Error;

            #endregion
        }

        // Add file event to cache for CacheTimeMilliseconds
        private void CheckFolderFSWatcher_OnChanged(object source, FileSystemEventArgs e)
        {
            _cacheItemPolicy.AbsoluteExpiration =
                DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);

            // Only add if it is not there already (swallow others)
            _memCache.AddOrGetExisting(e.Name, e, _cacheItemPolicy);
        }

        // Handle cache item expiring
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;

            // Now actually handle file event
            var e = (FileSystemEventArgs)args.CacheItem.Value;

            PerformTranscoderOverwriteRequest(e.FullPath);
        }

        private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                Console.WriteLine("Error: File System Watcher internal buffer overflow at " + DateTime.Now);
            }
            else
            {
                Console.WriteLine("Error: Watched directory not accessible at " + DateTime.Now);
            }
            NotAccessibleError(sender as FileSystemWatcher, e);
        }

        static void NotAccessibleError(FileSystemWatcher source, ErrorEventArgs e)
        {
            source.EnableRaisingEvents = false;
            int iMaxAttempts = 120;
            int iTimeOut = 30000;
            int i = 0;
            while (source.EnableRaisingEvents == false && i < iMaxAttempts)
            {
                i += 1;
                try
                {
                    source.EnableRaisingEvents = true;
                }
                catch
                {
                    source.EnableRaisingEvents = false;
                    System.Threading.Thread.Sleep(iTimeOut);
                }
            }

        }

        private void PerformTranscoderOverwriteRequest(string xmlSourceFilePath)
        {
            var overWriteRequest = OverwriteRequestFileHelper.Read(xmlSourceFilePath);

            string mxfFilePath = Path.Combine(Settings.Instance.SharedSettings.Watchfolder, overWriteRequest.FileName);
            if (overWriteRequest != null)
            {
                bool isProcessFound = false;

                // if the overwrite requested for a file is present in the unprocessed queue,
                if (UnproccessedFiles.Contains(mxfFilePath))
                {
                    List<string> list = UnproccessedFiles.ToList();
                    if (isProcessFound = list.Remove(mxfFilePath))
                    {
                        UnproccessedFiles = new ConcurrentQueue<string>(list);

                        overWriteRequest.Status = TranscoderOverwriteRequest.StatusDone;
                    }
                }
                else
                {
                    foreach (var slave in Settings.Instance.Slaves)
                    {
                        var process = slave.FilesBeingProcessedDictionary
                            .FirstOrDefault(o => Path.GetFileName(o.Key)
                            .Equals(overWriteRequest.FileName, StringComparison.OrdinalIgnoreCase));

                        if (process.Key != null)
                        {
                            isProcessFound = true;
                            overWriteRequest.Status = slave.KillProcess(process.Value) ?
                                TranscoderOverwriteRequest.StatusDone :
                                TranscoderOverwriteRequest.StatusFailed;
                            break;
                        }
                    }
                }

                // if this is false, this means no slave currently processing this file, 
                // we can set it's status as 'Done' in case 3rd party app don't have to wait indefinitely 
                if (!isProcessFound)
                {
                    overWriteRequest.Status = TranscoderOverwriteRequest.StatusDone;
                }

                // change the name with status as a suffix
                overWriteRequest.TimeofProcessed = string
                            .Format("{0}T{1}", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm"));


                string xmlOutputFilePath = Path.Combine(Settings.Instance.SharedSettings.OverwriteRequestOutputFolder,
                          $"{Path.GetFileName(xmlSourceFilePath)}_{overWriteRequest.Status.ToLower()}");

                // update the XML file
                OverwriteRequestFileHelper.Write(xmlOutputFilePath, overWriteRequest);

                // send it back to slave for re-processing
                if (overWriteRequest.Status.Equals(TranscoderOverwriteRequest.StatusDone)
                    /* if we get V:abc.mxf & V:\\abc.mxf. Both strings are not equal so 
                     * now we're just comparing the file names ignoring folder path as 
                     * we will have files from just one folder
                     */
                    && FilesBeingProcessed
                    .FirstOrDefault(file => Path.GetFileName(file)
                                            .Equals(Path.GetFileName(mxfFilePath))) != null)
                {
                    lock (padlock)
                    {
                        FilesBeingProcessed.Remove(mxfFilePath);
                    }
                }
            }
        }

        private void RetryCallback(object state)
        {
            if (UnproccessedFiles.Count == 0)
            {
                return;
            }

            UnproccessedFiles.TryDequeue(out string fileName);

            PutSlaveToWork(fileName);

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
