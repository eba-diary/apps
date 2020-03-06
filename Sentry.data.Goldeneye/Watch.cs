using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Newtonsoft.Json;
using System.Security.Cryptography;
using StructureMap;
using Sentry.data.Infrastructure;
using Sentry.data.Common;
using System.Threading.Tasks;
using System.Threading;
using Hangfire;

namespace Sentry.data.Goldeneye
{
    public class Watch
    {
        //Created by Andrew Quaschnick
        //On 11/15/2017

        private FileSystemWatcher watcher;
        private List<FileProcess> allFiles = new List<FileProcess>();
        private IContainer container;
        private DateTime FileCounterStart { get; set; }
        private int DatasetFileConfigId { get; set; }
        private int RetrieverJobId { get; set; }
        private CancellationTokenSource _internalTokenSource;
        private CancellationToken _internalToken;
        private int _iterationLimit;

        /// <summary>
        /// Directory that is to be watched
        /// </summary>
        public string WatchedDir { get; set; }

        private class FileProcess {

            public FileProcess(string fileName)
            {
                this.fileName = fileName;
                this.started = false;
                this.fileCorrectlyDeleted = false;
            }

            public string fileName { get; set; }
            public Boolean started { get; set; }
            public Boolean fileCorrectlyDeleted { get; set; }
        }

        //This is the main method that is run every iteration of the Core.DoWork() method.
        //  It's most likely wise to set it to a few seconds in Core.DoWork() as you might be wasting CPU cycles making it go faster.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        public void Run(string path, CancellationToken token)
        {
            int iterationCounter = 0;
            bool iterationLimitFlag = false;
            do
            {
                var files = allFiles.ToList();

                foreach (FileProcess file in files)
                {
                    if (file.started)
                    {
                        //The DatasetLoader said Success and the file is gone.
                        if (file.fileCorrectlyDeleted)
                        {
                            Logger.Info("File: " + file.fileName + " Success");
                            allFiles.Remove(file);
                        }
                        //The DatasetLoader said Success but the file still exists.                    
                        else
                        {
                            Logger.Info("File: " + file.fileName + " Still Exists");
                        }
                    }
                    else if (!IsFileLocked(file.fileName) && !file.started)
                    {
                        using (container = Bootstrapper.Container.GetNestedContainer())
                        {
                            //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop location
                            BackgroundJob.Enqueue<RetrieverJobService>(RetrieverJobService => RetrieverJobService.RunRetrieverJob(RetrieverJobId, JobCancellationToken.Null, Path.GetFileName(file.fileName)));

                            file.started = true;
                        }
                    }
                    else
                    {
                        Logger.Debug("File: " + file.fileName + " Locked");
                    }
                }

                iterationLimitFlag = (iterationCounter++ > _iterationLimit);

                Thread.Sleep(2000);
                
            } while (!token.IsCancellationRequested && !iterationLimitFlag);

            if (token.IsCancellationRequested)
            {
                Logger.Info($"Watch cancelled for Job:{RetrieverJobId.ToString()}");
            }
            
            if (iterationLimitFlag)
            {
                Logger.Info($"Watch interation limit restart:{RetrieverJobId.ToString()}");
            }
        }

        //This method checks to see if a file is locked by another process.  
        //  This allows us to see if the DatasetLoader is correctly processing a file or 
        //  if another process is still uploading or writing to a location.
        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }
            catch (UnauthorizedAccessException e)
            {
                return false;
            }
            catch (Exception e)
            {
                Logger.Error("File Locked Test Try/Catch Failed", e);
            }

