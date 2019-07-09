using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum HttpDataFormat
    {
        none = 0,
        [Description("JSON")]
        json = 1
    }
}
