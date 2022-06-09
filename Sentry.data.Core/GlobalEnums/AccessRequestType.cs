using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AccessRequestType
    {
        Default = 0,
        [Description("AWS Consume")]
        AwsArn = 1
    }
}
