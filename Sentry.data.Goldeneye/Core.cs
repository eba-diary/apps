﻿using Hangfire;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
        private IDatasetContext _requestContext;
        private ITicketMonitorService _ticketMonitorService;
        private Scheduler _backgroundJobServer;
        private List<RunningTask> currentTasks = new List<RunningTask>();

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
            Task.Factory.StartNew(() => DoWork(), _token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Start the core worker
        /// </summary>
        protected class Configuration
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public DateTime TimeLastStarted { get; set; }
            public DateTime LastRunSecond { get; set; }
            public DateTime LastRunMinute { get; set; }
            public DateTime LastRunHour { get; set; }
            public DateTime LastRunDay { get; set; }
            public DateTime LastRunWeek { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// 
        /// </summary>
        protected class RunningTask
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="task"></param>
            /// <param name="name"></param>
            /// <param name="jobId"></param>
            /// <param name="path"></param>
            public RunningTask(Task task, String name, int jobId = 0, Uri path = null)
            {
                this.Task = task;
                this.Name = name;
                this.JobId = jobId;
                this.WatchPath = path;
                this.TimeStarted = DateTime.Now;
            }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public Task Task { get; set; }
            public string Name { get; set; }
            public DateTime TimeStarted { get; set; }
            public int JobId { get; set; }
            public Uri WatchPath { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// Loop until cancellation is requested, doing the primary work of this service
        /// </summary>
        private void DoWork()
        {
            Logger.Info("Worker task started.");

            //Initialize the Bootstrapper
            Bootstrapper.Init();

            var registry = new StructureMap.Registry();

            //adding ThreadCurrentUserIdProvider similar to how Web app adds this to context.
            Bootstrapper.Container.Configure((x) =>
            {
                x.AddRegistry(registry);
                x.For<ICurrentUserIdProvider>().Use<ThreadCurrentUserIdProvider>();
            });
            //Start all the internal processes.

            //Get or Create the Runtime Configuration
            Configuration config = new Configuration();
            config.TimeLastStarted = DateTime.Now;  //The last time Goldeneye was started.

            config.LastRunSecond = DateTime.Now;    //For Logging Purposes.  Don't need logs more then once per second.
            config.LastRunMinute = DateTime.Now;    //Keeps track of the processes that must run once per minute.
            config.LastRunHour = DateTime.Now;      //  once per hour
            config.LastRunDay = DateTime.Now;       //  once per day
            config.LastRunWeek = DateTime.Now;      //  once per week
            
            Boolean firstRun = true;
            do
            {
                using (_container = Bootstrapper.Container.GetNestedContainer())
                {
                    //THIS IS A BANDAID.  No copyright intended.
                    Boolean complete = false;
                    while (!complete)
                    {
                        try
                        {
                            _requestContext = _container.GetInstance<IDatasetContext>();
                            _ticketMonitorService = _container.GetInstance<ITicketMonitorService>();
                            complete = true;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Cannot Get Instance of IRequestContext", ex);
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                    //Run All the Processes that MUST BE run once per SECOND
                    if ((DateTime.Now - config.LastRunSecond).TotalSeconds >= 1 || firstRun)
                    {
                        Console.WriteLine("There are currently " + currentTasks.Count + " processes running. " + currentTasks.Count(x => x.Task.IsCompleted) + " Completed.");
                        foreach (RunningTask rt in currentTasks)
                        {
                            if (rt.Task.IsFaulted)
                            {
                                Console.WriteLine(rt.Name + " faulted.");
                            }
                        }

                        if (firstRun)
                        {
                            _backgroundJobServer = new Scheduler();
                            currentTasks.Add(new RunningTask(
                                Task.Factory.StartNew(() => _backgroundJobServer.Run(_token), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted), "BackgroundJobServer")
                            );

                            //https://crontab.guru/
                            //Schecule SpamFactory:Instance to run every minute
                            // Adding TimeZoneInfo based on https://discuss.hangfire.io/t/need-local-time-instead-of-utc/279/8

                            RecurringJob.AddOrUpdate("spamfactory_instant", () => SpamFactory.Run("Instant"), Cron.Minutely, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            RecurringJob.AddOrUpdate("spamfactory_hourly", () => SpamFactory.Run("Hourly"), "00 * * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            RecurringJob.AddOrUpdate("spamfactory_daily", () => SpamFactory.Run("Daily"), "00 8 * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            RecurringJob.AddOrUpdate("spamfactory_weekly", () => SpamFactory.Run("Weekly"), "00 8 * * MON", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

                            //Schedule Livy Job state monitor to run every minute
                            RecurringJob.AddOrUpdate("LivyJobStateMonitor", () => RetrieverJobService.UpdateJobStatesAsync(), Cron.Minutely, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

                            //Schedule the Ticket Monitor to run based on cron within configuration file.
                            RecurringJob.AddOrUpdate("HPSMTicketMonitor", () => _ticketMonitorService.CheckTicketStatus(), Config.GetHostSetting("HpsmTicketMonitorTimeInterval"), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

                            //Schedule WallEService every day at midnight
                            RecurringJob.AddOrUpdate("WallEService", () => WallEService.Run(), "00 0 * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            //Load all scheduled and enabled jobs into hangfire on startup to ensure all jobs are registered
                            List<RetrieverJob> JobList = _requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && w.IsEnabled).ToList();

                            foreach (RetrieverJob Job in JobList)
                            {
                                try
                                {
                                    // Adding TimeZoneInfo based on https://discuss.hangfire.io/t/need-local-time-instead-of-utc/279/8
                                    RecurringJob.AddOrUpdate<RetrieverJobService>($"{Job.JobName()}", RetrieverJobService => RetrieverJobService.RunRetrieverJob(Job.Id, JobCancellationToken.Null, null), Job.Schedule, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                                    Job.JobLoggerMessage("Info", "Goldeneye initialization, performing AddOrUpdate to Hangfire");
                                }
                                catch (Exception ex)
                                {
                                    Job.JobLoggerMessage("Error", "Failed to AddOrUpdate job on Goldeneye initialization", ex);
                                }
                            }

                            //Remove all jobs that are marked as disabled
                            List<RetrieverJob> DisabledJobList = _requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && !w.IsEnabled).ToList();

                            foreach (RetrieverJob Job in DisabledJobList)
                            {
                                try
                                {
                                    RecurringJob.RemoveIfExists($"{Job.JobName()}");
                                    Job.JobLoggerMessage("Info", "Marked as disabled, performing RemoveIfExists from Hangfire");
                                }
                                catch (Exception ex)
                                {
                                    Job.JobLoggerMessage("Error", "Failed to remove disabled job on Goldeneye initialization", ex);
                                }
                            }

                            //Starting MetadataProcessor Consumer
                            MetadataProcessorService metaProcessor = new MetadataProcessorService();
                            currentTasks.Add(new RunningTask(
                                            Task.Factory.StartNew(() => metaProcessor.Run(), TaskCreationOptions.LongRunning), "metadataProcessor"));
                        }

                        ////Dataset Loader
                        if (true)
                        {
                            //How many loader tasks can be started
                            var availableLoaderTasks = Int32.Parse(Config.GetHostSetting("activeLoaderTaskThrottle")) - currentTasks.Where(x => x.Name.StartsWith("DatasetLoader : Minute") && !x.Task.IsCompleted).ToList().Count;
                            foreach (var a in Directory.GetFiles(Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath"), "*", SearchOption.AllDirectories).Where(w => !IsFileLocked(w)))
                            {
                                if (availableLoaderTasks > 0)
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

                                        //Remove one available loader task
                                        availableLoaderTasks--;
                                    }
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
                        //Reload and modifed\new jobs
                        List<RetrieverJob> JobList = _requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && w.IsEnabled && (w.Created > config.LastRunMinute.AddSeconds(-5) || w.Modified > config.LastRunMinute.AddSeconds(-5))).ToList();

                        foreach (RetrieverJob Job in JobList)
                        {
                            // Adding TimeZoneInfo based on https://discuss.hangfire.io/t/need-local-time-instead-of-utc/279/8
                            RecurringJob.AddOrUpdate<RetrieverJobService>($"{Job.JobName()}", RetrieverJobService => RetrieverJobService.RunRetrieverJob(Job.Id, JobCancellationToken.Null, null), Job.Schedule, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                            Job.JobLoggerMessage("Info", "Job update detected, performing AddOrUpdate to Hangfire");
                        }


                        if (JobList.Count > 0)
                        {
                            string jobIds = null;
                            int cnt = 0;
                            foreach (RetrieverJob a in JobList)
                            {
                                if (cnt > 0)
                                {
                                    jobIds += " | " + a.Id.ToString();
                                }
                                else
                                {
                                    jobIds += a.Id.ToString();
                                }
                                cnt++;
                            }

                            Console.WriteLine($"Detected {JobList.Count} new or modified jobs to be loaded into hangfire : JobIds:{jobIds}");
                            Logger.Info($"Detected {JobList.Count} new or modified jobs to be loaded into hangfire : JobIds:{jobIds}");
                        }

                        //Remove disabled jobs
                        List<RetrieverJob> DisabledJobList = _requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && !w.IsEnabled && (w.Created > config.LastRunMinute.AddSeconds(-5) || w.Modified > config.LastRunMinute.AddSeconds(-5))).ToList();

                        foreach (RetrieverJob Job in DisabledJobList)
                        {
                            // Adding TimeZoneInfo based on https://discuss.hangfire.io/t/need-local-time-instead-of-utc/279/8
                            RecurringJob.RemoveIfExists($"{Job.JobName()}");
                            Job.JobLoggerMessage("Info", "Job update detected, performing RemoveIfExists from Hangfire");
                        }

                        /**************************************
                         *  DFS Directory Monitors                          
                        ***************************************/                     
                        //Get all active watch retriever jobs 
                        List<RetrieverJob> rtJobList = _requestContext.RetrieverJob.Where(w => (w.DataSource is DfsBasic || w.DataSource is DfsCustom || w.DataSource is DfsDataFlowBasic) && w.Schedule == "Instant" && w.IsEnabled).FetchAllConfiguration(_requestContext).ToList();

                        //If initial start of GOLDENEYE, init all jobs else only new jobs
                        List<RetrieverJob> initJobList = (firstRun) ? rtJobList : rtJobList.Where(s => !currentTasks.Any(ct => ct.JobId == s.Id)).ToList();                        

                        //Initilize filewatcher jobs
                        foreach (RetrieverJob rJob in initJobList)
                        {
                            try
                            {
                                var jobId = rJob.Id;
                                Uri path = rJob.GetUri();

                                Task t = InitializeFileWatcherTask(jobId, path);

                                currentTasks.Add(new RunningTask(t, GenerateWatcherName(rJob), jobId, path));
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Initializing retrieverjob jobId:{rJob.Id}", ex);
                            }
                        }

                        config.LastRunMinute = DateTime.Now;
                    }
                }

                firstRun = false;

            } while (!_token.IsCancellationRequested);

            if (_token.IsCancellationRequested)
            {
                Logger.Info("Cancellation Requested, shutting down Goldeneye");
            };

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

            try
            {
                Task.WaitAll(currentTasks.Select(s => s.Task).ToArray());
            }
            catch (AggregateException e)
            {
                Logger.Debug("AggregateException thrown with inner exceptions:");
                // Display information about each exception. 
                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                    {
                        Logger.Debug($"TaskCanceledException: Task {((TaskCanceledException)v).Task.Id.ToString()}");
                    }
                    else
                    {
                        Logger.Error($"   Exception: {v.GetType().Name}");
                    }
                }
            }
            finally
            {
                _tokenSource.Dispose();
            }

            foreach (RunningTask task in currentTasks)
            {
                Logger.Info($"Task {task.Name} status is now {task.Task.Status}");
            }

            Logger.Info("Windows Service stopped.");
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

        /// <summary>
        /// An exception occurred on the main task; log it and shut down the service with an error exit code
        /// </summary>
        /// <param name="t">The main thread task</param>
        /// <param name="jobId"></param>
        private void WatcherTaskRestart(Task t, int jobId)
        {
            //If service is shutting down, we do not want to restart the task
            if (!_token.IsCancellationRequested)
            {
                //Find associated RunningTask object
                RunningTask curTask = currentTasks.FirstOrDefault(w => w.JobId == jobId);

                if (curTask == null)
                {
                    Logger.Debug($"core-watchertaskrestart no-associated-runningtask-entry - JobId:{jobId}");
                }
                else
                {
                    Logger.Debug($"core-watchertaskrestart restart - JobId:{jobId} Path:{curTask.WatchPath}");

                    //Create new task
                    Task newTask = InitializeFileWatcherTask(curTask.JobId, curTask.WatchPath);

                    //Associate new task with RunningTask object for furture tracking
                    curTask.Task = newTask;
                }

                Logger.Debug($"core-watchertaskrestart disposing-completed-task - JobId:{jobId}");
                t.Dispose();
                Logger.Debug($"core-watchertaskrestart disposing-completed-task-success - JobId:{jobId}");
            }
        }

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
            catch(UnauthorizedAccessException e)
            { 
                return false;
            }
            catch(Exception e)
            {
                Logger.Error("File Locked Test Try/Catch Failed", e);
            }

            return false;
        }

        private string GenerateWatcherName(RetrieverJob job)
        {
            return (job.DataFlow != null) ? $"Watch_{job.Id}_df_{job.DataFlow.Id}" : $"Watch_{job.Id}_config_{job.DatasetConfig.ConfigId}";
        }

        private Task InitializeFileWatcherTask(int jobId, Uri path)
        {
            Logger.Info($"core-initializefilewatcher start - JobId:{jobId} Path:{path}");
            Task newTask = Task.Factory.StartNew(() => (new Watch()).OnStart(jobId, path, _token, Int32.Parse(Config.GetHostSetting("FileWatcherIterationLimit"))),
                                                    TaskCreationOptions.LongRunning).
                            ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted).
                            ContinueWith(task => WatcherTaskRestart(task, jobId), TaskContinuationOptions.OnlyOnCanceled);

            Logger.Info($"core-initializefilewatcher end - JobId:{jobId} Path:{path}");
            return newTask;
        }
    }
}