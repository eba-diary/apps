using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.Registry;
using RestSharp;
using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Infrastructure.PollyPolicies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class RetrieverJobTests
    {
        [TestMethod]
        public void GoogleApiProvider_SendRequest_Returns_OK_Response()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            GoogleApiProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<GoogleApiProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            ///
            /// Setup provider
            ///

            var stubRestClient = MockRepository.GenerateMock<IRestClient>();
            IRestRequest req = new RestRequest() { Method = Method.GET };
            IRestResponse resp = new RestResponse() { StatusCode = System.Net.HttpStatusCode.OK };
            stubRestClient.Stub(a => a.Execute(Arg<IRestRequest>.Is.Anything))
                .Return(resp);

            ////Setup provider
            ///
            Lazy<IDatasetContext> mockDatasetContext = MockRepository.GenerateMock<Lazy<IDatasetContext>>();
            Lazy<IConfigService> mockConfigService = MockRepository.GenerateMock<Lazy<IConfigService>>();
            Lazy<IEncryptionService> mockEncryptionService = MockRepository.GenerateMock<Lazy<IEncryptionService>>();
            Lazy<IJobService> mockJobService = MockRepository.GenerateMock<Lazy<IJobService>>();
            BaseHttpsProvider baseProvider = MockRepository.GenerateMock<BaseHttpsProvider>(mockDatasetContext, mockConfigService, mockEncryptionService, stubRestClient);

            GoogleApiProvider googleApiProvider = new GoogleApiProvider(mockDatasetContext, mockConfigService, mockEncryptionService, mockJobService, policyRegistry, stubRestClient);
            baseProvider.Stub(a => a.Request).Return(req);

            ////
            ///Act
            ///
            IRestResponse x = googleApiProvider.SendRequest();

            ///
            /// Assert
            ///
            Assert.AreEqual(x.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        [ExpectedException(typeof(RetrieverJobProcessingException))]
        public void GoogleApiProvider_SendRequest_Returns_BadRequest_Response()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            GoogleApiProviderPolicy pollyPolicyLivy = MockRepository.GenerateMock<GoogleApiProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Register();

            ///
            /// Setup provider
            ///

            var stubRestClient = MockRepository.GenerateMock<IRestClient>();
            IRestRequest req = new RestRequest() { Method = Method.GET };
            var mockResp = MockRepository.GenerateMock<IRestResponse>();
            mockResp.Stub(s => s.StatusCode).Return(HttpStatusCode.BadRequest);
            mockResp.Stub(s => s.ErrorException).Return(new Exception("Exception message"));
            IRestResponse resp = new RestResponse() { StatusCode = System.Net.HttpStatusCode.BadRequest };

            stubRestClient.Stub(a => a.Execute(Arg<IRestRequest>.Is.Anything))
                .Return(mockResp);

            ////Setup provider
            ///
            Lazy<IDatasetContext> mockDatasetContext = MockRepository.GenerateMock<Lazy<IDatasetContext>>();
            Lazy<IConfigService> mockConfigService = MockRepository.GenerateMock<Lazy<IConfigService>>();
            Lazy<IEncryptionService> mockEncryptionService = MockRepository.GenerateMock<Lazy<IEncryptionService>>();
            Lazy<IJobService> mockJobService = MockRepository.GenerateMock<Lazy<IJobService>>();
            BaseHttpsProvider baseProvider = MockRepository.GenerateMock<BaseHttpsProvider>(mockDatasetContext, mockConfigService, mockEncryptionService, stubRestClient);

            GoogleApiProvider googleApiProvider = new GoogleApiProvider(mockDatasetContext, mockConfigService, mockEncryptionService, mockJobService, policyRegistry, stubRestClient);
            baseProvider.Stub(a => a.Request).Return(req);

            ////
            ///Act
            ///
            IRestResponse x = googleApiProvider.SendRequest();

            ///
            /// Assert
            /// Method is decorated with expected exception
            ///
        }
    }
}
