﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Polly;
using Polly.Registry;
using RestSharp;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Sentry.data.Infrastructure
{
    public class GoogleApiProvider : BaseHttpsProvider, IGoogleApiProvider
    {
        private readonly Lazy<IJobService> _jobService;
        private readonly string _nextVal = "0";
        protected bool _IsTargetS3;
        protected string _targetPath;

        public GoogleApiProvider(Lazy<IDatasetContext> datasetContext,
            Lazy<IConfigService> configService, Lazy<IEncryptionService> encryptionService, 
            Lazy<IJobService> jobService, IReadOnlyPolicyRegistry<string> policyRegistry,
            IRestClient restClient) : base(datasetContext, configService, encryptionService, restClient)
        {
            _jobService = jobService;
            _providerPolicy = policyRegistry.Get<ISyncPolicy>(PollyPolicyKeys.GoogleAPiProviderPolicy);
        }

        protected IJobService JobService
        {
            get { return _jobService.Value; }
        }

        protected override void ConfigureClient()
        {
            string baseUri = _job.DataSource.BaseUri.ToString();

            string Find = "/";
            string Replace = "";
            int place = baseUri.LastIndexOf(Find);
            if (place != -1)
            {
                baseUri = baseUri.Remove(place, Find.Length).Insert(place, Replace);
            }                

            _client = new RestClient
            {
                BaseUrl = new Uri(baseUri),
                Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"), int.Parse(Configuration.Config.GetHostSetting("SentryWebProxyPort")))
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                }
            };
        }

        protected override void ConfigureRequest()
        {
            _request = new RestRequest();

            ConfigureOAuth(_request, _job);

            switch (_job.JobOptions.HttpOptions.RequestMethod)
            {
                case HttpMethods.get:
                    _request.Method = Method.GET;
                    _request.Resource = _job.GetUri().ToString();
                    break;
                case HttpMethods.post:
                    _request.Method = Method.POST;
                    _request.Resource = "/" + _job.RelativeUri;

                    switch (_job.JobOptions.HttpOptions.RequestDataFormat)
                    {
                        case HttpDataFormat.json:
                            _request.RequestFormat = DataFormat.Json;
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }

        }

        public override void Execute(RetrieverJob job)
        {
            //Set Job
            _job = job;

            ConfigureClient();
            ConfigureRequest();

            //TODO: Revisit when GOOGLEAPI data source needs paging implemented
            //do
            //{
            ConfigurePaging();                

            IRestResponse resp = SendRequest();

            FindTargetJob();

            SetTargetPath(resp.ParseContentType());
                
            /*
                * Google API will not return compressed files, therefore, no need to check whether job
                * has compression set.
            */

            //Setup temporary work space for job
            var tempFile = _job.SetupTempWorkSpace();

            if (_IsTargetS3)
            {
                _job.JobLoggerMessage("Info", "Sending file to S3 drop location");

                try
                {
                    using (Stream filestream = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        resp.CopyToStream(filestream);
                    }
                }
                catch (Exception ex)
                {
                    _job.JobLoggerMessage("Error", "Retriever job failed streaming temp location.", ex);
                    _job.JobLoggerMessage("Info", "Performing HTTPS post-failure cleanup.");

                    //Cleanup temp file if exists
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }

                S3ServiceProvider s3Service = new S3ServiceProvider();
                string targetkey = _targetPath;

                string versionId;
                //Need to handle both a retrieverjob target (legacy platform) and 
                //  S3Drop or ProducerS3Drop (new processing platform) data flow steps
                //  as targets.
                if (_targetStep != null)
                {
                    versionId = s3Service.UploadDataFile(tempFile, _targetStep.Action.TargetStorageBucket, targetkey);
                }
                else
                {
                   versionId = s3Service.UploadDataFile(tempFile, targetkey);
                }

                _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");

                //Cleanup temp file if exists
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            else
            {
                _job.JobLoggerMessage("Info", "Sending file to DFS drop location");

                try
                {
                    using (Stream filestream = new FileStream(_targetPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                    {                            
                        resp.CopyToStream(filestream);
                    }
                }
                catch (WebException ex)
                {
                    _job.JobLoggerMessage("Error", "Web request return error", ex);
                    _job.JobLoggerMessage("Info", "Performing HTTPS post-failure cleanup.");

                    //Cleanup target file if exists
                    if (File.Exists(_targetPath))
                    {
                        File.Delete(_targetPath);
                    }
                }
                catch (Exception ex)
                {
                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                    _job.JobLoggerMessage("Info", "Performing HTTPS post-failure cleanup.");

                    //Cleanup target file if exists
                    if (File.Exists(_targetPath))
                    {
                        File.Delete(_targetPath);
                    }
                }
            }
                
            //TODO: Revisit when GOOGLEAPI data source needs paging implemented
            //} while (_hasNext);
        }

        public override void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }

        public override List<IRestResponse> SendPagingRequest()
        {
            throw new NotImplementedException();
        }

        public override IRestResponse SendRequest()
        {
            IRestResponse resp;

            resp = _providerPolicy.Execute(() =>
            {
                IRestResponse response = _client.Execute(Request);

                if (response.ErrorException != null)
                {
                    const string message = "Error retrieving response";
                    var ex = new RetrieverJobProcessingException(message, response.ErrorException);
                    throw ex;
                }

                return response;
            });

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                _job.JobLoggerMessage("Error", "failed_request", resp.ErrorException);
                throw new HttpListenerException((int)resp.StatusCode, resp.Content);
            }

            return resp;
        }

        protected override void ConfigureOAuth(IRestRequest req, RetrieverJob job)
        {
            HTTPSSource source = (HTTPSSource)job.DataSource;

            if (source.GrantType == Core.GlobalEnums.OAuthGrantType.jwtbearer)
            {
                req.AddHeader("Authorization", "Bearer " + GetOAuthAccessToken(source));
            }
        }

        protected override string GetOAuthAccessToken(HTTPSSource source)
        {
            if (source.CurrentToken == null || source.CurrentTokenExp == null || source.CurrentTokenExp < ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds))
            {
               var httpHandler = new System.Net.Http.HttpClientHandler()
                {
                    Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"), int.Parse(Configuration.Config.GetHostSetting("SentryWebProxyPort")))
                    {
                        Credentials = CredentialCache.DefaultNetworkCredentials
                    }
                };

                var httpClient = new System.Net.Http.HttpClient(httpHandler);

                var keyValues = new List<KeyValuePair<string, string>>();
                AddOAuthGrantType(keyValues, source);
                keyValues.Add(new KeyValuePair<string, string>("assertion", GenerateJwtToken(source)));
                var oAuthPostContent = new System.Net.Http.FormUrlEncodedContent(keyValues);
                var oAuthPostResult = httpClient.PostAsync(source.TokenUrl, oAuthPostContent).Result;
                var response = oAuthPostResult.Content.ReadAsStringAsync().Result;
                var responseAsJson = Newtonsoft.Json.Linq.JObject.Parse(response);
                var accessToken = responseAsJson.GetValue("access_token");
                var expires_in = responseAsJson.GetValue("expires_in");
                var token_type = responseAsJson.GetValue("token_type");

                Logger.Info($"recieved_oauth_access_token - source:{source.Name} sourceId:{source.Id} expires_in:{expires_in} token_type:{token_type}");

                DateTime newTokenExp = ConvertFromUnixTimestamp(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(double.Parse(expires_in.ToString()))).TotalSeconds);

                bool result = ConfigService.UpdateandSaveOAuthToken(source, accessToken.ToString(), newTokenExp);

                return accessToken.ToString();
            }
            else
            {
                return EncryptionService.DecryptString(source.CurrentToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);
            }
        }

        protected override void AddOAuthGrantType(List<KeyValuePair<string, string>> list, HTTPSSource source)
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

        protected override string GenerateJwtToken(HTTPSSource source)
        {
            string claimsJSON = GenerateClaims(source);
            return SignOAuthToken(claimsJSON, source);
        }

        protected override string SignOAuthToken(string claims, HTTPSSource source)
        {
            List<string> segments = new List<string>();

            byte[] header = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" }));
            byte[] payload = Encoding.UTF8.GetBytes(claims);

            segments.Add(Base64UrlEncode(header));
            segments.Add(Base64UrlEncode(payload));

            string stringToSign = string.Join(".", segments.ToArray());

            byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            string x = EncryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey);

            byte[] keyBytes = Convert.FromBase64String(EncryptionService.DecryptString(source.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey));

            var asymmetricKeyParameter = PrivateKeyFactory.CreateKey(keyBytes);
            var rsaKeyParameter = (RsaKeyParameters)asymmetricKeyParameter;

            ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");

            sig.Init(true, rsaKeyParameter);

            sig.BlockUpdate(bytesToSign, 0, bytesToSign.Length);
            byte[] signature = sig.GenerateSignature();

            segments.Add(Base64UrlEncode(signature));
            return string.Join(".", segments.ToArray());
        }

        private string AddPageToken(string body)
        {
            JObject x = JObject.Parse(body);
            JArray reportReqeusts = (JArray)x["reportRequests"];
            JObject reportReq = (JObject)reportReqeusts[0];
            reportReq.Add("pageToken", _nextVal);

            return x.ToString();
        }

        private string AddPageSize(string body)
        {
            JObject x = JObject.Parse(body);
            JArray reportReqeusts = (JArray)x["reportRequests"];
            JObject reportReq = (JObject)reportReqeusts[0];
            reportReq.Add("pageSize", 1);

            return x.ToString();
        }

        //TODO: Revisit when GOOGLEAPI data source needs paging implemented
        private void ConfigurePaging()
        {
            if (_job.JobOptions.HttpOptions.Body != null)
            {
                string requestBody = _job.JobOptions.HttpOptions.Body;

            //    if (((GoogleApiSource)_job.DataSource).PagingEnabled)
            //    {
            //        ConfigurePaging();
            //        requestBody = AddPageSize(requestBody);
            //        if (_nextVal != "0")
            //        {
            //            requestBody = AddPageToken(requestBody);
            //        };
            //    }
                _request.AddJsonBody(requestBody);
            }
        }

        protected override void FindTargetJob()
        {
            //Find appropriate drop location (S3Basic or DfsBasic)
            _targetJob = JobService.FindBasicJob(_job);

            _IsTargetS3 = _targetJob.DataSource.Is<S3Basic>();
        }

        protected override void SetTargetPath(string extension)
        {
            try
            {
                _targetPath = $"{_targetJob.GetTargetPath(_job)}.{extension}";
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "targetjob_gettargetpath_failure", ex);
                throw;
            }
        }
    }
}
