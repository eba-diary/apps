using Microsoft.Extensions.Logging;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.EnterpriseLogging;
using System;
using System.Linq;
using System.ServiceProcess;

namespace Sentry.data.Goldeneye
{

    public class Program
    {

        /// <summary>
        /// The entry point into the Windows Service.  You can also run this program
        /// as a console app if you pass "-console" as the first parameter.
        /// </summary>
        /// <remarks>
        /// Setup the Debug command line arguments in your Project properties
        /// to include "-console" so that you can debug your service.
        /// </remarks>
        static void Main(string[] args)
        {
            //initialize logging framework
            var loggerFactory = ConfigureLogger();

            Bootstrapper.Init();
            Bootstrapper.Container.Configure((x) =>
            {
                x.For<ICurrentUserIdProvider>().Use<ThreadCurrentUserIdProvider>();
                x.For<ILoggerFactory>().Singleton().Use(loggerFactory);
                x.For(typeof(ILogger<>)).Singleton().Use(typeof(Logger<>));
            });

            if (args.Contains("-console"))
            {
                //start as a console app
                Core myCore = new Core(loggerFactory.CreateLogger<Core>());
                myCore.OnStart();

                Console.WriteLine("Press any key to stop");
                while (!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(10);
                }

                myCore.OnStop();

            }
            else
            {
                //start as a windows service
                Service winService = new Service(loggerFactory);
                ServiceBase.Run(new ServiceBase[] { winService });
            }            
        }


        /// <summary>
        /// Wire up logging to use Sentry.Common.Logging
        /// </summary>
        public static ILoggerFactory ConfigureLogger()
        {
            // The following configures the "default" logging behavior for anything that continues to 
            // use Sentry.Common for logging, including legacy Sentry.* libraries
            Sentry.Common.Logging.Logger.LoggingFrameworkAdapter =
                new Sentry.Common.Logging.Adapters.Log4netAdapter("Sentry.Common.Logging");

            // The following is the new ILoggerFactory/ILogger setup for modern logging that is 
            // ready for .NET 5.
            var loggerFactory = new SentryLoggerFactory(new Microsoft.Extensions.Logging.LoggerFactory());
            var log4netOptions = new Log4NetProviderOptions();
            if (Sentry.Configuration.Config.GetDefaultEnvironmentName().ToUpper() == "DEV")
            {
                log4netOptions.Log4NetConfigFileName = "log4net.local.config";
            }
            else
            {
                log4netOptions.Log4NetConfigFileName = "log4net.server.config";
            }

            loggerFactory.AddLog4Net(log4netOptions);

            return loggerFactory;
        }
    }
}