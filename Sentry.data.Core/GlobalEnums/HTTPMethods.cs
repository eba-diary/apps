using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum HttpMethods
    {
        none = 0,
        [Description("Get")]
        get = 1,
        [Description("Post")]
        post = 2
    }
}
