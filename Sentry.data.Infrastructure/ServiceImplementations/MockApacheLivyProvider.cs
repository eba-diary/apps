using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly.Registry;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;

namespace Sentry.data.Infrastructure
{
    public class MockApacheLivyProvider : ApacheLivyProvider
    {
        public MockApacheLivyProvider(IHttpClientProvider httpClientProvider, IPolicyRegistry<string> policyRegistry) : base(httpClientProvider, policyRegistry)
        {
        }

        public override Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent)
        {
            Core.Entities.Livy.LivyBatch livyBatch = new Core.Entities.Livy.LivyBatch()
            {
                Id = 17,
                State = "starting",
                Appid = "App Id",
                AppInfo = new System.Collections.Generic.Dictionary<string, string>() { { "driverLogUrl", "driver value" }, { "sparkUiUrl", "spark UI Url value" } }
            };

            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(livyBatch)),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            return Task.FromResult(response);
        }

        public override Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            Core.Entities.Livy.LivyBatch livyBatch = new Core.Entities.Livy.LivyBatch()
            {
                Id = 17,
                State = "dead",
                Appid = "App Id",
                AppInfo = new System.Collections.Generic.Dictionary<string, string>() { { "driverLogUrl", "driver value" }, { "sparkUiUrl", "spark UI Url value" } }
            };

            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(livyBatch)),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            return Task.FromResult(response);
        }

    }
}
