using Hangfire;
using Hangfire.SqlServer;
using Hangfire.StructureMap;
using Sentry.data.Infrastructure;
using System;
using System.Threading;

namespace Sentry.data.Goldeneye
{
    internal class Scheduler : IDisposable
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
                PrepareSchemaIfNecessary = false,
                SchemaName = "HangFire7",
                UsePageLocksOnDequeue = true, // Migration to Schema 7 is required
                DisableGlobalLocks = true,    // Migration to Schema 7 is required
                EnableHeavyMigrations = false // Default value: false
            };

            GlobalConfiguration.Configuration.UseStructureMapActivator(Bootstrapper.Container.GetNestedContainer());
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(Configuration.Config.GetHostSetting("DatabaseConnectionString"), options)
                .UseLog4NetLogProvider()

                //https://docs.hangfire.io/en/latest/upgrade-guides/upgrading-to-hangfire-1.7.html#updating-configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

        }

        public void Dispose()
        {
            _server.Dispose();
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

            token.Register(() =>
            {
                Sentry.Common.Logging.Logger.Info("Cancellation Requested of HangFire Server");
                _server.Dispose();
            });
        }
    }
}
