using Confluent.Kafka;
using Sentry.Common.Logging;
using System;

namespace Sentry.data.Infrastructure
{
    public class ProducerDeliveryHandler : IDeliveryHandler<string, string>
    {
        public bool MarshalData => true;

        public void HandleDeliveryReport(Message<string, string> mge)
        {
            try
            {
                if (mge.Error.ToString() == "Success")
                {
                    Logger.Info($"Producer Message Delivery - Key:{mge.Key} Offset:{mge.Offset} Partition:{mge.Partition} Topic:{mge.Topic} Message:{mge.Value}");
                }
                else
                {
                    Logger.Error($"Failed Producer Message Delivery - Error:{mge.Error} Key:{mge.Key} Offset:{mge.Offset} Partition:{mge.Partition} Topic:{mge.Topic} Message:{mge.Value}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Producer Delivery Report Capture Failed", ex);
            }
        }
    }
}