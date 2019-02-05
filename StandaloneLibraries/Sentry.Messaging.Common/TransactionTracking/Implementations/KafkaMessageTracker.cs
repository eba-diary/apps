namespace Sentry.Messaging.Common
{
    public class KafkaMessageTracker : IMessageTracker
    {
        #region Declarations
        private readonly IMessageHandler<IPublishable> _publisher;
        #endregion

        #region Constructors
        public KafkaMessageTracker(KafkaSettings settings)
        {
            _publisher = new KafkaMessagePublisher(settings);
            _publisher.Init();
        }
        #endregion

        #region IMessageTracker Implementation
        public void TrackMessageProcessingBegin(ITrackableMessage msg)
        {
            _publisher.Handle(new MessageTransaction(MessageActionCodes.MessageActionBegin, msg));
        }

        public void TrackMessageProcessingFailure(ITrackableMessage msg, string code, string detail)
        {
            _publisher.Handle(new MessageTransaction(MessageActionCodes.MessageActionFailure, msg, code, detail));
        }

        public void TrackMessageProcessingSkip(ITrackableMessage msg, string detail)
        {
            _publisher.Handle(new MessageTransaction(MessageActionCodes.MessageActionSkip, msg, detail));
        }

        public void TrackMessageProcessingSuccess(ITrackableMessage msg)
        {
            _publisher.Handle(new MessageTransaction(MessageActionCodes.MessageActionSuccess, msg));
        }
        public void RunOffPendingTransactions()
        {
            // There should be no pending transactions as messages are published instantly to Kafka
        }

        #endregion
    }
}
