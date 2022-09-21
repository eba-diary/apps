using Sentry.Common.Logging;
using Sentry.data.Infrastructure;
using System;

namespace TESTING_CONSOLE
{
    public class Class1
    {

        static void Main(string[] args)
        {
            //Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            Logger.Info("Starting TESTING_CONSOLE");
            //Call your bootstrapper to initialize your application
            Bootstrapper.Init();

            
            Console.WriteLine("Press any key to stop");
            while (!Console.KeyAvailable)
            {
                System.Threading.Thread.Sleep(10);
            }

        }
    }
}
