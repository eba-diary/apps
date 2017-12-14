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

namespace Sentry.data.Goldeneye
{
    class Watch
    {
        //Created by Andrew Quaschnick
        //On 11/15/2017

        private static FileSystemWatcher watcher;
        private static List<FileProcess> allFiles = new List<FileProcess>();

        private class FileProcess {

            public FileProcess(string fileName)
            {
                this.fileName = fileName;
                this.started = false;
                this.fileCorrectlyDeleted = false;
            }

            public string fileName { get; set; }
            public Process process { get; set; }
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
                if(file.started && file.process != null && file.process.HasExited)
                {
                    //The DatasetLoader said Success and the file is gone.
                    if(file.process.ExitCode == 0 && file.fileCorrectlyDeleted)
                    {
                        Console.WriteLine("File: " + file.fileName + " Success");
                        Logger.Info("File: " + file.fileName + " Success");
                        allFiles.Remove(file);
                    }
                    //The DatasetLoader said Success but the file still exists.
                    else if(file.process.ExitCode == 0)
                    {
                        Console.WriteLine("File: " + file.fileName + " Still Exists");
                        Logger.Info("File: " + file.fileName + " Still Exists");
                    }
                    //The DatasetLoader Failed
                    else
                    {
                        Console.WriteLine("File: " + file.fileName + " Failed " + file.process.ExitCode);
                        Logger.Info("File: " + file.fileName + " Failed " + file.process.ExitCode);
                    }
                }
                else if(!IsFileLocked(file.fileName)  && !file.started)
                {
                    StartLoader(file);
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

        //This method starts the DatasetLoader given a specific file for it to load.
        private static void StartLoader(FileProcess file)
        {
            string datasetLoaderLocation = Sentry.Configuration.Config.GetHostSetting("DatasetLoaderLocation");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = datasetLoaderLocation;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-p \"{file.fileName}\"";

            try
            {
                // Start the process with the info we specified.
                file.process = Process.Start(startInfo);
                file.started = true;

                Console.WriteLine("File: " + file.fileName + " was sent to the Dataset Loader : " + file.process.StartTime);
                Logger.Info("File: " + file.fileName + " was sent to the Dataset Loader : " + file.process.StartTime);
            }
            catch (Exception ex)
            {
                // Log error.
                Console.WriteLine("File: " + file.fileName + " was NOT sent to the Dataset Loader : " + ex.Message);
                Logger.Error("File: " + file.fileName + " was NOT sent to the Dataset Loader", ex);
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
            FileAttributes attr = File.GetAttributes(e.FullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                Logger.Info($"New Directory Added for Monitoring: {e.FullPath}");
            }
            else
            {
                allFiles.Add(new FileProcess(e.FullPath));
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
            var file = allFiles.FirstOrDefault(x => x.fileName == e.FullPath);
            file.fileCorrectlyDeleted = true;
        }
    }
}
