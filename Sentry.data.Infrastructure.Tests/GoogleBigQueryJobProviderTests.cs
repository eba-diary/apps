using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Net;
using Moq.Protected;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GoogleBigQueryJobProviderTests
    {
        [TestMethod]
        public void Execute_RetrieverJob_FieldsNotFound()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            HTTPSSource httpsSource = new HTTPSSource() { BaseUri = new Uri("https://bigquery.googleapis.com/bigquery/v2/") };

            Mock<IAuthorizationProvider> authorizationProvider = repository.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(httpsSource)).Returns("token");

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                Content = new StringContent(GetNotFoundResponse()),
                StatusCode = HttpStatusCode.NotFound
            };
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            Mock<IHttpClientGenerator> httpClientGenerator = repository.Create<IHttpClientGenerator>();
            httpClientGenerator.Setup(x => x.GenerateHttpClient()).Returns(httpClient);

            GoogleBigQueryJobProvider provider = new GoogleBigQueryJobProvider(null, null, authorizationProvider.Object, httpClientGenerator.Object, null);

            string datePartition = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

            RetrieverJob job = new RetrieverJob()
            {
                DataSource = httpsSource,
                RelativeUri = $"projects/project1/datasets/dataset1/tables/events_{datePartition}/data",
            };

            provider.Execute(job);

            Assert.IsTrue(httpClient.DefaultRequestHeaders.Contains("Authorization"));
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.GetValues("Authorization").First());

            repository.VerifyAll();
        }

        [TestMethod]
        public void Execute_RetrieverJob_SingleLoop()
        {

        }

        [TestMethod]
        public void Execute_RetrieverJob_FailInMiddle()
        {

        }

        [TestMethod]
        public void Execute_RetrieverJob_StartFromFailure()
        {

        }

        #region Helpers
        private string GetNotFoundResponse()
        {
            return $"{{\r\n  \"error\": {{\r\n    \"code\": 404,\r\n    \"message\": \"Not found: Table ga4-data-lake:analytics_255461376.events_2022091\",\r\n    \"errors\": [\r\n      {{\r\n        \"message\": \"Not found: Table ga4-data-lake:analytics_255461376.events_2022091\",\r\n        \"domain\": \"global\",\r\n        \"reason\": \"notFound\"\r\n      }}\r\n    ],\r\n    \"status\": \"NOT_FOUND\"\r\n  }}\r\n}}";
        }
        #endregion
    }
}
