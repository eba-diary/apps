using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;

namespace DatasetLoader
{
    public class Class1
    {

        static void Main(string[] args)
        {
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            //Call your bootstrapper to initialize your application
            //Bootstrapper.Init();

            //create an IOC (structuremap) container to wrap this transaction
            //using (container = Bootstrapper.Container.GetNestedContainer)
            //{
            //    var service = container.GetInstance<MyService>();
            //    var result = service.DoWork();
            //    container.GetInstance<IDatasetLoaderContext>.SaveChanges();
            //}

            Logger.Info("Console App completed successfully.");
        }
    }
}
