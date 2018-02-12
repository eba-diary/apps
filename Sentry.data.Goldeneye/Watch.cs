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

namespace Sentry.data.Goldeneye
{
    public class Watch
    {
        //Created by Andrew Quaschnick
        //On 11/15/2017

        private static FileSystemWatcher watcher;
        private static List<FileProcess> allFiles = new List<FileProcess>();
        private static IContainer container;
        private static IDatasetContext _datasetContext;


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
        public static void Run(string path)
        {
            var files = allFiles.ToList();

            //Console.WriteLine("Run Process Started for : " + files.Capacity + " files.");

            foreach (FileProcess file in files)
            {
                if(file.started)
                {
                    //The DatasetLoader said Success and the file is gone.
                    if(file.fileCorrectlyDeleted)
                    {
                        //Console.WriteLine("File: " + file.fileName + " Success");
                        Logger.Info("File: " + file.fileName + " Success");
                        allFiles.Remove(file);
                    }
                    //The DatasetLoader said Success but the file still exists.                    
                    else
                    {
                        //Console.WriteLine("File: " + file.fileName + " Still Exists");
                        Logger.Info("File: " + file.fileName + " Still Exists");
                    }
                }
                else if(!IsFileLocked(file.fileName)  && !file.started)
                {
                    //Create non-bundled request
                    if (path == Sentry.Configuration.Config.GetHostSetting("PathToWatch") && file.fileName.Contains(path))
                    {
                        //Console.WriteLine($"Dataset Loader Connection String:{Configuration.Config.GetHostSetting("DatabaseConnectionString")}");
                        using (container = Bootstrapper.Container.GetNestedContainer())
                        {
                            _datasetContext = container.GetInstance<IDatasetContext>();
                            SubmitLoaderRequest(file);
                            file.started = true;
                        }
                    }                    
                }
                else
                {
                    //Console.WriteLine("File: " + file.fileName + " Locked");
                    Logger.Debug("File: " + file.fileName + " Locked");
                }
            }
        }

        //This method checks to see if a file is locked by another process.  
        //  This allows us to see if the DatasetLoader is correctly processing a file or 
        //  if another process is still uploading or writing to a location.
        private static bool IsFileLocked(string filePath)
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

            return false;
        }

        private static void SubmitLoaderRequest(FileProcess file)
        {
            LoaderRequest loadReq = null;
            try
            {
                var orginalPath = Path.GetFullPath(file.fileName).Replace(Path.GetFileName(file.fileName), "");
                var origFileName = Path.GetFileName(file.fileName);
                var processingFile = orginalPath + Configuration.Config.GetHostSetting("ProcessedFilePrefix") + origFileName;
                var fileOwner = Utilities.GetFileOwner(new FileInfo(file.fileName));

                //Rename file to indicate a request has been sent to Dataset Loader
                File.Move(file.fileName, processingFile);

                var hashInput = $"{Sentry.Configuration.Config.GetHostSetting("ServiceAccountID")}_{DateTime.Now.ToString("MM-dd-yyyyHH:mm:ss.fffffff")}_{file.fileName}";
                //Create new loader request object and set file property
                loadReq = new LoaderRequest(GenerateHash(hashInput));
                loadReq.File = processingFile;
                loadReq.IsBundled = false;
                loadReq.RequestInitiatorId = fileOwner;

                Logger.Debug($"Submitting Loader Request - File:{file.fileName} Guid:{loadReq.RequestGuid} HashInput:{hashInput}");

                string jsonReq = JsonConvert.SerializeObject(loadReq, Formatting.Indented);

                //Send request to DFS location loader service is watching for requests
                using (MemoryStream ms = new MemoryStream())
                {
                    StreamWriter writer = new StreamWriter(ms);

                    writer.WriteLine(jsonReq);
                    writer.Flush();

                    //You have to rewind the MemoryStream before copying
                    ms.Seek(0, SeekOrigin.Begin);

                    using (FileStream fs = new FileStream($"{Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath")}{loadReq.RequestGuid}.json", FileMode.OpenOrCreate))
                    {
                        ms.CopyTo(fs);
                        fs.Flush();
                    }
                }

                //Create Bundle Success Event
                Event e = new Event();
                e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault();
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Started").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;                
                e.UserWhoStartedEvent = loadReq.RequestInitiatorId;
                e.Dataset = loadReq.DatasetID;
                e.DataConfig = loadReq.DatasetFileConfigId;
                e.Reason = $"Successfully submitted request to Dataset Loader to upload file [<b>{origFileName}</b>]";
                e.Parent_Event = loadReq.RequestGuid;
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                Logger.Error("Error submitting loader request", ex);

                //Create Bundle Failed Event
                Event e = new Event();
                e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault();
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = loadReq.RequestInitiatorId;
                e.Dataset = loadReq.DatasetID;
                e.DataConfig = loadReq.DatasetFileConfigId;
                e.Reason = "Failed to submit request to Dataset Loader";
                e.Parent_Event = loadReq.RequestGuid;
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            }
            

            

        }

