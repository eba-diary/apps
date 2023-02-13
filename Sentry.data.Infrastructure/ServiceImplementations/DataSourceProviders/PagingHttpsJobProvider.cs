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
using System.Text;
using System.Text.RegularExpressions;
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

            if (job.HasValidRequestVariables())
            {
                using (_authorizationProvider)
                {
                    PagingHttpsConfiguration config = SetConfiguration(job);
                    using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(config.Source.BaseUri.ToString()))
                    {
                        httpClient.Timeout = new TimeSpan(0, 10, 0);

                        foreach (RequestHeader header in config.Source.RequestHeaders)
                        {
                            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }

                        RetrieveDataAsync(config, httpClient).Wait();
                    }
                }
            }
            else
            {
                Logger.Info($"Paging Https Retriever Job request variables were invalid to execute - Job: {job.Id}");
            }

            Logger.Info($"Paging Https Retriever Job end - Job: {job.Id}");
        }

        #region Not Implemented
        public void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }

        public void ConfigureProvider(RetrieverJob job)
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion

        #region Protected
        protected virtual string GetDataPathRegexPattern(RetrieverJob job)
        {
            if (string.IsNullOrEmpty(job.FileSchema.SchemaRootPath))
            {
                return "";
            }
            else
            {
                string regex = job.FileSchema.SchemaRootPath.Replace(",", @"\[\d+\]\.");
                return $"^{regex}";
            }
        }

        protected virtual async Task WriteToFileAsync(Stream contentStream, Stream fileStream, PagingHttpsConfiguration config)
        {
            //move back to beginning of content
            contentStream.Seek(0, SeekOrigin.Begin);
            await contentStream.CopyToAsync(fileStream);

            //Add new line
            using (MemoryStream newLineStream = new MemoryStream(Encoding.UTF8.GetBytes("\r\n")))
            {
                await newLineStream.CopyToAsync(fileStream);
            }
        }

        protected virtual void EndFile(Stream fileStream)
        {
            //do nothing
        }

        protected StreamReader GetStreamReader(Stream contentStream)
        {
            return new StreamReader(contentStream, Encoding.UTF8, true, 1024, true);
        }
        #endregion

        #region Private
        private async Task RetrieveDataAsync(PagingHttpsConfiguration config, HttpClient httpClient)
        {
            string tempDirectory = GetTempDirectory(config.Job.Id);
            string tempFile = $@"{tempDirectory}\{config.Filename}.json";

            try
            {
                using (Stream fileStream = _fileProvider.GetFileStream(tempFile, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    Logger.Info($"Paging Https Retriever Job {tempFile} created - Job: {config.Job.Id}");
                    //loop until no more to retrieve
                    while (!string.IsNullOrEmpty(config.RequestUri))
                    {
                        //set header each time in case token expires
                        SetAuthorizationHeader(config, httpClient);

                        //copy response to file
                        int resultCount = await CopyResponseToFileAsync(httpClient, fileStream, config);

                        //get next request to make
                        SetNextRequest(config, resultCount);

                        //upload file if over 2GB
                        FlushAccumulatedProgress(fileStream, config);
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
                Logger.Error($"Paging Https Retriever Job failed retrieving data from {RequestLog(config)} - Job: {config.Job.Id}", ex);
                throw;
            }
            finally
            {
                //always clean up temp space because we only save progress after file has been uploaded to S3
                _fileProvider.DeleteDirectory(tempDirectory);
                Logger.Info($"Paging Https Retriever Job {tempDirectory} deleted - Job: {config.Job.Id}");
            }
        }

        private async Task<Stream> GetResponseStreamAsync(PagingHttpsConfiguration config, HttpClient httpClient)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(config.Source.BaseUri, config.RequestUri)
            };

            if (config.Options.RequestMethod == HttpMethods.post)
            {
                request.Method = HttpMethod.Post;
                request.Content = new StringContent(config.RequestBody.ToString(), Encoding.UTF8, "application/json");
            }
            else
            {
                request.Method = HttpMethod.Get;
            }

            //make request
            Logger.Info($"Paging Https Retriever Job making request to {RequestLog(config)} - Job: {config.Job.Id}");

            using (HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.IsSuccessStatusCode)
                {
                    MemoryStream memoryStream = new MemoryStream();

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        contentStream.CopyTo(memoryStream);
                    }

                    Logger.Info($"Paging Https Retriever Job successful response from {RequestLog(config)} - Job: {config.Job.Id}");

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
                else
                {
                    throw new HttpsJobProviderException($"HTTPS request to {RequestLog(config)} failed. {response.Content.ReadAsStringAsync().Result}");
                }
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
                DataPathRegex = new Regex(GetDataPathRegexPattern(job)),
                Filename = job.JobOptions.TargetFileName,
                Options = job.JobOptions.HttpOptions,
                InitialRequestVariables = job.RequestVariables
            };

            if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
            {
                //start from first data source token if using OAuth
                config.OrderedDataSourceTokens = config.Source.Tokens?.OrderBy(x => x.Id).ToList();
                config.CurrentDataSourceToken = config.OrderedDataSourceTokens.First();
            }

            ReplaceVariablePlaceholders(config);

            if (job.ExecutionParameters.Any()) //start from saved progress
            {
                if (config.Source.SourceAuthType.Is<OAuthAuthentication>() && job.ExecutionParameters.TryGetValue(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, out string value))
                {
                    int tokenId = int.Parse(value);
                    config.CurrentDataSourceToken = config.Source.Tokens.First(x => x.Id == tokenId);
                }

                CheckForPagingExecutionParameters(config);
            }       

            Logger.Info($"Paging Https Retriever Job starting from {RequestLog(config)} - Job: {job.Id}");

            return config;
        }

        private async Task<int> CopyResponseToFileAsync(HttpClient httpClient, Stream fileStream, PagingHttpsConfiguration config)
        {
            int resultCount = 0;

            using (Stream responseStream = await GetResponseStreamAsync(config, httpClient))
            {
                using (StreamReader streamReader = GetStreamReader(responseStream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject && config.DataPathRegex.IsMatch(jsonReader.Path))
                        {
                            resultCount++;
                            jsonReader.Skip();
                        }
                    }
                }

                if (resultCount > 0)
                {
                    await WriteToFileAsync(responseStream, fileStream, config);
                    Logger.Info($"Paging Https Retriever Job response content copied to temp file - Job: {config.Job.Id}");
                    config.MostRecentVariablesWithCollectedData = config.Job.RequestVariables;
                }
                else
                {
                    Logger.Info($"Paging Https Retriever Job no rows found - Job: {config.Job.Id}");
                }
            }

            return resultCount;
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

        private void ReplaceVariablePlaceholders(PagingHttpsConfiguration config)
        {
            config.RequestUri = config.Job.RelativeUri;
            string body = config.Options.Body;

            foreach (RequestVariable variable in config.Job.RequestVariables)
            {
                string placeholder = string.Format(Indicators.REQUESTVARIABLEINDICATOR, variable.VariableName);
                config.RequestUri = config.RequestUri.Replace(placeholder, variable.VariableValue);

                if (config.Options.RequestMethod == HttpMethods.post)
                {
                    body = body.Replace(placeholder, variable.VariableValue);
                }
            }

            if (config.Options.RequestMethod == HttpMethods.post)
            {
                config.RequestBody = JObject.Parse(body);
            }
        }

        private string GetTempDirectory(int jobId)
        {
            string tempDirectory = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", jobId.ToString());
            _fileProvider.CreateDirectory(tempDirectory);

            return tempDirectory;
        }

        private void FlushAccumulatedProgress(Stream fileStream, PagingHttpsConfiguration config)
        {
            //combined size of file and response will be over 2GB
            if (fileStream.Length >= Math.Pow(1024, 3) * 2)
            {
                Logger.Info($"Paging Https Retriever Job hit 2GB size threshold - Job: {config.Job.Id}");
                //upload to S3 and save progress
                UploadStreamToS3(fileStream, config);

                AddUpdatePagingExecutionParameter(config);

                if (config.Source.SourceAuthType.Is<OAuthAuthentication>())
                {
                    config.Job.AddOrUpdateExecutionParameter(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, config.CurrentDataSourceToken.Id.ToString());
                    Logger.Info($"Paging Https Retriever Job added execution parameter {ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID}: {config.CurrentDataSourceToken.Id} - Job: {config.Job.Id}");
                }

                _datasetContext.SaveChanges();
                Logger.Info($"Paging Https Retriever Job progress saved - Job: {config.Job.Id}");

                //clear contents of filestream for continued use
                fileStream.SetLength(0);
                Logger.Info($"Paging Https Retriever Job temp file reset - Job: {config.Job.Id}");
            }
        }

        private void UploadStreamToS3(Stream fileStream, PagingHttpsConfiguration config)
        {
            EndFile(fileStream);

            string targetKey = $"{config.S3DropStep.TriggerKey}{config.Filename}_{DateTime.Now:yyyyMMddHHmmssfff}.json";
            Logger.Info($"Paging Https Retriever Job start S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
            _s3ServiceProvider.UploadDataFile(fileStream, config.S3DropStep.TriggerBucket, targetKey);
            Logger.Info($"Paging Https Retriever Job complete S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
        }

        private void SetNextRequest(PagingHttpsConfiguration config, int resultCount)    
        {            
            if (resultCount > 0 && config.Options.PagingType != PagingType.None)
            {
                //get next page if still getting results
                AddUpdatePagingQueryParameter(config, resultCount);
            }
            else
            {
                //reset page parameters for new token or new variable values
                config.PageNumber = 1;
                config.Index = 0;

                if (config.Source.SourceAuthType.Is<OAuthAuthentication>() && config.CurrentDataSourceToken != config.OrderedDataSourceTokens.Last())
                {
                    //using OAuth, move to the next token if there are more
                    int nextIndex = config.OrderedDataSourceTokens.IndexOf(config.CurrentDataSourceToken) + 1;
                    config.CurrentDataSourceToken = config.OrderedDataSourceTokens[nextIndex];
                    RemovePageParameter(config);

                    Logger.Info($"Paging Https Retriever Job using data source token {nextIndex + 1} of {config.OrderedDataSourceTokens.Count} - Job: {config.Job.Id}");
                }
                else
                {
                    bool dataRetrievedForCurrentVariables = IsDataRetrievedForCurrentVariables(config);
                    SetNextRequestVariables(config, dataRetrievedForCurrentVariables);
                }
            }
        }

        private bool IsDataRetrievedForCurrentVariables(PagingHttpsConfiguration config)
        {
            //check if data was found for the current request variables before incrementing
            bool dataFoundForCurrentVariables = config.MostRecentVariablesWithCollectedData != null &&
                config.MostRecentVariablesWithCollectedData.Count == config.Job.RequestVariables.Count && 
                config.MostRecentVariablesWithCollectedData.All(x => config.Job.RequestVariables.Any(a => x.EqualTo(a)));
            
            if (!dataFoundForCurrentVariables)
            {
                Logger.Warn($"Paging Https Retriever Job no data retrieved from {RequestLog(config)} - Job: {config.Job.Id}");
            }

            return dataFoundForCurrentVariables;
        }

        private void SetNextRequestVariables(PagingHttpsConfiguration config, bool dataRetrievedForCurrentVariables)
        {
            config.Job.IncrementRequestVariables();

            if (config.Job.RequestVariables.Any() && config.Job.HasValidRequestVariables())
            {
                //variables able to be incremented, update request uri with incremented values
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

                if (!dataRetrievedForCurrentVariables)
                {
                    //is null when nothing was collected 
                    if (config.MostRecentVariablesWithCollectedData == null)
                    {
                        //set back to starting variable values since nothing was collected
                        Logger.Info($"Paging Https Retriever Job variables set back to initial variables - Job: {config.Job.Id}");
                        config.Job.RequestVariables = config.InitialRequestVariables;
                    }
                    else
                    {
                        //keep track of what point data was last retrieved for
                        Logger.Info($"Paging Https Retriever Job variables set after most recently collected variables - Job: {config.Job.Id}");
                        config.Job.RequestVariables = config.MostRecentVariablesWithCollectedData;
                        config.Job.IncrementRequestVariables();
                    }
                }

                config.RequestUri = "";
            }
        }

        private void AddUpdatePageParameter(PagingHttpsConfiguration config, int parameterValue)
        {
            if (config.Options.RequestMethod == HttpMethods.post)
            {
                config.RequestBody[config.Options.PageParameterName] = parameterValue;
            }
            else
            {
                NameValueCollection parameters;
                List<string> uriParts = config.RequestUri.Split('?').ToList();

                if (uriParts.Count > 1)
                {
                    parameters = HttpUtility.ParseQueryString(uriParts.Last());
                }
                else
                {
                    parameters = HttpUtility.ParseQueryString("");
                }

                parameters.Set(config.Options.PageParameterName, parameterValue.ToString());

                config.RequestUri = $"{uriParts.First()}?{parameters}";
            }
        }

        private void RemovePageParameter(PagingHttpsConfiguration config)
        {
            if (config.Options.RequestMethod == HttpMethods.post)
            {
                config.RequestBody.Remove(config.Options.PageParameterName);
            }
            else
            {
                List<string> uriParts = config.RequestUri.Split('?').ToList();

                if (uriParts.Count > 1)
                {
                    NameValueCollection parameters = HttpUtility.ParseQueryString(uriParts.Last());
                    parameters.Remove(config.Options.PageParameterName);

                    config.RequestUri = uriParts.First();
                    if (parameters.HasKeys())
                    {
                        config.RequestUri += $"?{parameters}";
                    }
                }
            }
        }

        private string RequestLog(PagingHttpsConfiguration config)
        {
            string log = config.Source.BaseUri + config.RequestUri;

            if (config.Options.RequestMethod == HttpMethods.post)
            {
                log += " " + config.RequestBody.ToString(Formatting.None);
            }

            return log;
        }

        #region PageType Methods
        private void AddUpdatePagingQueryParameter(PagingHttpsConfiguration config, int resultCount)
        {
            switch (config.Options.PagingType)
            {
                case PagingType.PageNumber:
                    //add the next page number to uri
                    config.PageNumber++;
                    AddUpdatePageParameter(config, config.PageNumber);
                    break;
                case PagingType.Index:
                    //add on data retrieved so far
                    config.Index += resultCount;
                    AddUpdatePageParameter(config, config.Index);
                    break;
            }
        }

        private void AddUpdatePagingExecutionParameter(PagingHttpsConfiguration config)
        {
            string parameterValue = null;

            switch (config.Options.PagingType)
            {
                case PagingType.PageNumber:
                    parameterValue = config.PageNumber.ToString();
                    break;
                case PagingType.Index:
                    parameterValue = config.Index.ToString();
                    break;
            }

            if (!string.IsNullOrEmpty(parameterValue))
            {
                config.Job.AddOrUpdateExecutionParameter(config.Options.PageParameterName, parameterValue);
                Logger.Info($"Paging Https Retriever Job added execution parameter {config.Options.PageParameterName}: {parameterValue} - Job: {config.Job.Id}");
            }
        }

        private void CheckForPagingExecutionParameters(PagingHttpsConfiguration config)
        {
            if (config.Job.ExecutionParameters.TryGetValue(config.Options.PageParameterName, out string parameterValue))
            {
                int intValue = int.Parse(parameterValue);
                switch (config.Options.PagingType)
                {
                    case PagingType.PageNumber:
                        config.PageNumber = intValue;
                        break;
                    case PagingType.Index:
                        config.Index = intValue;
                        break;
                }

                AddUpdatePageParameter(config, intValue);
            }
        }
        #endregion
        #endregion
    }
}
