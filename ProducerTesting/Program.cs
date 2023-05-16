using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Infrastructure;
using StructureMap;
using System.Threading;

namespace ProducerTesting
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Wire up logging here if needed

            //Call your bootstrapper to initialize your application
            Bootstrapper.Init();

            int JobId = 4848;

            for (int i = 1; i < 5; i++)
            {
                //DfsEvent DfsE = new DfsEvent();
                //DfsE.DatasetID = 0;
                //DfsE.PayLoad = new DfsEventPayload()
                //{
                //    JobId = JobId,
                //    FullPath = $"c:\\tmp\\DatasetLoader\\0000315\\JCGDTest{i.ToString()}.csv"
                //};

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
                                name = "sentry-data-nrdev-droplocation-ae2"
                            },
                            Object = new Sentry.data.Core.Entities.S3.Object()
                            {
                                key = $"droplocation/data/DATA/0000315/JCGDTest{i.ToString()}.csv"
                            }
                        }

                    }
                };

                SendMessage(s3e);
            }

            //S3Event s3e = null;
            ////Droplocation event
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
            //            Object = new Sentry.data.Core.Entities.S3.Object()
            //            {
            //                key = "64/Testfile.csv"
            //            }
            //        }

            //    }
            //};


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

            //SendMessage(s3e);

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

        private static void SendMessage(DfsEvent msg)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessagePublisher _messagePublisher = container.GetInstance<IMessagePublisher>();

                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(msg));

                //sleep 10 seconds (10000)
                //Thread.Sleep(10000);
            }
        }

    }
    
}