        //This method is called when the Windows Process is started in Core and Program.cs
        //  First it creates a file watcher, gives it a directory from app.config, then assigns it events to watch.
        //  The last thing it does is grabs all the files that currently in the directory and adds them to the file list.
        //  The Service will do the rest on it's periodic run.
        public static void OnStart()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = Sentry.Configuration.Config.GetHostSetting("PathToWatch");

            Console.WriteLine("The Goldeneye File Watcher is now watching : " + watcher.Path);
            Logger.Info("The Goldeneye File Watcher is now watching : " + watcher.Path);

            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            //Get all the files that are currently in the directory on Start to begin monitoring them.
            //  This may happen if the service was killed and needed to restart.
            //  Filter out DatasetLoader Request and Failed Request folders.
            foreach (var a in Directory.GetFiles(watcher.Path, "*", SearchOption.AllDirectories).Where(w => !w.Contains(Configuration.Config.GetHostSetting("LoaderRequestPath")) && !w.Contains(Configuration.Config.GetHostSetting("LoaderFailedRequestPath"))))
            {
                if (!Path.GetFileName(a).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix")))
                {
                    Console.WriteLine("Found : " + a);
                    Logger.Info("Found : " + a);
                    allFiles.Add(new FileProcess(a));
                }
            }
        }

        //When a new file is created add the file path to the list of All Files.
        //  The Service will do the rest on it's periodic run.
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            FileAttributes attr = File.GetAttributes(e.FullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                Logger.Info($"New Directory Added for Monitoring: {e.FullPath}");
            }
            else
            {
                //  Filter out DatasetLoader Request and Failed Request folders.
                if (!Path.GetFileName(e.FullPath).Contains(Configuration.Config.GetHostSetting("LoaderRequestPath")) && !Path.GetFileName(e.FullPath).Contains(Configuration.Config.GetHostSetting("LoaderFailedRequestPath")) && !Path.GetFileName(e.FullPath).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix")))
                {
                    allFiles.Add(new FileProcess(e.FullPath));
                }
            }            
        }

        //We can see when a user is writing to a file.
        //  At this time we don't need this method.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Method intentionally left empty.
        }

        //When a file is deleted find the file in the internal list and mark it deleted.
        //  The Service will do the rest on it's periodic run.
        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            //  Filter out DatasetLoader Request and Failed Request folders.
            if (!Path.GetFileName(e.FullPath).Contains(Configuration.Config.GetHostSetting("LoaderRequestPath")) && !Path.GetFileName(e.FullPath).Contains(Configuration.Config.GetHostSetting("LoaderFailedRequestPath")) && Path.GetFileName(e.FullPath).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix")))
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

        private static Guid GenerateHash(string input)
        {
            string start = input + DateTime.Now.ToString();
            Guid result;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(start));
                result = new Guid(hash);
            }
            return result;
        }

    }
}