            return false;
        }

        /// <summary>
        /// This method is called when the Windows Process is started in Core and Program.cs
        /// First it creates a file watcher, gives it a directory from app.config, then assigns it events to watch.
        /// The last thing it does is grabs all the files that currently in the directory and adds them to the file list.
        /// The Service will do the rest on it's periodic run.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="watchPath"></param>
        /// <param name="token"></param>
        /// <param name="iterationLimit"></param>
        public void OnStart(int jobId, Uri watchPath, CancellationToken token, int iterationLimit = 43200)
        {

            try
            {
                //Create watcher cancellation token
                _internalTokenSource = new CancellationTokenSource();
                _internalToken = _internalTokenSource.Token;
                _iterationLimit = iterationLimit;

                // Create a new FileSystemWatcher and set its properties.
                watcher = new FileSystemWatcher();
                try
                {
                    watcher.Path = watchPath.LocalPath;
                }
                catch (ArgumentException ex)
                {
                    Logger.Error($"Watcher Path not found - Job:{jobId} ", ex);
                    Logger.Info($"Attempting to self-heal... issuing create directory - Job:{jobId}");
                    Logger.Info($"Attempting to create directory - Job:{jobId} Path:{watchPath.LocalPath}");
                    System.IO.Directory.CreateDirectory(watchPath.LocalPath);
                    Logger.Info($"Directory successfully created - Job:{jobId}");
                    Logger.Info($"Second attempt to assign watcher.path - Job:{jobId}");
                    watcher.Path = watchPath.LocalPath;
                    Logger.Info($"Second attempt successful - Job:{jobId}");
                    Logger.Info($"Self-heal successfull - Job:{jobId}");
                }

                FileCounterStart = DateTime.Now;
                RetrieverJobId = jobId;

                Console.WriteLine("Watcher instance started for : " + watcher.Path);
                Logger.Info("Watcher instance started for : " + watcher.Path);

                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnCreated);
                watcher.Deleted += new FileSystemEventHandler(OnDeleted);
                watcher.Error += new ErrorEventHandler(OnError);

                // Begin watching.
                watcher.EnableRaisingEvents = true;
                watcher.IncludeSubdirectories = true;

                //Get all the files that are currently in the directory on Start to begin monitoring them.
                //  This may happen if the service was killed and needed to restart.
                //  Filter out DatasetLoader Request and Failed Request folders.
                foreach (var a in Directory.GetFiles(watcher.Path, "*", SearchOption.TopDirectoryOnly).Where(w => !Path.GetFileName(w).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))))
                {
                    Console.WriteLine("Found : " + a);
                    Logger.Info("Found : " + a);
                    allFiles.Add(new FileProcess(a));
                }

                using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalToken))
                {
                    //Start directory monitor
                    this.Run(WatchedDir, linkedCts.Token);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initilizing watch for {watchPath}",ex);
            }

        }

        //When a new file is created add the file path to the list of All Files.
        //  The Service will do the rest on it's periodic run.
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!Path.GetFileName(e.FullPath).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix")))
            {
                allFiles.Add(new FileProcess(e.FullPath));
            }            
        }

        //We can see when a user is writing to a file.
        //  At this time we don't need this method.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Method intentionally left empty.
        }

        //When a file is deleted find the file in the internal list and mark it deleted.
        //  The Service will do the rest on it's periodic run.
        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            var path = Path.GetFullPath(e.FullPath).Replace(Path.GetFileName(e.FullPath), "");
            var fileName = Path.GetFileName(e.FullPath);

            //remove ProcessedFilePrefix if it exists
            var origFileName = path + ((fileName.StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))) ? fileName.Substring(Configuration.Config.GetHostSetting("ProcessedFilePrefix").Length) : fileName);

            var file = allFiles.FirstOrDefault(x => x.fileName == origFileName);
            if (file != null)
            {
                file.fileCorrectlyDeleted = true;
            }
            else
            {
                Logger.Info($"Watch detected delete for non-tracked file: {e.FullPath}");
            }           
        }
        private void OnError(object source, ErrorEventArgs e)
        {
            Logger.Error($"Watcher Error - Cancelling watch job (JobId:{RetrieverJobId})", e.GetException());

            _internalTokenSource.Cancel();
        }

        /// <summary>
        /// Returns start datetime of file counter
        /// </summary>
        /// <returns></returns>
        public DateTime GetCountStart()
        {
            return FileCounterStart;
        }
    }
}
