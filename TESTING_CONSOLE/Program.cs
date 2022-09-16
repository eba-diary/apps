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

            ExampleofThread obj = new ExampleofThread();

            // Creating and initializing threads
            Thread thr1 = new Thread(new ThreadStart(obj.Thread1));
            Thread thr2 = new Thread(new ThreadStart(obj.Thread2));

            thr1.Start();
            thr2.Start();

            Console.WriteLine("Press any key to stop");
            while (!Console.KeyAvailable)
            {
                System.Threading.Thread.Sleep(10);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        public class ExampleofThread
        {
            // Non-Static method
            public void Thread1()
            {
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    IApacheLivyProvider apacheProvider = container.GetInstance<IApacheLivyProvider>();

                    for (int x = 0; x < 2; x++)
                    {

                        Console.WriteLine("Thread1 is working");
                        string before = apacheProvider.GetBaseUrl();
                        apacheProvider.SetBaseUrl("abc.com");
                        Console.WriteLine($"Thread1 - BaseUrl:before-{before}:::after-{apacheProvider.GetBaseUrl()}");

                        // Sleep for 4 seconds
                        // Using Sleep() method
                        Thread.Sleep(4000);
                    }
                }
            }



            /// <summary>
            /// 
            /// </summary>
            public void Thread2()
            {
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    IApacheLivyProvider apacheProvider = container.GetInstance<IApacheLivyProvider>();

                    for (int x = 0; x < 6; x++)
                    {

                        Console.WriteLine("Thread2 is working");
                        string before = apacheProvider.GetBaseUrl();
                        apacheProvider.SetBaseUrl("sentry.com");
                        Console.WriteLine($"Thread2 - BaseUrl:before-{before}:::after-{apacheProvider.GetBaseUrl()}");

                        // Sleep for 4 seconds
                        // Using Sleep() method
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        
    }
}
