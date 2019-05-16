using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sentry.Common.Logging;
using System.Runtime.InteropServices;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public class HszFileWatchService : IHszFileWatchService
    {
        #region Declarations
        private List<FileProcess> allFiles = new List<FileProcess>();
        private IContainer container;
        private CancellationTokenSource _internalTokenSource;
        private CancellationToken _internalToken;
        private int RetrieverJobId { get; set; }
        private RetrieverJob Job { get; set; }
        #endregion

        public void OnStart(RetrieverJob Job, CancellationToken token)
        {
            Uri watchPath = Job.GetUri();

            try
            {
                //Create watcher cancellation token
                _internalTokenSource = new CancellationTokenSource();
                _internalToken = _internalTokenSource.Token;

                // Create a new FileSystemWatcher and set its properties.
                FileSystemWatcher watcher = new FileSystemWatcher();
                try
                {
                    watcher.Path = watchPath.LocalPath;
                }
                catch (ArgumentException ex)
                {
                    Logger.Error($"Watcher Path not found - Job:{Job.Id} ", ex);
                    Logger.Info($"Attempting to self-heal... issuing create directory - Job:{Job.Id}");
                    Logger.Info($"Attempting to create directory - Job:{Job.Id} Path:{watchPath.LocalPath}");
                    System.IO.Directory.CreateDirectory(watchPath.LocalPath);
                    Logger.Info($"Directory successfully created - Job:{Job.Id}");
                    Logger.Info($"Second attempt to assign watcher.path - Job:{Job.Id}");
                    watcher.Path = watchPath.LocalPath;
                    Logger.Info($"Second attempt successful - Job:{Job.Id}");
                    Logger.Info($"Self-heal successfull - Job:{Job.Id}");
                }

                this.Job = Job;
                RetrieverJobId = Job.Id;

                Console.WriteLine("hszWatcher_instance_started : " + watcher.Path);
                Logger.Info("hszWatcher_instance_started : " + watcher.Path);

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
                    Console.WriteLine("found : " + a);
                    Logger.Info("found : " + a);
                    allFiles.Add(new FileProcess(a));
                }

                using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalToken))
                {
                    //Start directory monitor
                    this.Run(linkedCts.Token);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"error_initilizing_hszwatch : {watchPath}", ex);
            }
        }

        //This is the main method that is run every iteration of the Core.DoWork() method.
        //  It's most likely wise to set it to a few seconds in Core.DoWork() as you might be wasting CPU cycles making it go faster.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        public void Run(CancellationToken token)
        {
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
                            RetrieverJobService _retrieverJobService = container.GetInstance<RetrieverJobService>();

                            _retrieverJobService.RunHszRetrieverJob(Job, token, file.fileName);

                            file.started = true;
                        }
                    }
                    else
                    {
                        Logger.Debug("File: " + file.fileName + " Locked");
                    }
                }

                Thread.Sleep(2000);

            } while (!token.IsCancellationRequested);

            if (token.IsCancellationRequested)
            {
                Logger.Info($"Watch cancelled for Job:{RetrieverJobId.ToString()}");
            }
        }


        #region PrivateFunctions

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
            //  Filter out DatasetLoader Request and Failed Request folders.
            if (Path.GetFileName(e.FullPath).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix")))
            {
                var path = Path.GetFullPath(e.FullPath).Replace(Path.GetFileName(e.FullPath), "");
                var fileName = Path.GetFileName(e.FullPath);
                var origFileName = path + fileName.Substring(Configuration.Config.GetHostSetting("ProcessedFilePrefix").Length);
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
        }

        private void OnError(object source, ErrorEventArgs e)
        {
            Logger.Error($"Watcher Error - Cancelling watch job (JobId:{RetrieverJobId})", e.GetException());

            _internalTokenSource.Cancel();
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
        #endregion
    }
}
