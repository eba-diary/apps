﻿using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RestSharp;
using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Sentry.data.Infrastructure
{
    public class HttpsProvider : IHttpsProvider
    {
        #region Declarations
        private RestRequest _request;
        private Uri _uri;
        private IDatasetContext _dsContext;
        private IConfigService _configService;
        private IEncryptionService _encryptionService;
        #endregion

        public HttpsProvider(IDatasetContext datasetContext, IConfigService configService, IEncryptionService encryptionService)
        {
            _dsContext = datasetContext;
            _configService = configService;
            _encryptionService = encryptionService;
        }

        public void ConfigureProvider(RetrieverJob job, List<KeyValuePair<string,string>> headers)
        {
            _uri = job.GetUri();
            _request = new RestSharp.RestRequest();

            if (job.DataSource.SourceAuthType.Is<TokenAuthentication>())
            {
                EncryptionService encryptService = new EncryptionService();
                _request.AddHeader(((HTTPSSource)job.DataSource).AuthenticationHeaderName, encryptService.DecryptString(((HTTPSSource)job.DataSource).AuthenticationTokenValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)job.DataSource).IVKey));
            }

            if (job.DataSource.SourceAuthType.Is<OAuthAuthentication>())
            {
                HTTPSSource source = (HTTPSSource)job.DataSource;

                var token = GetAccessToken(source);

                if (source.GrantType == Core.GlobalEnums.OAuthGrantType.jwtbearer)
                {
                    _request.AddHeader("Authorization", "Bearer " + token);
                }
            }
            
            //Add datasource specific headers to request
            List<RequestHeader> headerList = ((HTTPSSource)job.DataSource).RequestHeaders;

            if (headerList != null)
            {
                foreach (RequestHeader header in headerList)
                {
                    switch (header.Key.ToUpper())
                    {
                        case "ACCEPT":
                            _request.AddHeader("Accept", header.Value);
                            break;
                        case "EXPECT":
                            _request.AddHeader("Except", header.Value);
                            break;
                        default:
                            _request.AddHeader(header.Key, header.Value);
                            break;
                    }
                }
            }

            _request.Resource = _uri.ToString();
        }

        public IRestResponse SendRequest()
        {
            RestClient client = new RestClient
            {
                Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"))
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                }
            };
            
            return client.ExecuteAsGet(_request, "GET");                    
        }

        public void CopyToStream(Stream targetStream)
        {
            RestClient client = new RestClient
            {
                Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"))
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                }
            };

            _request.ResponseWriter = (responseStream) => responseStream.CopyTo(targetStream);

            client.DownloadData(_request);
        }


        #region Private Methods
        private string Sign(string claims, HTTPSSource source)
        {
            List<string> segments = new List<string>();

            byte[] header = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" }));
            byte[] payload = Encoding.UTF8.GetBytes(claims);

            segments.Add(Base64UrlEncode(header));
            segments.Add(Base64UrlEncode(payload));

            string stringToSign = string.Join(".", segments.ToArray());

            byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            byte[] keyBytes = Convert.FromBase64String(_encryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));

            var asymmetricKeyParameter = PrivateKeyFactory.CreateKey(keyBytes);
            var rsaKeyParameter = (RsaKeyParameters)asymmetricKeyParameter;

            ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");

            sig.Init(true, rsaKeyParameter);

            sig.BlockUpdate(bytesToSign, 0, bytesToSign.Length);
            byte[] signature = sig.GenerateSignature();

            segments.Add(Base64UrlEncode(signature));
            return string.Join(".", segments.ToArray());
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        private object GetAccessToken(HTTPSSource source)
        {
            if (source.CurrentTokenExp < ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds))
            {
                // Get the OAuth access token
                var httpClient = new System.Net.Http.HttpClient();
                var keyValues = new List<KeyValuePair<string, string>>();
                AddGrantType(keyValues, source);
                keyValues.Add(new KeyValuePair<string, string>("assertion", GenerateJwtToken(source)));
                var oAuthPostContent = new System.Net.Http.FormUrlEncodedContent(keyValues);
                var oAuthPostResult = httpClient.PostAsync(source.TokenUrl, oAuthPostContent).Result;
                var response = oAuthPostResult.Content.ReadAsStringAsync().Result;
                var responseAsJson = Newtonsoft.Json.Linq.JObject.Parse(response);
                var accessToken = responseAsJson.GetValue("access_token");

                DateTime newTokenExp = ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(double.Parse(source.TokenExp))).TotalSeconds);

                bool result = _configService.UpdateandSaveOAuthToken(source, accessToken.ToString(), newTokenExp);

                return accessToken;
            }
            else
            {
                return source.CurrentToken;
            }
        }

        private void AddGrantType(List<KeyValuePair<string, string>> list, HTTPSSource source)
        {
            switch (source.GrantType)
            {
                case Core.GlobalEnums.OAuthGrantType.jwtbearer:
                    list.Add(new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"));
                    break;
                default:
                    break;
            }
        }

        private string GenerateJwtToken(HTTPSSource source)
        {
            string claimsJSON = GenerateClaims(source);
            return Sign(claimsJSON, source);
        }

        private string GenerateClaims(HTTPSSource source)
        {
            Dictionary<string, object> claims = new Dictionary<string, object>();
            List<OAuthClaim> sourceClaims;

            sourceClaims = _dsContext.OAuthClaims.Where(w => w.DataSourceId == source).ToList();

            foreach (OAuthClaim claim in sourceClaims)
            {
                switch (claim.Type)
                {
                    case Core.GlobalEnums.OAuthClaims.exp:
                        claims.Add(claim.Type.ToString(), DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(double.Parse(source.TokenExp))).TotalSeconds);
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

        private static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }
        #endregion
    }
}
