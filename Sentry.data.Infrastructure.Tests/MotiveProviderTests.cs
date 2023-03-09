using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class MotiveProviderTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public async Task MotiveOnBoardingAsync_Expected()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                TokenName = "OriginalName",
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                RefreshToken = "EncryptedRefreshToken",
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetDataString("Motive_Companies.json")),
                StatusCode = HttpStatusCode.OK
            };

            string requestUrl = "https://api.keeptruckin.com/v1/companies";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            Mock<IAuthorizationProvider> authProvider = repository.Create<IAuthorizationProvider>();
            authProvider.Setup(x => x.GetOAuthAccessToken(source, token)).Returns("ACCESS_TOKEN");

            Mock<IDataFlowService> dataFlowService = repository.Create<IDataFlowService>();
            dataFlowService.Setup(x => x.GetDataFlowStepForDataFlowByActionType(999, DataActionType.S3Drop)).Returns(new DataFlowStep() { TriggerBucket = "TestBucket", TriggerKey = "TestKey" });

            Mock<IS3ServiceProvider> s3serviceProvider = repository.Create<IS3ServiceProvider>();
            s3serviceProvider.Setup(x => x.UploadDataFile(It.IsAny<MemoryStream>(), "TestBucket", "TestKey")).Returns("Uploaded");

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IDataFeatures> dataFeatures = repository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4485_DropCompaniesFile.GetValue()).Returns(true);

            MotiveProvider motiveProvider = new MotiveProvider(httpClient, s3serviceProvider.Object, datasetContext.Object, dataFlowService.Object, authProvider.Object, dataFeatures.Object, null);

            await motiveProvider.MotiveOnboardingAsync(source, token, 999);

            Assert.AreEqual("COMPANY NAME", token.TokenName);
            
            repository.VerifyAll();
        }

        [TestMethod]
        public async Task MotiveOnBoardingAsync_Unexpected()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                TokenName = "OriginalName",
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                RefreshToken = "EncryptedRefreshToken",
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetDataString("Motive_Companies_Unexpected.json")),
                StatusCode = HttpStatusCode.OK
            };

            string requestUrl = "https://api.keeptruckin.com/v1/companies";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            Mock<IAuthorizationProvider> authProvider = repository.Create<IAuthorizationProvider>();
            authProvider.Setup(x => x.GetOAuthAccessToken(source, token)).Returns("ACCESS_TOKEN");

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IDataFeatures> dataFeatures = repository.Create<IDataFeatures>();

            MotiveProvider motiveProvider = new MotiveProvider(httpClient, null, datasetContext.Object, null, authProvider.Object, dataFeatures.Object, null);

            await motiveProvider.MotiveOnboardingAsync(source, token, 999);

            Assert.AreEqual("OriginalName", token.TokenName);

            repository.VerifyAll();
        }
    }
}
