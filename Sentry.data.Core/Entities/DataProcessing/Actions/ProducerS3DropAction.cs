using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ProducerS3DropAction : BaseAction
    {
        public ProducerS3DropAction() { }
        public ProducerS3DropAction(IS3DropProvider s3DropProvider) : base(s3DropProvider) { }
    }
}
