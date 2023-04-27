using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly.Registry;
using Sentry.data.Core.Interfaces;
using Sentry.data.Infrastructure.PollyPolicies;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ApacheLivyProviderTests
    {
        [TestMethod]
        public async Task ApacheLivyProvider_GetRequestAsync_BadRequest()
        {
            var policyRegistry = new PolicyRegistry();
            Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Mock<IHttpClientProvider> httpClientProvider = new Mock<IHttpClientProvider>();
            httpClientProvider.Setup(h => h.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            Mock<ApacheLivyProvider> providerB = new Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };
            providerB.Object.SetBaseUrl("www.abc.com");
            var result = await providerB.Object.GetRequestAsync("/batches");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ApacheLivyProvider_GetRequestAsync_HttpRequestException()
        {
            var policyRegistry = new PolicyRegistry();
            Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Mock<IHttpClientProvider> httpClientProvider = new Mock<IHttpClientProvider>();
            httpClientProvider.Setup(h => h.GetAsync(Moq.It.IsAny<string>())).Throws(new HttpRequestException());

            Mock<ApacheLivyProvider> providerB = new Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            providerB.Object.SetBaseUrl("www.abc.com");
            var result = await providerB.Object.GetRequestAsync("/batches");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void ApacheLivyProvider_GetRequestAsync_NoResource_ArgumentNullException()
        {
            var policyRegistry = new PolicyRegistry();
            Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Mock<IHttpClientProvider> httpClientProvider = new Mock<IHttpClientProvider>();

            Mock<ApacheLivyProvider> providerB = new Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            providerB.Object.SetBaseUrl("www.abc.com");

            Assert.ThrowsException<ArgumentNullException>(() => providerB.Object.GetRequestAsync(String.Empty).Wait());
        }

        [TestMethod]
        public void ApacheLivyProvider_GetRequestAsync_NoBaseUrl_ArgumentNullException()
        {
            var policyRegistry = new PolicyRegistry();
            Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Mock<IHttpClientProvider> httpClientProvider = new Mock<IHttpClientProvider>();

            Mock<ApacheLivyProvider> providerB = new Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            Assert.ThrowsException<ArgumentNullException>(() => providerB.Object.GetRequestAsync("/batches").Wait());
        }

        [TestMethod]
        public void ApacheLivyProvider_GetRequestAsync_TaskCanceledException()
        {
            var policyRegistry = new PolicyRegistry();
            Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Mock<IHttpClientProvider> httpClientProvider = new Mock<IHttpClientProvider>();
            httpClientProvider.Setup(x => x.GetAsync(It.IsAny<string>())).Throws<TaskCanceledException>();

            Mock<ApacheLivyProvider> providerB = new Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            providerB.Object.SetBaseUrl("www.abc.com");

            AggregateException ex2 = Assert.ThrowsException<AggregateException>(
                () => providerB.Object.GetRequestAsync("/batches").Wait());

            int nullExceptionCnt = 0;

            foreach (var inner in ex2.InnerExceptions)
            {
                if (inner is TaskCanceledException)
                {
                    nullExceptionCnt++;
                }
            }

            Assert.AreEqual(1, nullExceptionCnt);
        }

        [TestMethod]
        public async Task ApacheLivyProvider_PostRequestAsync_BadRequest()
        {

            var policyRegistry = new PolicyRegistry();
            Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Mock<IHttpClientProvider> httpClientProvider = new Mock<IHttpClientProvider>();
            httpClientProvider.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            Mock<ApacheLivyProvider> providerB = new Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            var result = await providerB.Object.PostRequestAsync("/batches", new StringContent(""));

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}
