using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class S3DropAction : BaseAction
    {
       private IS3DropProvider _s3DropProvider;
       public S3DropAction() { }
       public S3DropAction(IS3DropProvider s3DropProvider)
        {
            _s3DropProvider = s3DropProvider;
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX + Configuration.Config.GetHostSetting("S3DataPrefix");
        }
    }
}
