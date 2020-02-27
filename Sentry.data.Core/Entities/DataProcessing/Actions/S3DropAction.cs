using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class S3DropAction : BaseAction
    {
       public S3DropAction() { }
       public S3DropAction(IS3DropProvider s3DropProvider) : base(s3DropProvider) { }
    }
}
