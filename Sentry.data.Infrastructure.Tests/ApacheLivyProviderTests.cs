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
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            ApacheLivyProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            /*Setup provider*/
            var StubIHttpClientProvider = MockRepository.GenerateMock<IHttpClientProvider>();
            StubIHttpClientProvider.Stub(a => a.GetAsync(Arg<string>.Is.Anything))
                .Return(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            //Setup provider
            ApacheLivyProvider providerA = MockRepository.GenerateMock<ApacheLivyProvider>(StubIHttpClientProvider, policyRegistry);

            //Act
            var x = await providerA.GetRequestAsync("/batches");

            Assert.AreEqual(x.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ApacheLivyProvider_GetRequestAsync_HttpRequestException()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            ApacheLivyProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            var StubIHttpClientProvider = MockRepository.GenerateMock<IHttpClientProvider>();

            StubIHttpClientProvider.Expect(_ => _.GetAsync(Arg<string>.Is.Anything)).Throw(new HttpRequestException());

            /*Setup provider*/
            ApacheLivyProvider providerA = MockRepository.GenerateMock<ApacheLivyProvider>(StubIHttpClientProvider, policyRegistry);

            //Act
            await providerA.GetRequestAsync("/batches");

            //Assert
            //The method ExpectedException attribute is performing assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ApacheLivyProvider_GetRequestAsync_ArgumentNullException()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            ApacheLivyProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            var StubIHttpClientProvider = MockRepository.GenerateMock<IHttpClientProvider>();

            StubIHttpClientProvider.Expect(_ => _.GetAsync(Arg<string>.Is.Anything)).Throw(new ArgumentNullException());

            /*Setup provider*/
            ApacheLivyProvider providerA = MockRepository.GenerateMock<ApacheLivyProvider>(StubIHttpClientProvider, policyRegistry);

            //Act
            await providerA.GetRequestAsync("/batches");

            //Assert
            //The method ExpectedException attribute is performing assert
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task ApacheLivyProvider_GetRequestAsync_TaskCanceledException()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            ApacheLivyProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            var StubIHttpClientProvider = MockRepository.GenerateMock<IHttpClientProvider>();

            StubIHttpClientProvider.Expect(_ => _.GetAsync(Arg<string>.Is.Anything)).Throw(new TaskCanceledException());

            /*Setup provider*/
            ApacheLivyProvider providerA = MockRepository.GenerateMock<ApacheLivyProvider>(StubIHttpClientProvider, policyRegistry);

            //Act
            await providerA.GetRequestAsync("/batches");

            //Assert
            //The method ExpectedException attribute is performing assert
        }

        [TestMethod]
        public async Task ApacheLivyProvider_PostRequestAsync_BadRequest()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            ApacheLivyProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<ApacheLivyProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            /*Setup provider*/
            var StubIHttpClientProvider = MockRepository.GenerateMock<IHttpClientProvider>();
            StubIHttpClientProvider.Stub(a => a.PostAsync(Arg<string>.Is.Anything, Arg<HttpContent>.Is.Anything))
                .Return(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)));

            //Setup provider
            ApacheLivyProvider providerA = MockRepository.GenerateMock<ApacheLivyProvider>(StubIHttpClientProvider, policyRegistry);

            //Act
            var x = await providerA.PostRequestAsync("/batches", new StringContent(""));

            Assert.AreEqual(x.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}
