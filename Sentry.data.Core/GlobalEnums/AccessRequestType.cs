using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AccessRequestType
    {
        Default = 0,
        [Description("AWS Consume")]
        AwsArn = 1,
        [Description("Remove Permission")]
        RemovePermission,
        [Description("Inheritance")]
        Inheritance,
        [Description("Snowflake Account")]
        SnowflakeAcount = 4
    }
}
