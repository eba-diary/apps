﻿using Sentry.data.Core;
using System.Net;
using System.Net.Http;

namespace Sentry.data.Infrastructure
{
    public class HttpClientGenerator : IHttpClientGenerator
    {
        private readonly IDataFeatures _dataFeatures;

        public HttpClientGenerator(IDataFeatures dataFeatures)
        {
            _dataFeatures = dataFeatures;
        }

        public HttpClient GenerateHttpClient()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            if (WebHelper.TryGetWebProxy(_dataFeatures.CLA3819_EgressEdgeMigration.GetValue(), out WebProxy webProxy))
            {
                httpClientHandler.Proxy = webProxy;
            };

            return new HttpClient(httpClientHandler, true);
        }
    }
}