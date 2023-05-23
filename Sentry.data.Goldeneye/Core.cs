using Hangfire;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Scheduler _backgroundJobServer;
        private List<RunningTask> currentTasks = new List<RunningTask>();
        private Configuration _config;

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
            _config = new Configuration
            {
                TimeLastStarted = DateTime.Now,  //The last time Goldeneye was started.

                LastRunSecond = DateTime.Now,    //For Logging Purposes.  Don't need logs more then once per second.
                LastRunMinute = DateTime.Now,    //Keeps track of the processes that must run once per minute.
                LastRunHour = DateTime.Now,      //  once per hour
                LastRunDay = DateTime.Now,       //  once per day
                LastRunWeek = DateTime.Now      //  once per week
            };

            Boolean firstRun = true;
            do
            {             

                if (firstRun)
                {
                    InitializeHangFireServer();
                    InitializeStandardHangfireJobs();
                    InitializeRetrieverJobsInHangFire();
                    InitializeEventProcessor();
                }

                //Run All the Processes that MUST BE run once per SECOND
                if ((DateTime.Now - _config.LastRunSecond).TotalSeconds >= 1 || firstRun)
                {
                    RunSecondlyProcessing();
                }

                //Run All the Processes that MUST BE run once per MINUTE
                if ((DateTime.Now - _config.LastRunMinute).TotalMinutes >= 1 || firstRun)
                {
                    RunMinutelyProcessing();
                }

                firstRun = false;

                Thread.Sleep(100);

            } while (!_token.IsCancellationRequested);

            if (_token.IsCancellationRequested)
            {
                Logger.Info("Cancellation Requested, shutting down Goldeneye");
            };

            Logger.Info("Worker task stopped.");
        }

        private void RunMinutelyProcessing()
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext requestContext = container.GetInstance<IDatasetContext>();

                //TODO: CLA-2888 - Add ObjectStatus filtering logic
                //Reload and modifed\new jobs
                List<RetrieverJob> JobList = requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && w.IsEnabled && (w.Created > _config.LastRunMinute.AddSeconds(-5) || w.Modified > _config.LastRunMinute.AddSeconds(-5))).ToList();

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

                //TODO: CLA-2888 - Add ObjectStatus filtering logic
                //Remove disabled jobs, non-instant jobs
                List<RetrieverJob> DisabledJobList = requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && !w.IsEnabled && (w.Created > _config.LastRunMinute.AddSeconds(-5) || w.Modified > _config.LastRunMinute.AddSeconds(-5))).ToList();

                foreach (RetrieverJob Job in DisabledJobList)
                {
                    // Adding TimeZoneInfo based on https://discuss.hangfire.io/t/need-local-time-instead-of-utc/279/8
                    RecurringJob.RemoveIfExists($"{Job.JobName()}");
                    Job.JobLoggerMessage("Info", "Job update detected, performing RemoveIfExists from Hangfire");
                }

                _config.LastRunMinute = DateTime.Now;
            }
        }

        private void RunSecondlyProcessing()
        {
            Console.WriteLine("There are currently " + currentTasks.Count + " processes running. " + currentTasks.Count(x => x.Task.IsCompleted) + " Completed.");
            foreach (RunningTask rt in currentTasks)
            {
                if (rt.Task.IsFaulted)
                {
                    Console.WriteLine(rt.Name + " faulted.");
                }
            }

            _config.LastRunSecond = DateTime.Now;
        }

        private void InitializeEventProcessor()
        {
            //Starting MetadataProcessor Consumer
            Logger.Info("starting metadataprocessorservice");
            MetadataProcessorService metaProcessor = Bootstrapper.Container.GetInstance<MetadataProcessorService>();
            currentTasks.Add(new RunningTask(
                            Task.Factory.StartNew(() => metaProcessor.Run(), TaskCreationOptions.LongRunning), "metadataProcessor"));
        }

        private void InitializeRetrieverJobsInHangFire()
        {
            

            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext requestContext = container.GetInstance<IDatasetContext>();

                //Load all scheduled and enabled jobs into hangfire on startup to ensure all jobs are registered
                List<RetrieverJob> JobList = requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && w.IsEnabled).ToList();

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
                List<RetrieverJob> DisabledJobList = requestContext.RetrieverJob.Where(w => w.Schedule != null && w.Schedule != "Instant" && !w.IsEnabled).ToList();

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
            }
        }

        private void InitializeStandardHangfireJobs()
        {
            //https://crontab.guru/
            //Schecule SpamFactory:Instance to run every minute
            // Adding TimeZoneInfo based on https://discuss.hangfire.io/t/need-local-time-instead-of-utc/279/8

            RecurringJob.AddOrUpdate("spamfactory_instant", () => SpamFactory.Run("Instant"), Cron.Minutely, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            RecurringJob.AddOrUpdate("spamfactory_hourly", () => SpamFactory.Run("Hourly"), "00 * * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            RecurringJob.AddOrUpdate("spamfactory_daily", () => SpamFactory.Run("Daily"), "00 8 * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            RecurringJob.AddOrUpdate("spamfactory_weekly", () => SpamFactory.Run("Weekly"), "00 8 * * MON", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

            //Schedule Livy Job state monitor to run every minute
            RecurringJob.AddOrUpdate<IRetrieverJobService>("LivyJobStateMonitor", x => x.UpdateJobStatesAsync(), Cron.Minutely, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

            //Schedule the Ticket Monitor to run based on cron within configuration file.
            RecurringJob.AddOrUpdate<TicketMonitorService>("HPSMTicketMonitor", x => x.CheckTicketStatus(), Config.GetHostSetting("HpsmTicketMonitorTimeInterval"), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

            //Schedule the Metadata monitor to monitor consumption layer statuses
            RecurringJob.AddOrUpdate<MetadataMonitorService>("MetadataMonitor", x => x.CheckConsumptionLayerStatus(), Config.GetHostSetting("MetadataMonitorTimeInterval"), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

            //Recurring job to consume infrastructure events
            RecurringJob.AddOrUpdate<InevService>("DBA_Inev_Consumer", x => x.CheckDbaPortalEvents(), Config.GetHostSetting("InevDBAConsumerMonitorTimeInterval"), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            
            //Schedule WallEService every day at midnight
            RecurringJob.AddOrUpdate("WallEService", () => WallEService.Run(), "00 0 * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
        }

        private void InitializeHangFireServer()
        {
            _backgroundJobServer = new Scheduler();
            currentTasks.Add(new RunningTask(
                Task.Factory.StartNew(() => _backgroundJobServer.Run(_token), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted), "BackgroundJobServer")
            );
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
    }
}