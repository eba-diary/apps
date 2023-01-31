using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
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
        public async Task MotiveOnBoardingAsync()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                RefreshToken = "EncryptedRefreshToken",
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);
            Mock<IEncryptionService> encryptionService = repository.Create<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString("EncryptedPrivateId", "ENCRYPT", "IVKey")).Returns("DecryptedPrivateId");
            encryptionService.Setup(x => x.DecryptString("EncryptedRefreshToken", "ENCRYPT", "IVKey")).Returns("DecryptedRefreshToken");
            encryptionService.Setup(x => x.EncryptString("fakeaccesstoken", "ENCRYPT", "IVKey")).Returns(new Tuple<string, string>("EncryptedAccessToken", null));
            encryptionService.Setup(x => x.EncryptString("fakerefreshtoken", "ENCRYPT", "IVKey")).Returns(new Tuple<string, string>("EncryptedRefreshToken", null));

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
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repository.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(token.TokenUrl)).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            MotiveProvider motiveProvider = new MotiveProvider(null, null, datasetContext.Object, null, encryptionService.Object, null);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            await motiveProvider.MotiveOnboardingAsync(source, token, 999);

            Assert.AreEqual("COMPANY NAME", token.TokenName);
            Assert.AreEqual("KT1111111", token.Id);

            repository.VerifyAll();
        }
    }
}
