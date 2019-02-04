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
    public class MessageTransactionKafkaConsumer : BaseKafkaConsumer<Null, MessageTransaction>
    {
        #region BaseKafkaConsumer Overrides
        protected override void _consumer_OnMessage(object sender, Message<Null, string> e)
        {
            MessageTransaction msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<MessageTransaction>(e.Value);
                Logger.Debug(msg.MessageId + " consumed from topic.");
                ProcessMessage(msg);
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

        protected override IDeserializer<Null> GetKeyDeserializer()
        {
            return null;
        }
        #endregion

        #region Constructors
        public MessageTransactionKafkaConsumer(KafkaSettings settings) : base(settings) { }
        #endregion
    }
}
