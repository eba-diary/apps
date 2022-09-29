using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Registry;
using Rhino.Mocks;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using Sentry.data.Infrastructure.PollyPolicies;
using System.Threading;
using System;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ApacheLivyProviderTests
    {
        [TestMethod]
        public async Task ApacheLivyProvider_GetRequestAsync_BadRequest()
        {
            var policyRegistry = new PolicyRegistry();
            Moq.Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Moq.Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Moq.Mock<IHttpClientProvider> httpClientProvider = new Moq.Mock<IHttpClientProvider>();
            httpClientProvider.Setup(h => h.GetAsync(Moq.It.IsAny<string>())).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            Moq.Mock<ApacheLivyProvider> providerB = new Moq.Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };
            providerB.Object.SetBaseUrl("www.abc.com");
            var result = await providerB.Object.GetRequestAsync("/batches");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ApacheLivyProvider_GetRequestAsync_HttpRequestException()
        {
            var policyRegistry = new PolicyRegistry();
            Moq.Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Moq.Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Moq.Mock<IHttpClientProvider> httpClientProvider = new Moq.Mock<IHttpClientProvider>();
            httpClientProvider.Setup(h => h.GetAsync(Moq.It.IsAny<string>())).Throws(new HttpRequestException());

            Moq.Mock<ApacheLivyProvider> providerB = new Moq.Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            providerB.Object.SetBaseUrl("www.abc.com");
            var result = await providerB.Object.GetRequestAsync("/batches");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void ApacheLivyProvider_GetRequestAsync_NoResource_ArgumentNullException()
        {
            var policyRegistry = new PolicyRegistry();
            Moq.Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Moq.Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Moq.Mock<IHttpClientProvider> httpClientProvider = new Moq.Mock<IHttpClientProvider>();

            Moq.Mock<ApacheLivyProvider> providerB = new Moq.Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            providerB.Object.SetBaseUrl("www.abc.com");

            Assert.ThrowsException<ArgumentNullException>(() => providerB.Object.GetRequestAsync(String.Empty).Wait());
        }

        [TestMethod]
        public void ApacheLivyProvider_GetRequestAsync_NoBaseUrl_ArgumentNullException()
        {
            var policyRegistry = new PolicyRegistry();
            Moq.Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Moq.Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Moq.Mock<IHttpClientProvider> httpClientProvider = new Moq.Mock<IHttpClientProvider>();

            Moq.Mock<ApacheLivyProvider> providerB = new Moq.Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            Assert.ThrowsException<ArgumentNullException>(() => providerB.Object.GetRequestAsync("/batches").Wait());
        }

        [TestMethod]
        public void ApacheLivyProvider_GetRequestAsync_TaskCanceledException()
        {
            var policyRegistry = new PolicyRegistry();
            Moq.Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Moq.Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Moq.Mock<IHttpClientProvider> httpClientProvider = new Moq.Mock<IHttpClientProvider>();
            httpClientProvider.Setup(x => x.GetAsync(Moq.It.IsAny<string>())).Throws<TaskCanceledException>();

            Moq.Mock<ApacheLivyProvider> providerB = new Moq.Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

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
            Moq.Mock<ApacheLivyProviderPolicy> pollyPolicyLivy = new Moq.Mock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            Moq.Mock<IHttpClientProvider> httpClientProvider = new Moq.Mock<IHttpClientProvider>();
            httpClientProvider.Setup(h => h.PostAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<HttpContent>())).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            Moq.Mock<ApacheLivyProvider> providerB = new Moq.Mock<ApacheLivyProvider>(httpClientProvider.Object, policyRegistry) { CallBase = true };

            var result = await providerB.Object.PostRequestAsync("/batches", new StringContent(""));

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}
