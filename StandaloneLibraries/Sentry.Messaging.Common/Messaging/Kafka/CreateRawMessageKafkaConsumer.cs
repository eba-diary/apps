using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Text;

namespace Sentry.Messaging.Common
{
    public class CreateRawMessageKafkaConsumer : BaseKafkaConsumer<string, RawMessage>
    {
        #region Declarations
        private readonly string _deliverySource;
        private readonly string _messageArea;
        private readonly string _messageSystemSource;
        private readonly string _detailMessageType;
        #endregion

        #region BaseKafkaConsumer Overrides
        protected override void _consumer_OnMessage(object sender, Message<string, string> e)
        {
            if (!string.IsNullOrEmpty(e.Value))
            {
                RawMessage msg = new RawMessage(e.Value, _deliverySource, _messageArea, _messageSystemSource, _detailMessageType);
                MessageTransactionTrackingAccess.MessageTransactionTracker.TrackMessageProcessingBegin(msg);
                try
                {
                    ProcessMessage(msg);
                }
                catch (Exception ex)
                {
                    MessageTransactionTrackingAccess.MessageTransactionTracker.TrackMessageProcessingFailure(msg, MessageTrackingFailureCodes.FailureCodeUnknown, ex.ToString());
                }
            }
        }

        protected override IDeserializer<string> GetKeyDeserializer()
        {
            return new Confluent.Kafka.Serialization.StringDeserializer(Encoding.UTF8);
        }
        #endregion

        #region Constructors
        public CreateRawMessageKafkaConsumer(KafkaSettings settings,
                                    string deliverySource,
                                    string msgArea,
                                    string detailMsgType) : base(settings)
        {
            _deliverySource = deliverySource;
            _messageArea = msgArea;
            _messageSystemSource = settings.TopicName;
            _detailMessageType = detailMsgType;
        }

        #endregion
    }
}

