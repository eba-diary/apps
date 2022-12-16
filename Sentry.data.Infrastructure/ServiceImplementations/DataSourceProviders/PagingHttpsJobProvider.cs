using Amazon.Auth.AccessControlPolicy;
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

namespace Sentry.data.Infrastructure
{
    public class PagingHttpsJobProvider : IBaseJobProvider
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientGenerator _httpClientGenerator;
        private readonly IFileProvider _fileProvider;
        #endregion

        #region Constructor
        public PagingHttpsJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator, IFileProvider fileProvider) 
        {
            _datasetContext = datasetContext;
            _s3ServiceProvider = s3ServiceProvider;
            _authorizationProvider = authorizationProvider;
            _httpClientGenerator = httpClientGenerator;
            _fileProvider = fileProvider;
        }
        #endregion

        #region IBaseJobProvider Implementation
        public void Execute(RetrieverJob job)
        {
            Logger.Info($"Paging Https Retriever Job start - Job: {job.Id}");
            using (_authorizationProvider)
            {
                PagingHttpsConfiguration config = SetConfiguration(job);
                using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(config.Source.BaseUri.ToString()))
                {
                    httpClient.BaseAddress = config.Source.BaseUri;
                    httpClient.Timeout = new TimeSpan(0, 10, 0);

                    RetrieveDataAsync(config, httpClient).Wait();
                }
            }

            Logger.Info($"Paging Https Retriever Job end - Job: {job.Id}");
        }
        #endregion

        #region Private
        private async Task RetrieveDataAsync(PagingHttpsConfiguration config, HttpClient httpClient)
        {
            string tempFile = GetTempFile(config.Job.Id, config.Filename);

            try
            {
                using (Stream fileStream = _fileProvider.GetFileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    Logger.Info($"Paging Https Retriever Job {tempFile} created - Job: {config.Job.Id}");
                    //loop until no more to retrieve
                    while (!string.IsNullOrEmpty(config.RequestUri))
                    {
                        //set header each time in case token expires
                        SetAuthorizationHeader(config, httpClient);

                        //make request
                        Logger.Info($"Paging Https Retriever Job making request to {config.RequestUri} - Job: {config.Job.Id}");
                        using (HttpResponseMessage response = await httpClient.GetAsync(config.RequestUri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                Logger.Info($"Paging Https Retriever Job successful response from {config.RequestUri} - Job: {config.Job.Id}");
                                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                                {
                                    //combined size of file and response is over 2GB, upload file
                                    FlushAccumulatedProgress(fileStream, contentStream, config);

                                    //copy response to file
                                    JToken responseObj = await ReadResponseAsync(contentStream, fileStream, config);

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
                        UploadStreamToS3(fileStream, config);
                    }

                    //clear execution parameters because we have made it to the end successfully
                    config.Job.ExecutionParameters = new Dictionary<string, string>();
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
                _fileProvider.DeleteFile(tempFile);
                Logger.Info($"Paging Https Retriever Job {tempFile} deleted - Job: {config.Job.Id}");
            }
        }

        private PagingHttpsConfiguration SetConfiguration(RetrieverJob job)
        {
            PagingHttpsConfiguration config = new PagingHttpsConfiguration
            {
                Job = job,
                PageNumber = 1,
                Source = (HTTPSSource)job.DataSource,
                S3DropStep = _datasetContext.DataFlowStep.FirstOrDefault(w => w.DataFlow.Id == job.DataFlow.Id && w.DataAction_Type_Id == DataActionType.ProducerS3Drop),
                DataPath = job.FileSchema.SchemaRootPath ?? "",
                Filename = job.JobOptions.TargetFileName,
                Options = job.JobOptions.HttpOptions
            };

            bool isBeginning = true;

            if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
            {
                config.OrderedDataSourceTokens = config.Source.Tokens.OrderBy(x => x.Id).ToList();

                //Use token left off at if available, otherwise start from beginning
                if (job.ExecutionParameters.ContainsKey(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID))
                {
                    int tokenId = int.Parse(job.ExecutionParameters[ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID]);
                    config.CurrentDataSourceToken = config.Source.Tokens.First(x => x.Id == tokenId);
                    isBeginning = false;
                }
            }

            if (config.Options.PagingType == PagingType.PageNumber && job.ExecutionParameters.ContainsKey(config.Options.PageParameterName))
            {
                config.PageNumber = int.Parse(job.ExecutionParameters[config.Options.PageParameterName]);
                AddUpdatePageParameter(config, config.PageNumber.ToString());
                isBeginning = false;
            }

            if (isBeginning)
            {
                SetNextRequestUri(config);
            }
            else
            {
                //start from current variables if starting from incomplete previous run
                ReplaceVariablePlaceholders(config);
            }

            Logger.Info($"Paging Https Retriever Job starting from {config.RequestUri} - Job: {job.Id}");

            return config;
        }

        private async Task<JToken> ReadResponseAsync(Stream contentStream, Stream fileStream, PagingHttpsConfiguration config)
        {
            using (StreamReader streamReader = new StreamReader(contentStream))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                JToken response = JToken.Load(jsonReader);

                //get the count of data from response object to determine to continue
                if (response.SelectToken(config.DataPath)?.Any() == true)
                {
                    //if this doesn't write in a single line, we'll write unformatted JObject
                    await contentStream.CopyToAsync(fileStream);
                    Logger.Info($"Paging Https Retriever Job response content copied to temp file - Job: {config.Job.Id}");
                    return response;
                }

                Logger.Info($"Paging Https Retriever Job no data found at {config.DataPath} - Job: {config.Job.Id}");
                return null;
            }
        }

        private void SetAuthorizationHeader(PagingHttpsConfiguration config, HttpClient httpClient)
        {
            if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
            {
                //make sure token is not expired
                string token = _authorizationProvider.GetOAuthAccessToken(config.Source, config.CurrentDataSourceToken);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
            else if (config.Source.SourceAuthType.Is<TokenAuthentication>() && !httpClient.DefaultRequestHeaders.Any())
            {
                string token = _authorizationProvider.GetTokenAuthenticationToken(config.Source);
                httpClient.DefaultRequestHeaders.Add(config.Source.AuthenticationHeaderName, token);
            }
        }

        private void SetNextRequestUri(PagingHttpsConfiguration config)
        {
            if (config.CurrentDataSourceToken != null && config.CurrentDataSourceToken != config.OrderedDataSourceTokens.Last())
            {
                //using OAuth, move to the next token if there are more
                int nextIndex = config.OrderedDataSourceTokens.IndexOf(config.CurrentDataSourceToken) + 1;
                config.CurrentDataSourceToken = config.OrderedDataSourceTokens[nextIndex];

                Logger.Info($"Paging Https Retriever Job using data source token {nextIndex + 1} of {config.OrderedDataSourceTokens.Count} - Job: {config.Job.Id}");
            }
            else if (config.Job.TryIncrementRequestVariables()) //variables can be incremented
            {                
                ReplaceVariablePlaceholders(config);
                
                if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
                {
                    //start from first data source token if using OAuth
                    config.CurrentDataSourceToken = config.OrderedDataSourceTokens.First();
                }
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
            _fileProvider.CreateDirectory(tempDirectory);

            string fullname = $@"{tempDirectory}\{filename}.json";
            _fileProvider.DeleteFile(fullname);

            return fullname;
        }

        private void FlushAccumulatedProgress(Stream fileStream, Stream contentStream, PagingHttpsConfiguration config)
        {
            //combined size of file and response is over 2GB
            if (fileStream.Length > 0 && contentStream.Length + fileStream.Length > Math.Pow(1024, 3) * 2)
            {
                Logger.Info($"Paging Https Retriever Job hit 2GB size threshold - Job: {config.Job.Id}");
                //upload to S3 and save progress
                UploadStreamToS3(fileStream, config);

                if (config.Options.PagingType == PagingType.PageNumber)
                {
                    int completedPageNumber = config.PageNumber - 1;
                    config.Job.AddOrUpdateExecutionParameter(config.Options.PageParameterName, completedPageNumber.ToString());
                    Logger.Info($"Paging Https Retriever Job added execution parameter {config.Options.PageParameterName}: {completedPageNumber} - Job: {config.Job.Id}");
                }

                if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
                {
                    config.Job.AddOrUpdateExecutionParameter(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, config.CurrentDataSourceToken.Id.ToString());
                    Logger.Info($"Paging Https Retriever Job added execution parameter {ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID}: {config.CurrentDataSourceToken.Id} - Job: {config.Job.Id}");
                }

                _datasetContext.SaveChanges();
                Logger.Info($"Paging Https Retriever Job progress saved - Job: {config.Job.Id}");

                //clear contents of filestream for continued use
                fileStream.SetLength(0);
                fileStream.Flush();
                Logger.Info($"Paging Https Retriever Job temp file reset - Job: {config.Job.Id}");
            }
        }

        private void UploadStreamToS3(Stream fileStream, PagingHttpsConfiguration config)
        {
            string targetKey = $"{config.S3DropStep.TriggerKey}{config.Filename}_{DateTime.Now:yyyyMMddHHmmssfff}.json";
            _s3ServiceProvider.UploadDataFile(fileStream, config.S3DropStep.TriggerBucket, targetKey);
            Logger.Info($"Paging Https Retriever Job complete S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
        }

        private void SetNextPageRequest(PagingHttpsConfiguration config, JToken responseObj)    
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

        private void AddPageTypeParameter(PagingHttpsConfiguration config, JToken responseObj)
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
                    if (tokenField != null && !string.IsNullOrEmpty(tokenField.Value<string>()))
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
