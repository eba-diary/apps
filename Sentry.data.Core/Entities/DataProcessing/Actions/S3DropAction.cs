using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class S3DropAction : BaseAction
    {
        public S3DropAction()
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX;
        }
    }
}
