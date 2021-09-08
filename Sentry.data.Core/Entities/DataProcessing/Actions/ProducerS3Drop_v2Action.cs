using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ProducerS3Drop_v2Action : BaseAction
    {
        public ProducerS3Drop_v2Action() { }
        public ProducerS3Drop_v2Action(IS3DropProvider s3DropProvider) : base(s3DropProvider) { }
    }
}
