using System;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StructureMap;
using Newtonsoft.Json;

namespace ConsoleAppProducer
{
    class Program
    {
        static void Main(string[] args)
        {

            S3Event s3e = new S3Event();

            s3e.EventType = "S3EVENT";
            s3e.PayLoad.eventName = "ObjectCreated:Put";
            s3e.PayLoad.s3.bucket.name = "sentry-dataset-management-np-nr";
            s3e.PayLoad.s3.Object.key = "droplocation/77/";

            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessagePublisher _messagePublisher = container.GetInstance<IMessagePublisher>();

                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
            }            
        }
    }
}
