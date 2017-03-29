using Owin;
using Sentry.data;

namespace Sentry.data.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}