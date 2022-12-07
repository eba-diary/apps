using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class AuthorizationProviderTests
    {
        [TestMethod]
        public void GetOAuthAccessToken_NotExpired_Token()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);
            Mock<IEncryptionService> encryptionService = new Mock<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString("CurrentToken", "ENCRYPT", "IVKey")).Returns("DecryptedToken");

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, null, null);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey"
            };

            DataSourceToken token = new DataSourceToken
            {
                CurrentToken = "CurrentToken",
                CurrentTokenExp = DateTime.UtcNow.AddMinutes(10)
            };

            string result = authorizationProvider.GetOAuthAccessToken(source, token);

            Assert.AreEqual("DecryptedToken", result);
        }

        [TestMethod]
        public void GetOAuthAccessToken_ExpiredRefreshToken_Token()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);
            Mock<IEncryptionService> encryptionService = new Mock<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString("CurrentToken", "ENCRYPT", "IVKey")).Returns("DecryptedToken");
            encryptionService.Setup(x => x.DecryptString("EncryptedPrivateId", "ENCRYPT", "IVKey")).Returns("DecryptedPrivateId");
            encryptionService.Setup(x => x.DecryptString("EncryptedRefreshToken", "ENCRYPT", "IVKey")).Returns("DecryptedRefreshToken");

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetMotiveResponse("RefreshToken")),
                StatusCode = HttpStatusCode.OK
            };

            string requestUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=DecryptedRefreshToken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=ClientId&client_secret=DecryptedPrivateId";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            Mock<IHttpClientGenerator> generator = new Mock<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient("TokenUrl")).Returns(httpClient);

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, null, null);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = Core.GlobalEnums.OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            DataSourceToken token = new DataSourceToken
            {
                CurrentToken = "CurrentToken",
                CurrentTokenExp = DateTime.UtcNow.AddMinutes(-1),
                RefreshToken = "EncryptedRefreshToken",
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
            };

            string result = authorizationProvider.GetOAuthAccessToken(source, token);

            Assert.AreEqual("DecryptedToken", result);
        }

        #region Helpers
        private string GetMotiveResponse(string filename)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\Motive_{filename}.json"))
            {
                return rdr.ReadToEnd().Replace("\r\n", string.Empty);
            }
        }
        #endregion
    }
}
