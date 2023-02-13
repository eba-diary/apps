using Sentry.data.Core;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Sentry.data.Infrastructure
{
    public class HttpClientGenerator : IHttpClientGenerator
    {
        private readonly IDataFeatures _dataFeatures;

        public HttpClientGenerator(IDataFeatures dataFeatures)
        {
            _dataFeatures = dataFeatures;
        }

        public HttpClient GenerateHttpClient(string url)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };
            
            if (WebHelper.TryGetWebProxy(_dataFeatures.CLA3819_EgressEdgeMigration.GetValue(), out WebProxy webProxy))
            {
                httpClientHandler.Proxy = webProxy;
            };

            HttpClient client = new HttpClient(httpClientHandler, true);

            if (url.ToLower().Contains(".sentry.com"))
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Configuration.Config.GetHostSetting("ServiceAccountID")}:{Configuration.Config.GetHostSetting("ServiceAccountPassword")}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
            }

            return client;
        }
    }
}
