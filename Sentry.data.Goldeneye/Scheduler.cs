using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using System.Threading;

namespace Sentry.data.Goldeneye
{
    class Scheduler
    {
        private BackgroundJobServer _server;

        /// <summary>
        /// Initialize the HangFire Scheuler server
        /// </summary>
        public Scheduler()
        {            
            var options = new SqlServerStorageOptions
            {
                //Turn off automatic creation of HangFire database schema
                PrepareSchemaIfNecessary = false
            };

            GlobalConfiguration.Configuration.UseSqlServerStorage(Configuration.Config.GetHostSetting("DatabaseConnectionString"), options);

        }

        public void Run(CancellationToken token)
        {
            Sentry.Common.Logging.Logger.Info("Starting Hangfire Job Server");

            var options = new BackgroundJobServerOptions
            {
                Queues = new[]
                {
                    "spamfactory",
                    "default",
                    "jawsservice"
                },
                ServerName = $"{Configuration.Config.GetHostSetting("EnvironmentName")}-{Environment.MachineName}.{Guid.NewGuid().ToString().Substring(0,20)}"
            };

            _server = new BackgroundJobServer(options);

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Sentry.Common.Logging.Logger.Info("Cancellation Requested of HangFire Server");
                    _server.Dispose();
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
