using System.Threading;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Threading.Tasks;
using System;
using System.Linq;
using Sentry.data.Infrastructure;
using System.Collections.Generic;
using Sentry.data.Core;
using System.IO;
using Sentry.Configuration;
using Newtonsoft.Json;
using StructureMap;

namespace Sentry.data.Goldeneye
{
    /// <summary>
    /// This class represents where you should run your application logic
    /// </summary>
    public class Core
    {

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private IContainer _container;
        private IDatasetContext _datasetContext;

        /// <summary>
        /// Start the core worker
        /// </summary>
        public void OnStart()
        {
            Logger.Info("Windows Service starting...");

            //setup a token to allow a task to be cancelled
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            //start up a new task
            Task.Factory.StartNew(this.DoWork, TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Start the core worker
        /// </summary>
        protected class Configuration
        {
            public DateTime TimeLastStarted { get; set; }
            public DateTime LastRunSecond { get; set; }
            public DateTime LastRunMinute { get; set; }
            public DateTime LastRunHour { get; set; }
            public DateTime LastRunDay { get; set; }
            public DateTime LastRunWeek { get; set; }
        }

        protected class RunningTask
        {
            public RunningTask(Task task, String name)
            {
                this.Task = task;
                this.Name = name;
                this.TimeStarted = DateTime.Now;
            }

            //public Task Task { get; set; }
            public Task Task { get; set; }
            public string Name { get; set; }
            public DateTime TimeStarted { get; set; }
        }

        /// <summary>
        /// Loop until cancellation is requested, doing the primary work of this service
        /// </summary>
        private void DoWork()
        {
            Logger.Info("Worker task started.");

            //Initialize the Bootstrapper
            Bootstrapper.Init();

            //Start all the internal processes.
            
            //Watch.OnStart(Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath"));


            //Get or Create the Runtime Configuration
            Configuration config = new Configuration();

            if(true) //There is no runtime configuration in the directory
            {
                config.TimeLastStarted = DateTime.Now;  //The last time Goldeneye was started.

                config.LastRunSecond = DateTime.Now;    //For Logging Purposes.  Don't need logs more then once per second.
                config.LastRunMinute = DateTime.Now;    //Keeps track of the processes that must run once per minute.
                config.LastRunHour = DateTime.Now;      //  once per hour
                config.LastRunDay = DateTime.Now;       //  once per day
                config.LastRunWeek = DateTime.Now;      //  once per week
            }

            List<RunningTask> currentTasks = new List<RunningTask>();
            List<Request> requests = new List<Request>();

            Boolean firstRun = true;
            do
            {
                //Run All the Processes that MUST BE run once per SECOND
                if ((DateTime.Now - config.LastRunSecond).TotalSeconds >= 1 || firstRun)
                {
                    Console.WriteLine("There are currently " + currentTasks.Count + " processes running. "  + currentTasks.Count(x => x.Task.IsCompleted) + " Completed.");
                    foreach (RunningTask rt in currentTasks)
                    {
                        Console.WriteLine("Name: " + rt.Name  + " - Done : " + rt.Task.IsCompleted  + " - Time Elapsed:" + (DateTime.Now - rt.TimeStarted).TotalSeconds.ToString("0.00") + " seconds");

                        if(rt.Task.IsFaulted)
                        {
                            Console.WriteLine(rt.Name + " faulted.");
                        }
                    }

                    //Cleanup Completed or Faulted Dataset Loader Tasks
                    if (currentTasks.Where(x => x.Name.StartsWith("DatasetLoader : Minute") && (x.Task.IsCompleted || x.Task.IsFaulted)).ToList().Count >= 1 || firstRun)
                    {
                        if (firstRun)
                        {
                            //Remove all DatasetLoader tasks
                            var tasks = currentTasks.Where(x => x.Name.StartsWith("DatasetLoader : Minute")).ToList();
                            tasks.ForEach(x => x.Task.Dispose());
                            currentTasks.RemoveAll(x => x.Name.StartsWith("DatasetLoader : Minute"));
                        }
                        else
                        {
                            //If it's completed dispose of it.
                            var tasks = currentTasks.Where(x => x.Name.StartsWith("DatasetLoader : Minute") && (x.Task.IsCompleted || x.Task.IsFaulted)).ToList();
                            tasks.ForEach(x => x.Task.Dispose());
                            foreach (RunningTask task in tasks)
                            {
                                currentTasks.RemoveAll(x => x.Name == task.Name);
                            }
                        }

                    }

                    ////Dataset Loader
                    if (true)
                    {
                        //How many loader tasks can be started
                        var availableLoaderTasks = Int32.Parse(Config.GetHostSetting("activeLoaderTaskThrottle")) - currentTasks.Where(x => x.Name.StartsWith("DatasetLoader : Minute") && !x.Task.IsCompleted).ToList().Count;
                        foreach (var a in Directory.GetFiles(Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath"), "*", SearchOption.AllDirectories))
                        {
                            if(availableLoaderTasks > 0)
                            {
                                //Disregard any request files picked up via Dataset Loader
                                //This will need to change when Reading requests from SDP
                                if (!Path.GetFileName(a).StartsWith(Sentry.Configuration.Config.GetHostSetting("ProcessedFilePrefix")))
                                {
                                    Console.WriteLine("Found : " + a);
                                    Logger.Info("Found : " + a);

                                    //Add Processing Prefix to file name so it is not picked up by another DatasetLoader task
                                    var orginalPath = Path.GetFullPath(a).Replace(Path.GetFileName(a), "");
                                    var origFileName = Path.GetFileName(a);
                                    var processingFile = orginalPath + Sentry.Configuration.Config.GetHostSetting("ProcessedFilePrefix") + origFileName;


                                    //Rename file to indicate request has been sent for processing
                                    File.Move(a, processingFile);

                                    //Create a new one.
                                    currentTasks.Add(new RunningTask(
                                        Task.Factory.StartNew(() => DatasetLoader.Run(processingFile), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted),
                                        $"DatasetLoader : Minute : {Path.GetFileNameWithoutExtension(a)}"));
                                }
                                availableLoaderTasks--;
                            }
                            else
                            {
                                //Max number of loader tasks has been reached.
                                break;
                            }                            
                        }
                    }

                    config.LastRunSecond = DateTime.Now;
                }



                //Run All the Processes that MUST BE run once per MINUTE
                if ((DateTime.Now - config.LastRunMinute).TotalMinutes >= 1 || firstRun)
                {

                    //DatasetLoader Back Pressure
                    if (true)
                    {
                        int files = Directory.GetFiles(Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath"), "*", SearchOption.AllDirectories).Where(w => !w.Contains(Config.GetHostSetting("LoaderRequestPath")+ Config.GetHostSetting("ProcessedFilePrefix"))).Count();
                        Console.WriteLine($"Dataset Loader Back Pressure: {files}");
                        Logger.Info($"Dataset Loader Back Pressure: {files}");
                    }

                    //Spam Factory
                    //  We don't want to add another Spam Factory : Instant if the old one is still running.  i.e. Race Conditions sending Emails.
                    if (currentTasks.Any(x => x.Task.IsCompleted && x.Name == "Spam Factory : Instant") || firstRun)
                    {
                        //If it's completed dispose of it.
                        var tasks = currentTasks.Where(x => x.Name == "Spam Factory : Instant").ToList();
                        tasks.ForEach(x => x.Task.Dispose());
                        currentTasks.RemoveAll(x => x.Name == "Spam Factory : Instant");

                        //Create a new one.
                        currentTasks.Add(new RunningTask(
                            Task.Factory.StartNew(() => SpamFactory.Run("Instant"), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted),
                            "Spam Factory : Instant")
                        );
                    }

                    ////Directory Monitoring (Original Goldeneye)
                    ////  We don't want to add another directory monitor if the old one is still running.
                    //if (currentTasks.Where(x => x.Name.StartsWith("Watch") && x.Task.IsCompleted).Count() > 0 || firstRun)
                    //{
                    //    //If it's completed dispose of it.
                    //    var tasks = currentTasks.Where(x => x.Name.StartsWith("Watch") && x.Task.IsCompleted).ToList();
                    //    tasks.ForEach(x => x.Task.Dispose());
                    //    tasks.ForEach(x => currentTasks.RemoveAll(ct => ct.Name == x.Name));

                    //    List<DatasetFileConfig> dsconfig = null;

                    //    using (_container = Bootstrapper.Container.GetNestedContainer())
                    //    {
                    //        _datasetContext = _container.GetInstance<IDatasetContext>();
                    //        dsconfig = _datasetContext.getAllDatasetFileConfigs().Where(w => w.DropLocationType == "DFS").OrderBy(o => o.ConfigId).ToList();
                    //    }

                    //    foreach (DatasetFileConfig dsfc in dsconfig)
                    //    {
                    //        if (firstRun || tasks.Any(w => Int64.Parse(w.Name.Replace("Watch_", "")) == dsfc.ConfigId))
                    //        {
                    //            currentTasks.Add(new RunningTask(
                    //                Task.Factory.StartNew(() => (new Watch()).OnStart(dsfc.ConfigId),
                    //                                                TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                    //                                                TaskContinuationOptions.OnlyOnFaulted),
                    //                                                $"Watch_{dsfc.ConfigId}")
                    //            );

                    //            if (!firstRun) { Console.WriteLine($"Restarting Watch_{dsfc.ConfigId}"); }
                    //        }
                    //    }
                    //}

                    //Dataset File Config Watch
                    if (true)
                    {
                        //If it's completed dispose of it.
                        var tasks = currentTasks.Where(x => x.Name.StartsWith("Watch") && x.Task.IsCompleted).ToList();
                        tasks.ForEach(x => x.Task.Dispose());
                        tasks.ForEach(x => currentTasks.RemoveAll(ct => ct.Name == x.Name));

                        List<DatasetFileConfig> dsconfig = null;

                        using (_container = Bootstrapper.Container.GetNestedContainer())
                        {
                            _datasetContext = _container.GetInstance<IDatasetContext>();
                            dsconfig = _datasetContext.getAllDatasetFileConfigs().Where(w => w.DropLocationType == "DFS").OrderBy(o => o.ConfigId).ToList();
                        }

                        foreach (DatasetFileConfig dsfc in dsconfig)
                        {
                            //On initial run start all watch tasks for all configs
                            if (firstRun)
                            {
                                currentTasks.Add(new RunningTask(
                                                                    Task.Factory.StartNew(() => (new Watch()).OnStart(dsfc.ConfigId),
                                                                                                    TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                                                    TaskContinuationOptions.OnlyOnFaulted),
                                                                                                    $"Watch_{dsfc.ConfigId}")
                                                                );
                            }
                            //Restart any completed tasks
                            else if (tasks.Any(w => Int64.Parse(w.Name.Replace("Watch_", "")) == dsfc.ConfigId))
                            {
                                currentTasks.Add(new RunningTask(
                                    Task.Factory.StartNew(() => (new Watch()).OnStart(dsfc.ConfigId),
                                                                    TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                    TaskContinuationOptions.OnlyOnFaulted),
                                                                    $"Watch_{dsfc.ConfigId}")
                                );

                                Logger.Info($"Resstarting Watch_{ dsfc.ConfigId}");
                            }
                            //Start any new directories added
                            else if (!currentTasks.Any(x => x.Name.StartsWith("Watch") && Int64.Parse(x.Name.Replace("Watch_", "")) == dsfc.ConfigId))
                            {
                                Logger.Info($"Detected new config ({dsfc.ConfigId}) to monitor ({dsfc.DropPath})");

                                currentTasks.Add(new RunningTask(
                                    Task.Factory.StartNew(() => (new Watch()).OnStart(dsfc.ConfigId),
                                                                    TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                    TaskContinuationOptions.OnlyOnFaulted),
                                                                    $"Watch_{dsfc.ConfigId}")
                                );
                            }
                        }
                    }

                    config.LastRunMinute = DateTime.Now;                    
                }
                
                //HOURLY Processing
                if ((DateTime.Now - config.LastRunHour).TotalHours >= 1 || firstRun)
                {
                    //Create a new one.
                    currentTasks.Add(new RunningTask(
                        Task.Factory.StartNew(() => SpamFactory.Run("Hourly"), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted),
                        "Spam Factory : Hourly")
                    );

                    config.LastRunHour = DateTime.Now;
                }

                //DAILY Processing
                if ((DateTime.Now - config.LastRunDay).TotalHours >= 24 || firstRun)
                {
                    //Create a new one.
                    currentTasks.Add(new RunningTask(
                        Task.Factory.StartNew(() => SpamFactory.Run("Daily"), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted),
                        "Spam Factory : Daily")
                    );

                    config.LastRunDay = DateTime.Now;
                }

                //WEEKLY Processing
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && (DateTime.Now - config.LastRunWeek).TotalDays >= 7 || firstRun)
                {
                    //Create a new one.
                    currentTasks.Add(new RunningTask(
                        Task.Factory.StartNew(() => SpamFactory.Run("Weekly"), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted),
                        "Spam Factory : Weekly")
                    );

                    config.LastRunWeek = DateTime.Now;
                }

                firstRun = false;

            } while (!_token.IsCancellationRequested);
            Logger.Info("Worker task stopped.");
        }

        /// <summary>
        /// Request the core worker to stop
        /// </summary>
        public void OnStop()
        {
            Logger.Info("Windows Service stopping...");
            //request the task to cancel itself
            _tokenSource.Cancel();
        }

        /// <summary>
        /// An exception occurred on the main task; log it and shut down the service with an error exit code
        /// </summary>
        /// <param name="t">The main thread task</param>
        private void TaskException(Task t)
        {
            Logger.Fatal("Exception occurred on main Windows Service Task. Stopping Service immediately.", t.Exception);
            Environment.Exit(10001);
        }

    }
}