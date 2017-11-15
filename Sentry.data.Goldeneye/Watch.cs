using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

namespace Sentry.data.Goldeneye
{
    class Watch
    {
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




        public static void Run()
        {
            var files = allFiles.ToList();

            foreach (FileProcess file in files)
            {
                if(file.started && file.process != null && file.process.HasExited)
                {
                    //The DatasetLoader said Success and the file is gone.
                    if(file.process.ExitCode == 0 && file.fileCorrectlyDeleted)
                    {
                        Logger.Info("File: " + file.fileName + " Success");
                        allFiles.Remove(file);
                    }
                    //The DatasetLoader said Success but the file still exists.
                    else if(file.process.ExitCode == 0)
                    {
                        Logger.Info("File: " + file.fileName + " Still Exists");
                    }
                    //The DatasetLoader Failed
                    else
                    {
                        Logger.Info("File: " + file.fileName + " Failed " + file.process.ExitCode);
                    }
                }
                else if(!IsFileLocked(file.fileName)  && !file.started)
                {
                    string datasetLoaderLocation = @"C:\TFS\Sentry.Data\Mainline\Sentry.data.DatasetLoader\bin\Debug\Sentry.data.DatasetLoader.exe";

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

                        Logger.Info("File: " + file.fileName + " was sent to the Dataset Loader : " + file.process.StartTime);
                    }
                    catch(Exception ex)
                    {
                        // Log error.
                        Logger.Error("File: " + file.fileName + " was NOT sent to the Dataset Loader : " + ex.Message);
                    }
                }
                else
                {
                    Logger.Debug("File: " + file.fileName + " Locked");
                }
            }
        }
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

        public static void OnStart()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = @"C:\tmp\DatasetLoader";

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
                allFiles.Add(new FileProcess(a));
            }
        }

        // Define the event handlers.
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
          //  Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            allFiles.Add(new FileProcess(e.FullPath));
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
          //  Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
           // Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

            var file = allFiles.Where(x => x.fileName == e.FullPath).FirstOrDefault();
            file.fileCorrectlyDeleted = true;
        }
    }
}
