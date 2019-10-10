using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class S3DropAction : BaseAction, IDataStep
    {
        public S3DropAction()
        {
            TargetStoragePrefix = "temp/s3drop/";
        }

    }
}
