using Hangfire;
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
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GenericHttpsProvider : BaseHttpsProvider
    {
        private readonly Lazy<IJobService> _jobService;
        private readonly HttpClientProvider _httpClient;
        protected bool _IsTargetS3;
        protected string _targetPath;

        public GenericHttpsProvider(Lazy<IDatasetContext> datasetContext,
            Lazy<IConfigService> configService, Lazy<IEncryptionService> encryptionService,
            Lazy<IJobService> jobService, IReadOnlyPolicyRegistry<string> policyRegistry, 
            IRestClient restClient, IDataFeatures dataFeatures, HttpClientProvider httpClient) : base(datasetContext, configService, encryptionService, restClient, dataFeatures)
        {
            _httpClient = httpClient;
            _jobService = jobService;
            _providerPolicy = policyRegistry.Get<ISyncPolicy>(PollyPolicyKeys.GenericHttpProviderPolicy);
        }
        protected IJobService JobService
        {
            get { return _jobService.Value; }
        }

        public async Task ExecuteHttpClient(RetrieverJob job)
        {
            //Set Job
            _job = job;


            FindTargetJob();

            //Setup temporary work space for job
            var tempFile = _job.SetupTempWorkSpace();

            if (_job.JobOptions != null && _job.JobOptions.CompressionOptions.IsCompressed)
            {
                _job.JobLoggerMessage("Info", $"Compressed option is detected... Streaming to temp location");

                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(job.GetUri().ToString(), HttpCompletionOption.ResponseHeadersRead);
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (Stream filestream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            await responseStream.CopyToAsync(filestream);
                        }
                    }
                    SetTargetPath(ParseContentType(response.Content.Headers.ContentType.ToString()));

                    //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop locations
                    //Jaws will cleanup the source temporary file after it completes processing file.
                    BackgroundJob.Enqueue<JawsService>(x => x.UncompressRetrieverJob(_job.Id, tempFile));
                }
                catch (Exception ex)
                {
                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                    _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                    //Cleanup target file if exists
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            else
            {
                if (_IsTargetS3)
                {
                    _job.JobLoggerMessage("Info", "Sending file to S3 drop location");

                    try
                    {
                        HttpResponseMessage response = await _httpClient.GetAsync(job.GetUri().ToString(), HttpCompletionOption.ResponseHeadersRead);
                        using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (Stream filestream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                await responseStream.CopyToAsync(filestream);
                            }
                        }
                        SetTargetPath(ParseContentType(response.Content.Headers.ContentType.ToString()));

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
                    // If _targetStep not null,
                    //    Utilizing Trigger bucket since we want to trigger the targetStep identified
                    versionId = _targetStep != null ? s3Service.UploadDataFile(tempFile, _targetStep.TriggerBucket, targetkey) : s3Service.UploadDataFile(tempFile, targetkey);

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
                        HttpResponseMessage response = await _httpClient.GetAsync(job.GetUri().ToString(), HttpCompletionOption.ResponseHeadersRead);
                        using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (Stream filestream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                await responseStream.CopyToAsync(filestream);
                            }
                        }
                        SetTargetPath(ParseContentType(response.Content.Headers.ContentType.ToString()));

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
            }
        }

        public override void Execute(RetrieverJob job)
        {
            if (_dataFeatures.CLA4310_UseHttpClient.GetValue())
            {
                ExecuteHttpClient(job);
            }
            else
            {
                //Set Job
                _job = job;

                ConfigureClient();
                ConfigureRequest();

                IRestResponse resp = SendRequest();

                FindTargetJob();

                SetTargetPath(ParseContentType(resp.ContentType));


                //Setup temporary work space for job
                var tempFile = _job.SetupTempWorkSpace();

                if (_job.JobOptions != null && _job.JobOptions.CompressionOptions.IsCompressed)
                {
                    _job.JobLoggerMessage("Info", $"Compressed option is detected... Streaming to temp location");

                    try
                    {
                        using (Stream filestream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            resp.CopyToStream(filestream);
                        }

                        //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop locations
                        //Jaws will cleanup the source temporary file after it completes processing file.
                        BackgroundJob.Enqueue<JawsService>(x => x.UncompressRetrieverJob(_job.Id, tempFile));
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                        _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                        //Cleanup target file if exists
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                    }
                }
                else
                {
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
                        // If _targetStep not null,
                        //    Utilizing Trigger bucket since we want to trigger the targetStep identified
                        versionId = _targetStep != null ? s3Service.UploadDataFile(tempFile, _targetStep.TriggerBucket, targetkey) : s3Service.UploadDataFile(tempFile, targetkey);

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
                }
            }
        }

        public override void Execute(RetrieverJob job, string filePath)
        {
            //This is not utilized by this type
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
                throw new RetrieverJobProcessingException($"Failed processing https request - response:{resp.Content}");
            }

            return resp;
        }

        protected override void AddOAuthGrantType(List<KeyValuePair<string, string>> list, HTTPSSource source)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureClient()
        {
            string methodName = $"{nameof(GenericHttpsDataFlowProvider).ToLower()}_{nameof(ConfigureClient).ToLower()}";
            Logger.Debug($"{methodName} Method Start");

            string baseUri = _job.DataSource.BaseUri.ToString();

            NetworkCredential proxyCredentials;
            string proxyUrl;

            if (_dataFeatures.CLA3819_EgressEdgeMigration.GetValue())
            {
                Logger.Debug($"{methodName} using edge proxy: true");
                string userName = Configuration.Config.GetHostSetting("ServiceAccountID");
                string password = Configuration.Config.GetHostSetting("ServiceAccountPassword");
                proxyUrl = Configuration.Config.GetHostSetting("EdgeWebProxyUrl");
                proxyCredentials = new NetworkCredential(userName, password);
            }
            else
            {
                Logger.Debug($"{methodName} using edge proxy: false");
                proxyUrl = Configuration.Config.GetHostSetting("WebProxyUrl");
                proxyCredentials = CredentialCache.DefaultNetworkCredentials;
            }

            Logger.Debug($"{methodName} proxyUser: {proxyCredentials.UserName}");

            _client = new RestClient
            {
                BaseUrl = new Uri(baseUri),
                Proxy = new WebProxy(proxyUrl)
                {
                    Credentials = proxyCredentials
                }
            };

            Logger.Debug($"{methodName} Method End");
        }

        protected override void ConfigureOAuth(IRestRequest req, RetrieverJob job)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureRequest()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            _request = new RestRequest();
#pragma warning restore IDE0017 // Simplify object initialization
            _request.Method = Method.GET;
            _request.Resource = _job.GetUri().ToString();

            //Add datasource specific headers to request
            List<RequestHeader> headerList = ((HTTPSSource)_job.DataSource).RequestHeaders;

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
        }

        protected override string GenerateJwtToken(HTTPSSource source)
        {
            throw new NotImplementedException();
        }

        protected override string GetOAuthAccessToken(HTTPSSource source)
        {
            throw new NotImplementedException();
        }

        protected override string SignOAuthToken(string claims, HTTPSSource source)
        {
            throw new NotImplementedException();
        }

        protected override void FindTargetJob()
        {
            //Find appropriate drop location (S3Basic or DfsBasic)
            _targetJob = JobService.FindBasicJob(this._job);

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
