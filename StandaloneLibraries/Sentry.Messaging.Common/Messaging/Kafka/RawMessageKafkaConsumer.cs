using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using System;

namespace Sentry.Messaging.Common
{
    public class RawMessageKafkaConsumer : BaseKafkaConsumer<Null, RawMessage>
    {
        #region BaseKafkaConsumer Overrides
        protected override void _consumer_OnMessage(object sender, Message<Null, string> e)
        {
            RawMessage msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<RawMessage>(e.Value);
                MessageTransactionTrackingAccess.MessageTransactionTracker.TrackMessageProcessingBegin(msg);
                ProcessMessage(msg);
            }
            catch (Exception ex)
            {
                if (msg == null) Logger.Error("Skipping message - bad payload: " + ex.ToString());
                else
                {
                    MessageTransactionTrackingAccess.MessageTransactionTracker.TrackMessageProcessingFailure(msg, MessageTrackingFailureCodes.FailureCodeUnknown, ex.ToString());
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
        public RawMessageKafkaConsumer(KafkaSettings settings) : base(settings) { }
        #endregion
    }
}

