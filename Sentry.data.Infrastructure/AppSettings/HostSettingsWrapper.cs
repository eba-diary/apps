using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class HostSettingsWrapper : IHostSettings
    {
        public string this[string key]
        {
            get
            {
                return Configuration.Config.GetHostSetting(key);
            }
        }
    }
}
