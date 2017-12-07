using System.Threading;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Threading.Tasks;
using System;
using Sentry.data.Core;
using Sentry.data.Infrastructure;

namespace Sentry.data.SpamFactory
{
    /// <summary>
    /// This class represents where you should run your application logic
    /// </summary>
    public class Core
    {

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

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
        private async Task DoWork()
        {
            Logger.Info("Worker task started.");

            Bootstrapper.Init();

            DateTime lastRunMinute = DateTime.Now;
            DateTime lastRunHour = DateTime.Now;
            DateTime lastRunDay = DateTime.Now;
            DateTime lastRunWeek = DateTime.Now;

            do
            {
                if ((DateTime.Now - lastRunMinute).TotalMinutes >= 1)
                {
                    lastRunMinute = DateTime.Now;
                    await Watch.Run("Instant");
                }

                if ((DateTime.Now - lastRunHour).TotalHours >= 1)
                {
                    lastRunHour = DateTime.Now;
                    await Watch.Run("Hourly");
                }

                if ((DateTime.Now - lastRunDay).TotalHours >= 24)
                {
                    lastRunDay = DateTime.Now;
                    await Watch.Run( "Daily");
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && (DateTime.Now - lastRunWeek).TotalDays >= 7)
                {
                    lastRunWeek = DateTime.Now;
                    await Watch.Run("Weekly");
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