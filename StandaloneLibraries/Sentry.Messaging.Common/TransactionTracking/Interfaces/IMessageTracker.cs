namespace Sentry.Messaging.Common
{
    public interface IMessageTracker
    {
        void TrackMessageProcessingBegin(ITrackableMessage msg);
        void TrackMessageProcessingSuccess(ITrackableMessage msg);
        void TrackMessageProcessingFailure(ITrackableMessage msg, string code, string detail);
        void TrackMessageProcessingSkip(ITrackableMessage msg, string detail);
        void RunOffPendingTransactions();
    }
}
