using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;
using StructureMap;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using Newtonsoft.Json;
using Sentry.data.Core.Entities.S3;
using System.Threading;

namespace ProducerTesting
{
    public class Class1
    {

        static void Main(string[] args)
        {
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            //Call your bootstrapper to initialize your application
            Bootstrapper.Init();
            S3Event s3e = null;
            //Droplocation event
            s3e = new S3Event
            {
                EventType = "S3EVENT",
                PayLoad = new S3ObjectEvent()
                {
                    eventName = "ObjectCreated:Put",
                    s3 = new S3()
                    {
                        bucket = new Bucket()
                        {
                            name = "sentry-dataset-management-np-nr"
                        },
                        Object = new Sentry.data.Core.Entities.S3.Object()
                        {
                            key = "25/Testfile.csv"
                        }
                    }

                }
            };

            //SendMessage(s3e);

            ////S3drop event
            //s3e = new S3Event
            //{
            //    EventType = "S3EVENT",
            //    PayLoad = new S3ObjectEvent()
            //    {
            //        eventName = "ObjectCreated:Put",
            //        s3 = new S3()
            //        {
            //            bucket = new Bucket()
            //            {
            //                name = "sentry-dataset-management-np-nr"
            //            },
            //            _object = new Sentry.data.Core.Entities.S3.Object()
            //            {
            //                key = "data/25/1571855649-1571855999/Testfile.csv"
            //            }
            //        }
            //    }
            //};

            SendMessage(s3e);

            //S3drop event
            //s3e = new S3Event
            //{
            //    EventType = "S3EVENT",
            //    PayLoad = new S3ObjectEvent()
            //    {
            //        eventName = "ObjectCreated:Put",
            //        s3 = new S3()
            //        {
            //            bucket = new Bucket()
            //            {
            //                name = "sentry-dataset-management-np-nr"
            //            },
            //            _object = new Sentry.data.Core.Entities.S3.Object()
            //            {
            //                key = "data/17/1571763616/Testfile.csv"
            //            }
            //        }
            //    }
            //};

            //SendMessage(s3e);

            Logger.Info("Console App completed successfully.");
        }




        private static void SendMessage(S3Event msg)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessagePublisher _messagePublisher = container.GetInstance<IMessagePublisher>();

                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(msg));

                //sleep 10 seconds (10000)
                Thread.Sleep(10000);
            }
        }

    }
    
}
