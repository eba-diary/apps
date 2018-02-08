using System.ServiceProcess;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System;

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
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            if (args.Contains("-console"))
            {
                //start as a console app
                Core myCore = new Core();
                myCore.OnStart();

                Console.WriteLine("Press any key to stop");
                while (!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(10);
                }

                myCore.OnStop();

                while(true)
                {
                    int i = 0;
                }
            }
            else
            {
                //start as a windows service
                Service winService = new Service();
                ServiceBase.Run(new ServiceBase[] { winService });
            }
        }
    }
}