using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;
using Sentry.data.Infrastructure;
using StructureMap;

namespace TESTING_CONSOLE
{
    public class Class1
    {
        static void Main(string[] args)
        {
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            Logger.Info("Starting TESTING_CONSOLE");
            //Call your bootstrapper to initialize your application
            Bootstrapper.Init();

            //create an IOC (structuremap) container to wrap this transaction
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                //    var service = container.GetInstance<MyService>();
                //    var result = service.DoWork();
                //    container.GetInstance<ITESTING_CONSOLEContext>.SaveChanges();

                DataFlowProvider provider = container.GetInstance<DataFlowProvider>();

                provider.ExecuteDependencies("sentry-dataset-management-np-nr", "data/17/TestFile.csv");
            }

            Logger.Info("Console App completed successfully.");
        }
    }
}
