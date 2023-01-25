using Hangfire;
using Hangfire.SqlServer;
using Owin;
using Sentry.Configuration;

namespace Sentry.data.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
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

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(Config.GetHostSetting("DatabaseConnectionString"), options)
                // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888                        
                .UseLog4NetLogProvider()

                //https://docs.hangfire.io/en/latest/upgrade-guides/upgrading-to-hangfire-1.7.html#updating-configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();


            //Default URL is /Hangfire
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AuthorizeHangfireDashboard() }
            });
        }
    }
}