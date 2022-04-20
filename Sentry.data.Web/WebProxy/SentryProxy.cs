﻿using System;
using System.Linq;
using System.Net;
using Sentry.Common.Logging;

namespace Sentry.data.Web
{
    public class SentryProxy : IWebProxy
    {
        private static string proxyConfigScript = Sentry.Configuration.Config.GetHostSetting("WebProxyConfigFile");
        private static string[] useWebProxyList = Sentry.Configuration.Config.GetHostSetting("WebProxyUseList").Split(',');
        //private static string webProxyUrl = UseEdgeProxy ? Sentry.Configuration.Config.GetHostSetting("EdgeWebProxyUrl") : Sentry.Configuration.Config.GetHostSetting("WebProxyUrl");
        //private static ICredentials proxyCredentials = UseEdgeProxy
        //    ? new NetworkCredential(Sentry.Configuration.Config.GetHostSetting("ServiceAccountID"), Sentry.Configuration.Config.GetHostSetting("ServiceAccountPassword")) 
        //    : CredentialCache.DefaultNetworkCredentials;

        public static bool UseEdgeProxy { get; set; }

        private string WebProxyUrl
        {
            get
            {
                return UseEdgeProxy ? Sentry.Configuration.Config.GetHostSetting("EdgeWebProxyUrl") : Sentry.Configuration.Config.GetHostSetting("WebProxyUrl");
            }
        }

        public ICredentials Credentials
        {
            get
            {
                Logger.Debug($"{nameof(SentryProxy)}_{nameof(GetProxy)} Pulling Proxy Creds");
                return UseEdgeProxy
                        ? new NetworkCredential(Sentry.Configuration.Config.GetHostSetting("ServiceAccountID"), Sentry.Configuration.Config.GetHostSetting("ServiceAccountPassword"))
                        : CredentialCache.DefaultNetworkCredentials;
            }

            set
            {
                // do nothing
            }
        }

        public Uri GetProxy(Uri destination)
        {
            Logger.Debug($"{nameof(SentryProxy)}_{nameof(GetProxy)} UseEdgeProxy:::{UseEdgeProxy}");
            if (!String.IsNullOrEmpty(proxyConfigScript))
            {
                var proxyToUse = GetProxyForUrlUsingPac(destination.ToString(), proxyConfigScript);
                if (!proxyToUse.StartsWith("http"))
                    proxyToUse = "http://" + proxyToUse;

                Logger.Debug($"{nameof(SentryProxy)}_{nameof(GetProxy)} proxyscript:::{proxyToUse}");

                return new Uri(proxyToUse);
            }

            Logger.Debug($"{nameof(SentryProxy)}_{nameof(GetProxy)} proxy:::{WebProxyUrl}");

            return new Uri(WebProxyUrl);
        }

        /*this method is called everytime a URL is hit in the WEB app to determine if the SentryProxy should be bypassed for a given URL
         * todate the only thing to use the proxy is the snowflake integration
         * everything else should NOT use proxy
         */
        public bool IsBypassed(Uri host)
        {
            //check passed in host against list, if the host is in the list, then they DO NOT get to bypass and therefore return false meaning:  "NO, you can't bypass the proxy"
            if (useWebProxyList.Any(x => !String.IsNullOrEmpty(x) && host.AbsoluteUri.ToLower().Contains(x.ToLower())))
                return false;

            if (!String.IsNullOrEmpty(proxyConfigScript))
            {
                var proxyToUse = GetProxyForUrlUsingPac(host.ToString(), proxyConfigScript);
                return (proxyToUse == null);
            }

            return true;
        }

        public static string GetProxyForUrlUsingPac(string DestinationUrl, string PacUri)
        {

            IntPtr WinHttpSession = Win32Api.WinHttpOpen("User",
                                           Win32Api.WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
                                           IntPtr.Zero,
                                           IntPtr.Zero,
                                           0);

            Win32Api.WINHTTP_AUTOPROXY_OPTIONS ProxyOptions =
                     new Win32Api.WINHTTP_AUTOPROXY_OPTIONS();
            Win32Api.WINHTTP_PROXY_INFO ProxyInfo =
                            new Win32Api.WINHTTP_PROXY_INFO();

            ProxyOptions.dwFlags = Win32Api.WINHTTP_AUTOPROXY_CONFIG_URL;
            ProxyOptions.dwAutoDetectFlags = (Win32Api.WINHTTP_AUTO_DETECT_TYPE_DHCP |
                                              Win32Api.WINHTTP_AUTO_DETECT_TYPE_DNS_A);
            ProxyOptions.lpszAutoConfigUrl = PacUri;

            // Get Proxy 
            bool IsSuccess = Win32Api.WinHttpGetProxyForUrl(WinHttpSession,
                                                             DestinationUrl,
                                                             ref ProxyOptions,
                                                             ref ProxyInfo);

            Win32Api.WinHttpCloseHandle(WinHttpSession);

            if (IsSuccess)
            {
                return ProxyInfo.lpszProxy;
            }
            else
            {
                return "";
            }
        }


    }
}