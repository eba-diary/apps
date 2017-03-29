using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class SharedContextModel
    {
        public IApplicationUser CurrentUser { get; set; }
        public IApplicationUser CurrentRealUser { get; set; }
    }
}
