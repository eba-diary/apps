using Sentry.Common.Logging;
using System.Net;

namespace Sentry.data.Core
{
    public static class WebHelper
    {
        public static bool TryGetWebProxy(bool useEgress, out WebProxy webProxy)
        {
            if (bool.Parse(Configuration.Config.GetHostSetting("UseProxy")))
            {
                NetworkCredential proxyCredentials;
                string proxyUrl;

                if (useEgress)
                {
                    Logger.Debug($"Using edge proxy: true");
                    string userName = Configuration.Config.GetHostSetting("ServiceAccountID");
                    string password = Configuration.Config.GetHostSetting("ServiceAccountPassword");
                    proxyUrl = Configuration.Config.GetHostSetting("EdgeWebProxyUrl");
                    proxyCredentials = new NetworkCredential(userName, password);
                }
                else
                {
                    Logger.Debug($"Using edge proxy: false");
                    proxyUrl = Configuration.Config.GetHostSetting("WebProxyUrl");
                    proxyCredentials = CredentialCache.DefaultNetworkCredentials;
                }

                Logger.Debug($"ProxyUser: {proxyCredentials.UserName}");

                webProxy = new WebProxy(proxyUrl)
                {
                    Credentials = proxyCredentials
                };

                return true;
            }

            webProxy = null;
            return false;
        }
    }
}
