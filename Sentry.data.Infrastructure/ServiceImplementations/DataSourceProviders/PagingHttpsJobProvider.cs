using Amazon.Auth.AccessControlPolicy;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Pkcs;
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
        private readonly Dictionary<DataFlowPreProcessingTypes, Func<Stream, Stream, JToken, PagingHttpsConfiguration, Task<JToken>>> _readResponseRegistry;
        #endregion

        #region Constructor
        public PagingHttpsJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator, IFileProvider fileProvider) 
        {
            _datasetContext = datasetContext;
            _s3ServiceProvider = s3ServiceProvider;
            _authorizationProvider = authorizationProvider;
            _httpClientGenerator = httpClientGenerator;
            _fileProvider = fileProvider;

            _readResponseRegistry = new Dictionary<DataFlowPreProcessingTypes, Func<Stream, Stream, JToken, PagingHttpsConfiguration, Task<JToken>>>
            {
                { DataFlowPreProcessingTypes.googlesearchconsoleapi, (content, file, resp, config) => GoogleSearchConsoleReadResponseAsync(content, file, resp, config) }
            };
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
                        httpClient.BaseAddress = config.Source.BaseUri;
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

                        //make request
                        Logger.Info($"Paging Https Retriever Job making request to {config.RequestUri} - Job: {config.Job.Id}");
                        using (HttpResponseMessage response = await httpClient.GetAsync(config.RequestUri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                Logger.Info($"Paging Https Retriever Job successful response from {config.RequestUri} - Job: {config.Job.Id}");
                                //copy response to file
                                JToken responseData = await ReadResponseAsync(response, fileStream, config);

                                //get next request to make
                                SetNextRequest(config, responseData);

                                //upload file if over 2GB
                                FlushAccumulatedProgress(fileStream, config);
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
                //always clean up temp space because we only save progress after file has been uploaded to S3
                _fileProvider.DeleteDirectory(tempDirectory);
                Logger.Info($"Paging Https Retriever Job {tempDirectory} deleted - Job: {config.Job.Id}");
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

            Logger.Info($"Paging Https Retriever Job starting from {config.RequestUri} - Job: {job.Id}");

            return config;
        }

        private async Task<JToken> ReadResponseAsync(HttpResponseMessage response, Stream fileStream, PagingHttpsConfiguration config)
        {
            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            using (StreamReader streamReader = new StreamReader(contentStream))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                JToken responseToken = JToken.Load(jsonReader);

                if (config.Job.DataFlow.IsPreProcessingRequired && _readResponseRegistry.TryGetValue((DataFlowPreProcessingTypes)config.Job.DataFlow.PreProcessingOption, out var readResponse))
                {
                    return await readResponse(contentStream, fileStream, responseToken, config);
                }

                return await GenericReadResponseAsync(contentStream, fileStream, responseToken, config);
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

        private void ReplaceVariablePlaceholders(PagingHttpsConfiguration config)
        {
            config.RequestUri = config.Job.RelativeUri;

            foreach (RequestVariable variable in config.Job.RequestVariables)
            {
                string placeholder = string.Format(Indicators.REQUESTVARIABLEINDICATOR, variable.VariableName);
                config.RequestUri = config.RequestUri.Replace(placeholder, variable.VariableValue);
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
            string targetKey = $"{config.S3DropStep.TriggerKey}{config.Filename}_{DateTime.Now:yyyyMMddHHmmssfff}.json";
            _s3ServiceProvider.UploadDataFile(fileStream, config.S3DropStep.TriggerBucket, targetKey);
            Logger.Info($"Paging Https Retriever Job complete S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
        }

        private void SetNextRequest(PagingHttpsConfiguration config, JToken response)    
        {
            if (response != null && config.Options.PagingType != PagingType.None)
            {
                //get next page if still getting results
                AddUpdatePagingQueryParameter(config, response);
            }
            else
            {
                //reset page parameters for new token or variable values
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
                else if(config.Job.TryIncrementRequestVariables())
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
                    config.RequestUri = "";
                }
            }

            Logger.Info($"Paging Https Retriever Job next request {config.RequestUri} - Job: {config.Job.Id}");
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
                parameters = HttpUtility.ParseQueryString("");
            }

            parameters.Set(config.Options.PageParameterName, parameterValue);

            config.RequestUri = $"{uriParts.First()}?{parameters}";
        }

        private void RemovePageParameter(PagingHttpsConfiguration config)
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

        #region PageType Methods
        private void AddUpdatePagingQueryParameter(PagingHttpsConfiguration config, JToken response)
        {
            switch (config.Options.PagingType)
            {
                case PagingType.PageNumber:
                    //add the next page number to uri
                    config.PageNumber++;
                    AddUpdatePageParameter(config, config.PageNumber.ToString());
                    break;
                case PagingType.Index:
                    //add on data retrieved so far
                    config.Index += response.Count();
                    AddUpdatePageParameter(config, config.Index.ToString());
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
                switch (config.Options.PagingType)
                {
                    case PagingType.PageNumber:
                        config.PageNumber = int.Parse(parameterValue);
                        break;
                    case PagingType.Index:
                        config.Index = int.Parse(parameterValue);
                        break;
                }

                AddUpdatePageParameter(config, parameterValue);
            }
        }
        #endregion

        #region ReadResponse Methods
        private async Task<JToken> GenericReadResponseAsync(Stream contentStream, Stream fileStream, JToken response, PagingHttpsConfiguration config)
        {
            //verify there is data from response to write
            JToken data = response.SelectToken(config.DataPath);
            if (data?.Any() == true)
            {
                //move back to beginning of content
                contentStream.Position = 0;
                await contentStream.CopyToAsync(fileStream);

                //Add new line
                MemoryStream newLineStream = new MemoryStream(Encoding.UTF8.GetBytes("\r\n"));
                await newLineStream.CopyToAsync(fileStream);

                Logger.Info($"Paging Https Retriever Job response content copied to temp file - Job: {config.Job.Id}");
                return data;
            }

            Logger.Info($"Paging Https Retriever Job no data found at {config.DataPath} - Job: {config.Job.Id}");
            return null;
        }

        private async Task<JToken> GoogleSearchConsoleReadResponseAsync(Stream contentStream, Stream fileStream, JToken response, PagingHttpsConfiguration config)
        {
            return null;
        }
        #endregion
        #endregion
    }
}
