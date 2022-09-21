using Microsoft.Win32;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading;

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
