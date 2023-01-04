using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Sentry.data.Infrastructure
{
    public sealed class AuthorizationProvider : IAuthorizationProvider
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IDatasetContext _datasetContext;
        private readonly IHttpClientGenerator _httpClientGenerator;
        private readonly IAuthorizationSigner _authorizationSigner;
        private HttpClient _httpClient;

        public AuthorizationProvider(IEncryptionService encryptionService, IDatasetContext datasetContext, IHttpClientGenerator httpClientGenerator, IAuthorizationSigner authorizationSigner)
        {
            _encryptionService = encryptionService;
            _datasetContext = datasetContext;
            _httpClientGenerator = httpClientGenerator;
            _authorizationSigner = authorizationSigner;
        }

        public string GetOAuthAccessToken(HTTPSSource source, DataSourceToken token)
        {
            if (token.CurrentToken == null || token.CurrentTokenExp == null || token.CurrentTokenExp < ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds))
            {
                bool isRefreshToken = source.GrantType == Core.GlobalEnums.OAuthGrantType.RefreshToken;

                HttpClient httpClient = GetHttpClient(token.TokenUrl);

                HttpResponseMessage oAuthPostResult;
                if (isRefreshToken)
                {
                    oAuthPostResult = GetOAuthResponseForRefreshToken(source, token, httpClient);
                }
                else
                {
                    oAuthPostResult = GetOAuthResponseForJwt(source, token, httpClient);
                }

                using (oAuthPostResult)
                {
                    if (oAuthPostResult.IsSuccessStatusCode)
                    {
                        using (Stream contentStream = oAuthPostResult.Content.ReadAsStreamAsync().Result)
                        using (StreamReader streamReader = new StreamReader(contentStream))
                        using (JsonReader jsonReader = new JsonTextReader(streamReader))
                        {
                            JObject responseAsJson = JObject.Load(jsonReader);
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

                            _datasetContext.SaveChanges();
                            return accessToken;
                        }
                    }
                    else
                    {
                        throw new OAuthException($"Failed to retrieve OAuth Access Token from {token.TokenUrl}. Response: {oAuthPostResult.Content.ReadAsStringAsync().Result}");
                    }
                }
            }

            return _encryptionService.DecryptString(token.CurrentToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);
        }

        public string GetTokenAuthenticationToken(HTTPSSource source)
        {
            return _encryptionService.DecryptString(source.AuthenticationTokenValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region Private
        private HttpResponseMessage GetOAuthResponseForJwt(HTTPSSource source, DataSourceToken token, HttpClient httpClient)
        {
            FormUrlEncodedContent oAuthPostContent = GetOAuthContent(source, token);
            return httpClient.PostAsync(token.TokenUrl, oAuthPostContent).Result;
        }

        private HttpResponseMessage GetOAuthResponseForRefreshToken(HTTPSSource source, DataSourceToken token, HttpClient httpClient)
        {
            var tokenUrl = token.TokenUrl;
            tokenUrl = tokenUrl.Replace("clientid", source.ClientId);
            tokenUrl = tokenUrl.Replace("clientsecret", _encryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));
            tokenUrl = tokenUrl.Replace("refreshtoken", _encryptionService.DecryptString(token.RefreshToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));
            return httpClient.PostAsync(tokenUrl, new StringContent("")).Result;
        }

        private FormUrlEncodedContent GetOAuthContent(HTTPSSource source, DataSourceToken token)
        {
            List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", GenerateJwt(source, token))
            };

            return new FormUrlEncodedContent(content);
        }

        private string GenerateJwt(HTTPSSource source, DataSourceToken token)
        {
            string claimsJSON = GenerateClaims(source, token);
            return _authorizationSigner.SignOAuthToken(claimsJSON, _encryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));
        }

        private string GenerateClaims(HTTPSSource source, DataSourceToken token)
        {
            Dictionary<string, object> claims = new Dictionary<string, object>();

            List<OAuthClaim> sourceClaims = _datasetContext.OAuthClaims.Where(w => w.DataSourceId == source).ToList();

            foreach (OAuthClaim claim in sourceClaims)
            {
                switch (claim.Type)
                {
                    case Core.GlobalEnums.OAuthClaims.exp:
                        claims.Add(claim.Type.ToString(), DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(token.TokenExp)).TotalSeconds);
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

        private void SaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime, DataSourceToken token, string newRefreshToken, string newScope)
        {
            SaveOAuthToken(source, newToken, tokenExpTime, token);

            token.RefreshToken = _encryptionService.EncryptString(newRefreshToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
            
            if(newScope.Length > 500)
            {
                newScope = newScope.Substring(0, 500);
            }

            token.Scope = newScope;
        }

        private void SaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime, DataSourceToken token)
        {
            token.CurrentToken = _encryptionService.EncryptString(newToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
            token.CurrentTokenExp = tokenExpTime;
        }

        private DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        private HttpClient GetHttpClient(string tokenUrl)
        {
            if (_httpClient == null)
            {
                _httpClient = _httpClientGenerator.GenerateHttpClient(tokenUrl);
            }

            return _httpClient;
        }
        #endregion
    }
}
