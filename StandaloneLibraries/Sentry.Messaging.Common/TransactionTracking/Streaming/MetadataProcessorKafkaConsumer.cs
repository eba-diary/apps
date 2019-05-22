using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public class MetadataProcessorKafkaConsumer : BaseKafkaConsumer<string, string>
    {
        #region BaseKafkaConsumer Overrides
        protected override void _consumer_OnMessage(object sender, Message<string, string> e)
        {
            Logger.Info($"Key:{e.Value}");
            BaseEventMessage msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<BaseEventMessage>(e.Value);
                Logger.Info(msg.EventType + " consumed from topic.");
                ProcessMessage(e.Value);
            }
            catch (Exception ex)
            {
                if (msg == null) Logger.Error("Skipping message - bad payload: " + ex.ToString());
                else
                {
                    Logger.Info("Message Skipped due to unknown reason: " + ex.ToString());
                }
            }
        }

        protected override IDeserializer<string> GetKeyDeserializer()
        {
            return new Confluent.Kafka.Serialization.StringDeserializer(Encoding.UTF8);
        }
        #endregion

        #region Constructors
        public MetadataProcessorKafkaConsumer(KafkaSettings settings) : base(settings) { }
        #endregion
    }
}
