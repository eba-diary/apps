using Sentry.Common.Logging;

namespace Sentry.Messaging.Common
{
    //this class is intentionally "light" do not expect it to be used outside of a unit test scenario
    //where you are not really concerned about message tracking
    public class EmptyMessageTracker : IMessageTracker
    {
        #region IMessageTracker Implementation
        void IMessageTracker.TrackMessageProcessingBegin(ITrackableMessage msg)
        {
            Logger.Info("Message Begin: " + msg.GetMessageId().ToString());
        }

        void IMessageTracker.TrackMessageProcessingFailure(ITrackableMessage msg, string code, string detail)
        {
            Logger.Info("Message Failure: " + msg.GetMessageId().ToString());
        }

        void IMessageTracker.TrackMessageProcessingSkip(ITrackableMessage msg, string detail)
        {
            Logger.Info("Message Skip: " + msg.GetMessageId().ToString());
        }

        void IMessageTracker.TrackMessageProcessingSuccess(ITrackableMessage msg)
        {
            Logger.Info("Message Success: " + msg.GetMessageId().ToString());
        }
        void IMessageTracker.RunOffPendingTransactions()
        {
            //Do nothing
        }
        #endregion
    }
}
