using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;
using System.Configuration;
using Sentry.data.Infrastructure;
using StructureMap;
using Sentry.data.Core;

namespace Sentry.data.Bundler
{
    class Watch
    {
        //Created by Andrew Quaschnick
        //On 11/15/2017

        private static FileSystemWatcher watcher;
        private static List<FileProcess> allFiles = new List<FileProcess>();

        public static IContainer _container;
        public static IDatasetContext _datasetContext;
        public static IS3ServiceProvider _datasetService;

        private class FileProcess {

            public FileProcess(string fileName)
            {
                this.fileName = fileName;
                this.started = false;
                this.fileCorrectlyDeleted = false;
            }

            public string fileName { get; set; }
            public Task task { get; set; }
            public Boolean started { get; set; }
            public Boolean fileCorrectlyDeleted { get; set; }
        }

        //This is the main method that is run every iteration of the Core.DoWork() method.
        //  It's most likely wise to set it to a few seconds in Core.DoWork() as you might be wasting CPU cycles making it go faster.
        public static void Run()
        {
            var files = allFiles.ToList();


            Console.WriteLine("Run Process Started for : " + files.Capacity + " files.");

            foreach (FileProcess file in files)
            {
                if (file.started && file.task != null && file.task.IsCompleted)
                {
                    //The DatasetLoader said Success and the file is gone.
                    if(file.task.Status == TaskStatus.RanToCompletion && file.fileCorrectlyDeleted)
                    {
                        Console.WriteLine("File: " + file.fileName + " Success");
                        Logger.Info("File: " + file.fileName + " Success");
                        allFiles.Remove(file);
                    }
                    //The DatasetLoader said Success but the file still exists.
                    else if(file.task.Status == TaskStatus.RanToCompletion)
                    {
                        Console.WriteLine("File: " + file.fileName + " Still Exists");
                        Logger.Info("File: " + file.fileName + " Still Exists");
                    }
                    //The DatasetLoader Failed
                    else
                    {
                        Console.WriteLine("File: " + file.fileName + " Failed " + file.task.Status);
                        Logger.Info("File: " + file.fileName + " Failed " + file.task.Status);
                    }
                }
                else if(!IsFileLocked(file.fileName)  && !file.started)
                {
                    Logger.Info($"Initializing Bundle Task for: {Path.GetFileName(file.fileName)}");   

                    file.task = Task.Run(() =>
                    {
                        Logger.Info($"Beginning Bundle Task for: {Path.GetFileName(file.fileName)}");
                        try
                        {
                            //create an IOC (structuremap) container to wrap this transaction
                            using (_container = Bootstrapper.Container.GetNestedContainer())
                            {
                                _datasetContext = _container.GetInstance<IDatasetContext>();
                                _datasetService = _container.GetInstance<IS3ServiceProvider>();
                                Bundle bundleProcess = _container.GetInstance<Bundle>();

                                bundleProcess.RequestFilePath = file.fileName;

                                try
                                {
                                    bundleProcess.KeyContatenation();
                                    Console.WriteLine($"Bundle Task Successful for request: {Path.GetFileName(file.fileName)}");
                                    Logger.Info($"Ended Bundle Task for request: {Path.GetFileName(file.fileName)}");
                                }
                                catch (Exception e)
                                {
                                    Logger.Error($"Bundle Task Failed for request: {Path.GetFileName(file.fileName)}", e);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Problem initializing bundle task for: {Path.GetFileName(file.fileName)} ", ex);
                        }
                    });
                    
                    file.started = true;                    
                }
                else
                {
                    Console.WriteLine("File: " + file.fileName + " Locked");
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
        

        //This method is called when the Windows Process is started in Core and Program.cs
        //  First it creates a file watcher, gives it a directory from app.config, then assigns it events to watch.
        //  The last thing it does is grabs all the files that currently in the directory and adds them to the file list.
        //  The Service will do the rest on it's periodic run.
        public static void OnStart()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = Sentry.Configuration.Config.GetHostSetting("BundleRequestDir");

            Console.WriteLine("The Bundler File Watcher is now watching : " + watcher.Path);
            Logger.Info("The Bundler File Watcher is now watching : " + watcher.Path);

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
            foreach(var a in Directory.GetFiles(watcher.Path, "*", SearchOption.AllDirectories))
            {
                Console.WriteLine("Found : " + a);
                Logger.Info("Found : " + a);
                allFiles.Add(new FileProcess(a));
            }
        }

        //When a new file is created add the file path to the list of All Files.
        //  The Service will do the rest on it's periodic run.
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            allFiles.Add(new FileProcess(e.FullPath));
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
            var file = allFiles.FirstOrDefault(x => x.fileName == e.FullPath);
            file.fileCorrectlyDeleted = true;
        }

    }
}
