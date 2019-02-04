using Sentry.AsyncCommandProcessor;
using Sentry.Common.Logging;

namespace Sentry.Messaging.Common
{
    public class EmptyQueueStatCommandProcessor : AsyncCommandProcessor.AsyncCommandProcessor
    {
        protected override void ProcessCommand(ICommand cmd)
        {
            Logger.Info("Processed Command: " + cmd.ToString());
        }
    }
}
