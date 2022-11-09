using System.Threading;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Threading.Tasks;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using System;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;

namespace HSZGOLDENEYE
{
    /// <summary>
    /// This class represents where you should run your application logic
    /// </summary>
    public class Core
    {

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private IContainer _container;
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
            Task.Factory.StartNew(this.DoWork, TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Loop until cancellation is requested, doing the primary work of this service
        /// </summary>
        private void DoWork()
        {
            Logger.Info("HszProcessor_Worker_task_started");

            //call your bootstrapper to initialize your application
            Bootstrapper.Init();

            Boolean initRun = true;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));

                using (_container = Bootstrapper.Container.GetNestedContainer())
                {
                    IDatasetContext _datasetContext = _container.GetInstance<IDatasetContext>();
                    //_hszFileWatchService = _container.GetInstance<IHszFileWatchService>();

                    //If it's completed dispose of it.
                    var tasks = currentTasks.Where(x => x.Name.StartsWith("Watch") && x.Task.IsCompleted).ToList();
                    tasks.ForEach(x => x.Task.Dispose());
                    tasks.ForEach(x => currentTasks.RemoveAll(ct => ct.Name == x.Name));

                    List<RetrieverJob> rtjob = null;


                    rtjob = _datasetContext.RetrieverJob.Where(w => (w.DataSource is DfsBasicHsz) && w.Schedule == "Instant" && w.IsEnabled).FetchParentMetadata(_datasetContext);

                    foreach (RetrieverJob job in rtjob)
                    {
                        Uri watchPath = new Uri(""); //job.GetUri(); //replaced with some junk since HSZ isn't used
                        int configID = job.DatasetConfig.ConfigId;

                        //On initial run start all watch tasks for all configs
                        if (initRun)
                        {
                            currentTasks.Add(new RunningTask(
                                Task.Factory.StartNew(() => new HszFileWatchService().OnStart(job, _token),
                                                                TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                TaskContinuationOptions.OnlyOnFaulted),
                                                                $"Watch_{job.Id}_{job.DatasetConfig.ConfigId}")
                                                        );
                            initRun = false;
                        }
                        //Restart any completed tasks
                        else if (tasks.Any(w => Int64.Parse(w.Name.Split('_')[2]) == configID))
                        {
                            currentTasks.Add(new RunningTask(
                                Task.Factory.StartNew(() => new HszFileWatchService().OnStart(job, _token),
                                                                TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                TaskContinuationOptions.OnlyOnFaulted),
                                                                $"Watch_{job.Id}_{job.DatasetConfig.ConfigId}")
                            );

                            Logger.Info($"Resstarting Watch_{job.Id}_{job.DatasetConfig.ConfigId}");
                        }
                        //Start any new directories added
                        else if (!currentTasks.Any(x => x.Name.StartsWith("Watch") && x.Name.Replace("Watch_", "") == $"{job.Id}_{job.DatasetConfig.ConfigId}"))
                        {
                            Logger.Info($"Detected new config ({configID}) to monitor ({watchPath})");

                            currentTasks.Add(new RunningTask(
                                Task.Factory.StartNew(() => new HszFileWatchService().OnStart(job, _token),
                                                                TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                TaskContinuationOptions.OnlyOnFaulted),
                                                                $"Watch_{job.Id}_{job.DatasetConfig.ConfigId}")
                            );
                        }
                    }
                }

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