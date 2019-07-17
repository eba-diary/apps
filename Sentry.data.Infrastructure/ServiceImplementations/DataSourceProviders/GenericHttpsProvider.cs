using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using RestSharp;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class GenericHttpsProvider : BaseHttpsProvider
    {
        private IJobService _jobService;

        public GenericHttpsProvider(IDatasetContext datasetContext,
            IConfigService configService, IEncryptionService encryptionService, IJobService jobService) : base(datasetContext, configService, encryptionService)
        {
            _jobService = jobService;
        }

        public override void Execute(RetrieverJob job)
        {
            string targetFullPath;

            //Set Job
            _job = job;

            ConfigureClient();
            ConfigureRequest();

            IRestResponse resp = SendRequest();

            //Find appropriate drop location (S3Basic or DfsBasic)
            RetrieverJob targetJob = _jobService.FindBasicJob(this._job);

            //Get target path based on basic job found
            string extension = resp.ParseContentType();

            try
            {
                targetFullPath = $"{targetJob.GetTargetPath(_job)}.{extension}";
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "targetjob_gettargetpath_failure", ex);
                throw;
            }

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
                if (targetJob.DataSource.Is<S3Basic>())
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
                    string targetkey = targetFullPath;
                    var versionId = s3Service.UploadDataFile(tempFile, targetkey);

                    _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");

                    //Cleanup temp file if exists
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                else if (targetJob.DataSource.Is<DfsBasic>())
                {
                    _job.JobLoggerMessage("Info", "Sending file to DFS drop location");

                    try
                    {
                        using (Stream filestream = new FileStream(targetFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                        {
                            resp.CopyToStream(filestream);
                        }
                    }
                    catch (WebException ex)
                    {
                        _job.JobLoggerMessage("Error", "Web request return error", ex);
                        _job.JobLoggerMessage("Info", "Performing HTTPS post-failure cleanup.");

                        //Cleanup target file if exists
                        if (File.Exists(targetFullPath))
                        {
                            File.Delete(targetFullPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                        _job.JobLoggerMessage("Info", "Performing HTTPS post-failure cleanup.");

                        //Cleanup target file if exists
                        if (File.Exists(targetFullPath))
                        {
                            File.Delete(targetFullPath);
                        }
                    }
                }
            }
        }

        public override List<IRestResponse> SendPagingRequest()
        {
            throw new NotImplementedException();
        }

        public override IRestResponse SendRequest()
        {
            IRestResponse resp;

            resp = _client.Execute(_request);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                _job.JobLoggerMessage("Error", "failed_request", resp.ErrorException);
                throw new HttpListenerException((int)resp.StatusCode, resp.Content);
            }

            return resp;
        }

        protected override void AddOAuthGrantType(List<KeyValuePair<string, string>> list, HTTPSSource source)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureClient()
        {
            string baseUri = _job.DataSource.BaseUri.ToString();
            
            _client = new RestClient
            {
                BaseUrl = new Uri(baseUri),
                Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"), int.Parse(Configuration.Config.GetHostSetting("SentryWebProxyPort")))
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                }
            };
        }

        protected override void ConfigureOAuth(IRestRequest req, RetrieverJob job)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureRequest()
        {
            _request = new RestRequest();

            _request.Method = Method.GET;

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
    }
}
