using Sentry.Common.Logging;
using System;

namespace Sentry.Messaging.Common
{
    public class ComboMessageTracker : IMessageTracker
    {
        #region Declarations
        private readonly IMessageTracker _tracker1;
        private readonly IMessageTracker _tracker2;
        #endregion

        #region Constructors
        public ComboMessageTracker(IMessageTracker tracker1, IMessageTracker tracker2)
        {
            _tracker1 = tracker1;
            _tracker2 = tracker2;
        }
        #endregion

        #region IMessageTracker Implementation
        void IMessageTracker.RunOffPendingTransactions()
        {
            _tracker1.RunOffPendingTransactions();
            _tracker2.RunOffPendingTransactions();
        }

        void IMessageTracker.TrackMessageProcessingBegin(ITrackableMessage msg)
        {
            int successes = 0;
            try
            {
                _tracker1.TrackMessageProcessingBegin(msg);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Begin message to Tracker 1 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }

            try
            {
                _tracker2.TrackMessageProcessingBegin(msg);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Begin message to Tracker 2 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }

            if (successes == 0)
            {
                throw new Exception("Failed to publish Begin message transaction to both trackers in ComboMessageTracker. Please examine.");
            }
        }

        void IMessageTracker.TrackMessageProcessingFailure(ITrackableMessage msg, string code, string detail)
        {
            int successes = 0;
            try
            {
                _tracker1.TrackMessageProcessingFailure(msg, code, detail);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Failure message to Tracker 1 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }
            try
            {
                _tracker2.TrackMessageProcessingFailure(msg, code, detail);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Failure message to Tracker 2 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }

            if (successes == 0)
            {
                throw new Exception("Failed to publish Failure message transaction to both trackers in ComboMessageTracker. Please examine.");
            }
        }

        void IMessageTracker.TrackMessageProcessingSkip(ITrackableMessage msg, string detail)
        {
            int successes = 0;
            try
            {
                _tracker1.TrackMessageProcessingSkip(msg, detail);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Skip message to Tracker 1 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }
            try
            {
                _tracker2.TrackMessageProcessingSkip(msg, detail);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Skip message to Tracker 2 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }

            if (successes == 0)
            {
                throw new Exception("Failed to publish Skip message transaction to both trackers in ComboMessageTracker. Please examine.");
            }
        }

        void IMessageTracker.TrackMessageProcessingSuccess(ITrackableMessage msg)
        {
            int successes = 0;
            try
            {
                _tracker1.TrackMessageProcessingSuccess(msg);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Success message to Tracker 1 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }
            try
            {
                _tracker2.TrackMessageProcessingSuccess(msg);
                successes++;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to publish Success message to Tracker 2 of ComboMessageTracker. Message Id: " + msg.GetMessageId(), e);
            }

            if (successes == 0)
            {
                throw new Exception("Failed to publish Success message transaction to both trackers in ComboMessageTracker. Please examine.");
            }
        }
        #endregion
    }
}
