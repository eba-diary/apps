using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, null, null, null);

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

            repository.VerifyAll();
        }

        [TestMethod]
        public void GetOAuthAccessToken_ExpiredRefreshToken_Token()
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
            Mock<IEncryptionService> encryptionService = new Mock<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString("CurrentToken", "ENCRYPT", "IVKey")).Returns("DecryptedToken");
            encryptionService.Setup(x => x.DecryptString("EncryptedPrivateId", "ENCRYPT", "IVKey")).Returns("DecryptedPrivateId");
            encryptionService.Setup(x => x.DecryptString("EncryptedRefreshToken", "ENCRYPT", "IVKey")).Returns("DecryptedRefreshToken");
            encryptionService.Setup(x => x.EncryptString("fakeaccesstoken", "ENCRYPT", "IVKey")).Returns(new Tuple<string, string>("EncryptedAccessToken", null));
            encryptionService.Setup(x => x.EncryptString("fakerefreshtoken", "ENCRYPT", "IVKey")).Returns(new Tuple<string, string>("EncryptedRefreshToken", null));

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetResponse("Motive_RefreshToken")),
                StatusCode = HttpStatusCode.OK
            };

            string requestUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=DecryptedRefreshToken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=ClientId&client_secret=DecryptedPrivateId";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = new Mock<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(token.TokenUrl)).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, datasetContext.Object, generator.Object, null);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            string result = authorizationProvider.GetOAuthAccessToken(source, token);
            authorizationProvider.Dispose();

            Assert.AreEqual("fakeaccesstoken", result);
            Assert.AreEqual("EncryptedAccessToken", token.CurrentToken);
            Assert.IsTrue(token.CurrentTokenExp > originalExp);
            Assert.AreEqual("EncryptedRefreshToken", token.RefreshToken);
            Assert.AreEqual("users.read", token.Scope);

            repository.VerifyAll();
        }

        [TestMethod]
        public void GetOAuthAccessToken_ExpiredJwtToken_Token()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                TokenUrl = "https://www.token.com/"
            };

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.JwtBearer,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);
            Mock<IEncryptionService> encryptionService = new Mock<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString(source.ClientPrivateId, "ENCRYPT", "IVKey")).Returns("DecryptedPrivateKey");
            encryptionService.Setup(x => x.EncryptString("fakeaccesstoken", "ENCRYPT", "IVKey")).Returns(new Tuple<string, string>("EncryptedAccessToken", null));
            
            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetResponse("OAuth_JwtToken")),
                StatusCode = HttpStatusCode.OK
            };

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == token.TokenUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = new Mock<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(token.TokenUrl)).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            List<OAuthClaim> claims = new List<OAuthClaim>
            {
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.iss, Value = "ClientId" },
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.aud, Value = "www.token.com" },
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.exp, Value = "100" },
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.scope, Value = "token.scope" }
            };
            datasetContext.SetupGet(x => x.OAuthClaims).Returns(claims.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IAuthorizationSigner> signer = new Mock<IAuthorizationSigner>();
            signer.Setup(x => x.SignOAuthToken(It.IsAny<string>(), "DecryptedPrivateKey")).Returns("SignedToken");

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, datasetContext.Object, generator.Object, signer.Object);

            string result = authorizationProvider.GetOAuthAccessToken(source, token);
            authorizationProvider.Dispose();

            Assert.AreEqual("fakeaccesstoken", result);
            Assert.AreEqual("EncryptedAccessToken", token.CurrentToken);
            Assert.IsTrue(token.CurrentTokenExp > originalExp);
            Assert.IsNull(token.RefreshToken);
            Assert.IsNull(token.Scope);

            repository.VerifyAll();
        }

        [TestMethod]
        public void GetOAuthAccessToken_ExpiredJwtToken_ThrowsOAuthException()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                TokenUrl = "https://www.token.com/"
            };

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.JwtBearer,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);
            Mock<IEncryptionService> encryptionService = new Mock<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString(source.ClientPrivateId, "ENCRYPT", "IVKey")).Returns("DecryptedPrivateKey");
            
            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent("Error"),
                StatusCode = HttpStatusCode.BadRequest
            };

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == token.TokenUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = new Mock<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(token.TokenUrl)).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            List<OAuthClaim> claims = new List<OAuthClaim>
            {
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.iss, Value = "ClientId" },
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.aud, Value = "www.token.com" },
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.exp, Value = "100" },
                new OAuthClaim() { DataSourceId = source, Type = OAuthClaims.scope, Value = "token.scope" }
            };
            datasetContext.SetupGet(x => x.OAuthClaims).Returns(claims.AsQueryable());

            Mock<IAuthorizationSigner> signer = new Mock<IAuthorizationSigner>();
            signer.Setup(x => x.SignOAuthToken(It.IsAny<string>(), "DecryptedPrivateKey")).Returns("SignedToken");

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, datasetContext.Object, generator.Object, signer.Object);

            Assert.ThrowsException<OAuthException>(() => authorizationProvider.GetOAuthAccessToken(source, token), $"Failed to retrieve OAuth Access Token from {token.TokenUrl}. Response: Error");

            Assert.AreEqual("CurrentToken", token.CurrentToken);
            Assert.AreEqual(originalExp, token.CurrentTokenExp);
            Assert.IsNull(token.RefreshToken);
            Assert.IsNull(token.Scope);

            repository.VerifyAll();
        }

        [TestMethod]
        public void GetTokenAuthenticationToken_HTTPSSource_DecryptedAuthenticationTokenValue()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);
            Mock<IEncryptionService> encryptionService = new Mock<IEncryptionService>();
            encryptionService.Setup(x => x.DecryptString("EncryptedTokenValue", "ENCRYPT", "IVKey")).Returns("DecryptedToken");

            AuthorizationProvider authorizationProvider = new AuthorizationProvider(encryptionService.Object, null, null, null);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                AuthenticationTokenValue = "EncryptedTokenValue"
            };

            string result = authorizationProvider.GetTokenAuthenticationToken(source);

            Assert.AreEqual("DecryptedToken", result);

            repository.VerifyAll();
        }

        #region Helpers
        private string GetResponse(string filename)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\{filename}.json"))
            {
                return rdr.ReadToEnd().Replace("\r\n", string.Empty);
            }
        }
        #endregion
    }
}
