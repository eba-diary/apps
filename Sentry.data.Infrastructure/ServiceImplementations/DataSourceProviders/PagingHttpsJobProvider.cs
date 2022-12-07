using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using static Sentry.data.Core.GlobalConstants;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Infrastructure
{
    public class PagingHttpsJobProvider : IBaseJobProvider
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientGenerator _httpClientGenerator;
        #endregion

        #region Constructor
        public PagingHttpsJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator) 
        {
            _datasetContext = datasetContext;
            _s3ServiceProvider = s3ServiceProvider;
            _authorizationProvider = authorizationProvider;
            _httpClientGenerator = httpClientGenerator;
        }
        #endregion

        #region IBaseJobProvider Implementation
        public void Execute(RetrieverJob job)
        {
            Logger.Info($"Paging Https Retriever Job start - Job: {job.Id}");
            using (_authorizationProvider)
            {
                HTTPSSource source = (HTTPSSource)job.DataSource;

                using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(job.DataSource.BaseUri.ToString()))
                {
                    httpClient.BaseAddress = source.BaseUri;
                    httpClient.Timeout = new TimeSpan(0, 10, 0);

                    if (source.SourceAuthType.Is<OAuthAuthentication>())
                    {
                        Logger.Info($"Paging Https Retriever Job using OAuth tokens - Job: {job.Id}");
                        foreach (DataSourceToken dataSourceToken in source.Tokens)
                        {
                            RetrieveDataAsync(job, httpClient, dataSourceToken).Wait();
                        }
                    }
                    else
                    {
                        RetrieveDataAsync(job, httpClient, null).Wait();
                    }
                }
            }

            Logger.Info($"Paging Https Retriever Job end - Job: {job.Id}");
        }
        #endregion

        #region Private
        private async Task RetrieveDataAsync(RetrieverJob job, HttpClient httpClient, DataSourceToken dataSourceToken)
        {
            PagingHttpsConfiguration config = SetConfiguration(job, dataSourceToken);
            string tempFile = GetTempFile(job.Id, config.Filename);

            try
            {
                using (FileStream fileStream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    Logger.Info($"Paging Https Retriever Job {tempFile} created - Job: {job.Id}");
                    //loop until no more to retrieve
                    while (!string.IsNullOrEmpty(config.RequestUri))
                    {
                        SetAuthorizationHeader(config, httpClient);

                        //make request
                        Logger.Info($"Paging Https Retriever Job making request to {config.RequestUri} - Job: {job.Id}");
                        using (HttpResponseMessage response = await httpClient.GetAsync(config.RequestUri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                Logger.Info($"Paging Https Retriever Job successful response from {config.RequestUri} - Job: {job.Id}");
                                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                                {
                                    //combined size of file and response is over 2GB, upload file
                                    FlushAccumulatedProgress(fileStream, contentStream, config);

                                    //copy response to file
                                    JObject responseObj = await ReadResponseAsync(contentStream, fileStream, config);

                                    //get next request to make
                                    SetNextPageRequest(config, responseObj);
                                }
                            }
                            else
                            {
                                throw new HttpsJobProviderException($"HTTPS request to {config.RequestUri} failed. {response.Content.ReadAsStringAsync().Result}");
                            }
                        }
                    }

                    Logger.Info($"Paging Https Retriever Job no further requests to make - Job: {config.Job.Id}");

                    //upload to S3 and save progress (last page retrieved had no results)
                    if (fileStream.Length > 0)
                    {
                        string targetKey = $"{config.S3DropStep.TriggerKey}{config.Filename}_{config.PageNumber}.json";
                        _s3ServiceProvider.UploadDataFile(fileStream, config.S3DropStep.TriggerBucket, targetKey);
                        Logger.Info($"Paging Https Retriever Job complete S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
                    }

                    //clear execution parameters because we have made it to the end successfully
                    job.ExecutionParameters = new Dictionary<string, string>();
                    //save cleared execution parameters and incremented variables
                    _datasetContext.SaveChanges();
                    Logger.Info($"Paging Https Retriever Job progress saved - Job: {config.Job.Id}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Paging Https Retriever Job failed retrieving data from {config.RequestUri} - Job: {config.Job.Id}", ex);
                throw;
            }
            finally
            {
                //always clean up temp file because we only save progress after file has been uploaded to S3
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    Logger.Info($"Paging Https Retriever Job {tempFile} deleted - Job: {job.Id}");
                }
            }
        }

        private PagingHttpsConfiguration SetConfiguration(RetrieverJob job, DataSourceToken dataSourceToken)
        {
            PagingHttpsConfiguration config = new PagingHttpsConfiguration
            {
                Job = job,
                Filename = GetFileName(job, dataSourceToken),
                PageNumber = 1,
                Source = (HTTPSSource)job.DataSource,
                S3DropStep = _datasetContext.DataFlowStep.FirstOrDefault(w => w.DataFlow.Id == job.DataFlow.Id && w.DataAction_Type_Id == DataActionType.ProducerS3Drop),
                DataPath = job.FileSchema.SchemaRootPath ?? "",
                Token = dataSourceToken,
                Options = job.JobOptions.HttpOptions
            };

            if (config.Options.PagingType == PagingType.PageNumber && job.ExecutionParameters.ContainsKey(config.Options.PageParameterName))
            {
                ReplaceVariablePlaceholders(config);
                config.PageNumber = int.Parse(job.ExecutionParameters[config.Options.PageParameterName]);
                AddUpdatePageParameter(config, config.PageNumber.ToString());
            }
            else
            {
                SetNextRequestUri(config);
            }

            Logger.Info($"Paging Https Retriever Job starting from {config.RequestUri} - Job: {job.Id}");

            return config;
        }

        private async Task<JObject> ReadResponseAsync(Stream contentStream, FileStream fileStream, PagingHttpsConfiguration config)
        {
            using (StreamReader streamReader = new StreamReader(contentStream))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                JObject responseObj = JObject.Load(jsonReader);

                //get the count of data from response object to determine to continue
                if (responseObj.SelectToken(config.DataPath)?.Any() == true)
                {
                    //if this doesn't write in a single line, we'll write unformatted JObject
                    await contentStream.CopyToAsync(fileStream);
                    Logger.Info($"Paging Https Retriever Job response content copied to temp file - Job: {config.Job.Id}");
                    return responseObj;
                }

                Logger.Info($"Paging Https Retriever Job no data found at {config.DataPath} - Job: {config.Job.Id}");
                return null;
            }
        }

        private string GetFileName(RetrieverJob job, DataSourceToken dataSourceToken)
        {
            string filename = job.JobOptions.TargetFileName;
            if (dataSourceToken != null)
            {
                filename += "_" + dataSourceToken.TokenName ?? dataSourceToken.Id.ToString();
            }

            return filename;
        }

        private void SetAuthorizationHeader(PagingHttpsConfiguration config, HttpClient httpClient)
        {
            if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
            {
                //make sure token is not expired
                string token = _authorizationProvider.GetOAuthAccessToken(config.Source, config.Token);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
            else if (config.Source.SourceAuthType.Is<TokenAuthentication>())
            {
                string token = _authorizationProvider.GetTokenAuthenticationToken(config.Source);
                httpClient.DefaultRequestHeaders.Add(config.Source.AuthenticationHeaderName, token);
            }
        }

        private void SetNextRequestUri(PagingHttpsConfiguration config)
        {
            if (config.Job.TryIncrementRequestVariables())
            {
                ReplaceVariablePlaceholders(config);
            }
            else
            {
                Logger.Info($"Paging Https Retriever Job variables could not be incremented further - Job: {config.Job.Id}");
                config.RequestUri = "";
            }
        }

        private void ReplaceVariablePlaceholders(PagingHttpsConfiguration config)
        {
            config.RequestUri = config.Job.RelativeUri;

            foreach (RequestVariable variable in config.Job.RequestVariables)
            {
                string placeholder = string.Format(Indicators.REQUESTVARIABLEINDICATOR, variable.VariableName);
                config.RequestUri = config.RequestUri.Replace(placeholder, variable.VariableValue);
            }
        }

        private string GetTempFile(int jobId, string filename)
        {
            string tempDirectory = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", jobId.ToString());
            Directory.CreateDirectory(tempDirectory);

            return $@"{tempDirectory}\{filename}.json";
        }

        private void FlushAccumulatedProgress(FileStream fileStream, Stream contentStream, PagingHttpsConfiguration config)
        {
            //combined size of file and response is over 2GB
            if (fileStream.Length > 0 && contentStream.Length + fileStream.Length > Math.Pow(1024, 3) * 2)
            {
                Logger.Info($"Paging Https Retriever Job hit 2GB size threshold - Job: {config.Job.Id}");
                //upload to S3 and save progress
                string targetKey = $"{config.S3DropStep.TriggerKey}{config.Filename}_{config.PageNumber}.json";
                _s3ServiceProvider.UploadDataFile(fileStream, config.S3DropStep.TriggerBucket, targetKey);
                Logger.Info($"Paging Https Retriever Job complete S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");

                if (config.Options.PagingType == PagingType.PageNumber)
                {
                    config.Job.AddOrUpdateExecutionParameter(config.Options.PageParameterName, config.PageNumber.ToString());
                    Logger.Info($"Paging Https Retriever Job added execution parameter {config.Options.PageParameterName}: {config.PageNumber} - Job: {config.Job.Id}");
                }

                _datasetContext.SaveChanges();
                Logger.Info($"Paging Https Retriever Job progress saved - Job: {config.Job.Id}");

                //clear contents of filestream for continued use
                fileStream.SetLength(0);
                fileStream.Flush();
                Logger.Info($"Paging Https Retriever Job temp file reset - Job: {config.Job.Id}");
            }            
        }

        private void SetNextPageRequest(PagingHttpsConfiguration config, JObject responseObj)
        {
            if (responseObj != null)
            {
                config.PageNumber++;
                AddPageTypeParameter(config, responseObj);
            }
            else
            {
                SetNextRequestUri(config);
            }

            Logger.Info($"Paging Https Retriever Job next request {config.RequestUri} - Job: {config.Job.Id}");
        }

        private void AddPageTypeParameter(PagingHttpsConfiguration config, JObject responseObj)
        {
            switch (config.Options.PagingType)
            {
                case PagingType.PageNumber:
                    //add the next page number to uri
                    AddUpdatePageParameter(config, config.PageNumber.ToString());
                    break;
                case PagingType.Token:
                    //get token value and add to uri
                    JToken tokenField = responseObj.SelectToken(config.Options.PageTokenField);
                    if (tokenField != null)
                    {
                        AddUpdatePageParameter(config, tokenField.ToString());
                        break;
                    }

                    throw new HttpsJobProviderException($"The page token value could not be found using '{config.Options.PageTokenField}'");
                default:
                    config.RequestUri = "";
                    break;
            }
        }

        private void AddUpdatePageParameter(PagingHttpsConfiguration config, string parameterValue)
        {
            NameValueCollection parameters;
            List<string> uriParts = config.RequestUri.Split('?').ToList();

            if (uriParts.Count > 1)
            {
                parameters = HttpUtility.ParseQueryString(uriParts.Last());
            }
            else
            {
                parameters = new NameValueCollection();
            }

            parameters.Set(config.Options.PageParameterName, parameterValue);            

            config.RequestUri = $"{uriParts.First()}?{parameters}";
        }
        #endregion

        #region Not Implemented
        public void ConfigureProvider(RetrieverJob job)
        {
            throw new NotImplementedException();
        }

        public void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
