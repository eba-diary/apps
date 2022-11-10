using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Sentry.data.Infrastructure
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IDatasetContext _datasetContext;
        private readonly IDataFeatures _dataFeatures;

        public AuthorizationProvider(IEncryptionService encryptionService, IDatasetContext datasetContext, IDataFeatures dataFeatures)
        {
            _encryptionService = encryptionService;
            _datasetContext = datasetContext;
            _dataFeatures = dataFeatures;
        }

        public string GetOAuthAccessTokenForToken(HTTPSSource source, DataSourceToken token)
        {
            if (token.CurrentToken == null || token.CurrentTokenExp == null || token.CurrentTokenExp < ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds))
            {
                bool isRefreshToken = source.GrantType == Core.GlobalEnums.OAuthGrantType.RefreshToken;
                HttpClientHandler httpHandler = new HttpClientHandler();
                if (WebHelper.TryGetWebProxy(_dataFeatures.CLA3819_EgressEdgeMigration.GetValue(), out WebProxy webProxy))
                {
                    httpHandler.Proxy = webProxy;
                }

                HttpClient httpClient = new HttpClient(httpHandler);

                HttpResponseMessage oAuthPostResult = new HttpResponseMessage();
                if (isRefreshToken)
                {
                    oAuthPostResult = GetOAuthResponseForRefreshToken(source, token, httpClient);
                }
                else
                {
                    oAuthPostResult = GetOAuthResponseForJwt(source, token, httpClient);
                }

                string response = oAuthPostResult.Content.ReadAsStringAsync().Result;

                if (oAuthPostResult.IsSuccessStatusCode)
                {
                    JObject responseAsJson = JObject.Parse(response);
                    string accessToken = responseAsJson.Value<string>("access_token");
                    string expires_in = responseAsJson.Value<string>("expires_in");
                    string token_type = responseAsJson.Value<string>("token_type");

                    Logger.Info($"recieved_oauth_access_token - source:{source.Name} sourceId:{source.Id} expires_in:{expires_in} token_type:{token_type}");

                    DateTime newTokenExp = ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(double.Parse(expires_in.ToString()))).TotalSeconds);
                    if (isRefreshToken)
                    {
                        SaveOAuthToken(source, accessToken, newTokenExp, token, responseAsJson.Value<string>("refresh_token"), responseAsJson.Value<string>("scope"));
                    }
                    else
                    {
                        SaveOAuthToken(source, accessToken, newTokenExp, token);
                    }

                    return accessToken;
                }
                else
                {
                    throw new OAuthException($"Failed to retrieve OAuth Access Token from {token.TokenUrl}. Response: {response}");
                }
            }

            return _encryptionService.DecryptString(token.CurrentToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);
        }

        public string GetOAuthAccessToken(HTTPSSource source)
        {
            return GetOAuthAccessTokenForToken(source, source.Tokens.FirstOrDefault());
        }

        #region Private
        private HttpResponseMessage GetOAuthResponseForJwt(HTTPSSource source, DataSourceToken token, HttpClient httpClient)
        {
            FormUrlEncodedContent oAuthPostContent = GetOAuthContent(source);
            return httpClient.PostAsync(token.TokenUrl, oAuthPostContent).Result;
        }

        private HttpResponseMessage GetOAuthResponseForRefreshToken(HTTPSSource source, DataSourceToken token, HttpClient httpClient)
        {
            var motiveUrl = token.TokenUrl;
            motiveUrl = motiveUrl.Replace("clientid", source.ClientId);
            var privateId = _encryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);
            motiveUrl = motiveUrl.Replace("clientsecret", _encryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));
            motiveUrl = motiveUrl.Replace("refreshtoken", _encryptionService.DecryptString(token.RefreshToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));
            return httpClient.PostAsync(motiveUrl, new StringContent("")).Result;
        }

        private FormUrlEncodedContent GetOAuthContent(HTTPSSource source)
        {
            List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", GenerateJwt(source))
            };

            return new FormUrlEncodedContent(content);
        }

        private string GenerateJwt(HTTPSSource source)
        {
            string claimsJSON = GenerateClaims(source);
            return SignOAuthToken(claimsJSON, source);
        }

        private string GenerateClaims(HTTPSSource source)
        {
            Dictionary<string, object> claims = new Dictionary<string, object>();
            List<OAuthClaim> sourceClaims;

            sourceClaims = _datasetContext.OAuthClaims.Where(w => w.DataSourceId == source).ToList();

            foreach (OAuthClaim claim in sourceClaims)
            {
                switch (claim.Type)
                {
                    case Core.GlobalEnums.OAuthClaims.exp:
                        claims.Add(claim.Type.ToString(), DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(source.Tokens.FirstOrDefault().TokenExp)).TotalSeconds);
                        break;
                    default:
                        claims.Add(claim.Type.ToString(), claim.Value);
                        break;
                }
            }

            //add Issue At time
            claims.Add("iat", DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

            return JsonConvert.SerializeObject(claims);
        }

        private string SignOAuthToken(string claims, HTTPSSource source)
        {
            List<string> segments = new List<string>();

            byte[] header = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" }));
            byte[] payload = Encoding.UTF8.GetBytes(claims);

            segments.Add(Base64UrlEncode(header));
            segments.Add(Base64UrlEncode(payload));

            string stringToSign = string.Join(".", segments.ToArray());

            byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            string privateKey = _encryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);

            byte[] keyBytes = Convert.FromBase64String(privateKey);

            var asymmetricKeyParameter = PrivateKeyFactory.CreateKey(keyBytes);
            var rsaKeyParameter = (RsaKeyParameters)asymmetricKeyParameter;

            ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");

            sig.Init(true, rsaKeyParameter);

            sig.BlockUpdate(bytesToSign, 0, bytesToSign.Length);
            byte[] signature = sig.GenerateSignature();

            segments.Add(Base64UrlEncode(signature));
            return string.Join(".", segments.ToArray());
        }

        private string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        private void SaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime, DataSourceToken token, string newRefreshToken, string newScope)
        {
            //test if this is even needed, or will saving source as is will update
            SaveOAuthToken(source, newToken, tokenExpTime, token);

            token.RefreshToken = _encryptionService.EncryptString(newRefreshToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
            
            if(newScope.Length > 500)
            {
                newScope = newScope.Substring(0, 500);
            }

            token.Scope = newScope;

            _datasetContext.SaveChanges();
        }

        private void SaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime, DataSourceToken token)
        {
            //test if this is even needed, or will saving source as is will update
            HTTPSSource updatedSource = (HTTPSSource)_datasetContext.GetById<DataSource>(source.Id);

            token.CurrentToken = _encryptionService.EncryptString(newToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
            token.CurrentTokenExp = tokenExpTime;

            _datasetContext.SaveChanges();
        }

        private DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }
        #endregion
    }
}
