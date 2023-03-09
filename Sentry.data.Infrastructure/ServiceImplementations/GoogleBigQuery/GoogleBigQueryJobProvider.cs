using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class GoogleBigQueryJobProvider : IBaseJobProvider
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientGenerator _httpClientGenerator;
        private readonly IGoogleBigQueryService _googleBigQueryService;

        public GoogleBigQueryJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator, IGoogleBigQueryService googleBigQueryService)
        {
            _datasetContext = datasetContext;
            _s3ServiceProvider = s3ServiceProvider;
            _httpClientGenerator = httpClientGenerator;
            _authorizationProvider = authorizationProvider;
            _googleBigQueryService = googleBigQueryService;
        }

        public void Execute(RetrieverJob job)
        {
            //calculate the tableId first
            GoogleBigQueryConfiguration config = GetConfig(job);

            Logger.Info($"Google BigQuery Retriever Job start - Job: {job.Id}");
            using (_authorizationProvider)
            {
                //get Google token
                HTTPSSource source = (HTTPSSource)job.DataSource;
                string accessToken = _authorizationProvider.GetOAuthAccessToken(source, source.GetActiveTokens().FirstOrDefault());
                Logger.Info($"Google BigQuery Retriever Job access token retrieved - Job: {job.Id}");

                using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(job.DataSource.BaseUri.ToString()))
                {
                    httpClient.BaseAddress = job.DataSource.BaseUri;
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                    httpClient.Timeout = new TimeSpan(0, 10, 0);

                    try
                    {
                        //update DSC schema
                        JArray bigQueryFields = GetBigQueryFieldsAsync(httpClient, config).Result;
                        Logger.Info($"Google BigQuery Retriever Job fields retrieved - Job: {job.Id}");
                        _googleBigQueryService.UpdateSchemaFields(job.DataFlow.SchemaId, bigQueryFields);
                        Logger.Info($"Google BigQuery Retriever Job fields saved - Job: {job.Id}");

                        do
                        {
                            //make request
                            GetBigQueryDataAsync(httpClient, config).Wait();

                            if (string.IsNullOrEmpty(config.PageToken))
                            {
                                IncrementTableId(config);
                            }

                            //refresh token if expired
                            accessToken = _authorizationProvider.GetOAuthAccessToken(source, source.GetActiveTokens().FirstOrDefault());
                            httpClient.DefaultRequestHeaders.Clear();
                            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        }
                        while (true); //only break loop on exception
                    }
                    catch (AggregateException ae)
                    {                        
                        ae.Handle(e =>
                        {
                            if (e is GoogleBigQueryNotFoundException)
                            {
                                //no more tables to collect from
                                DateTime expectedDate = DateTime.Today.AddDays(-1);
                                DateTime stopDate = DateTime.ParseExact(config.TableId.Split('_').Last(), "yyyyMMdd", CultureInfo.InvariantCulture);
                                if (stopDate < expectedDate)
                                {
                                    //means there may be an unexpected gap
                                    Logger.Error($"Google BigQuery Job Provider stopped before reaching the expected date partition ({expectedDate:yyyyMMdd}). Project: {config.ProjectId}, Dataset: {config.DatasetId}, Table: {config.TableId}");
                                }

                                return true;
                            }

                            return false;
                        });      
                    }
                }
            }

            Logger.Info($"Google BigQuery Retriever Job end - Job: {job.Id}");
        }

        #region Private
        private GoogleBigQueryConfiguration GetConfig(RetrieverJob job)
        {
            List<string> uriParts = job.RelativeUri.Split('/').ToList();

            GoogleBigQueryConfiguration config = new GoogleBigQueryConfiguration()
            {
                ProjectId = uriParts[1],
                DatasetId = uriParts[3],
                TableId = uriParts[5],
                RelativeUri = job.RelativeUri,
                Job = job,
                ExecutionKey = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                S3DropStep = _datasetContext.DataFlowStep.Where(w => w.DataFlow.Id == job.DataFlow.Id && w.DataAction_Type_Id == DataActionType.ProducerS3Drop).FirstOrDefault()
            };

            Dictionary<string, string> parameters = job.ExecutionParameters;

            if (parameters.ContainsKey(ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX) &&
                parameters.ContainsKey(ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS) &&
                parameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX] != parameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS])
            {
                //if have not completed retrieving all records for current table, continue from last index
                config.LastIndex = int.Parse(parameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
            }
            else
            {
                //else move to the next day for the table
                IncrementTableId(config);
            }

            return config;
        }

        private void IncrementTableId(GoogleBigQueryConfiguration config)
        {
            List<string> idParts = config.TableId.Split('_').ToList();
            DateTime lastDate = DateTime.ParseExact(idParts.Last(), "yyyyMMdd", CultureInfo.InvariantCulture);

            //reset config for new table
            config.TableId = idParts.First() + "_" + lastDate.AddDays(1).ToString("yyyyMMdd");
            config.PageToken = "";
            config.LastIndex = 0;
            config.TotalRows = 0;

            List<string> uriParts = config.RelativeUri.Split('/').ToList();
            uriParts[5] = config.TableId;
            config.RelativeUri = string.Join("/", uriParts);
        }

        private async Task<JArray> GetBigQueryFieldsAsync(HttpClient httpClient, GoogleBigQueryConfiguration config)
        {
            List<string> uriParts = config.RelativeUri.Split('/').ToList();
            string uri = string.Join("/", uriParts.GetRange(0, 6));

            using (HttpResponseMessage response = await GetBigQueryResponseAsync(httpClient, uri))
            using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                JObject responseObject = JObject.Load(jsonReader);
                return (JArray)responseObject.SelectToken("schema.fields");
            }
        }

        private async Task GetBigQueryDataAsync(HttpClient httpClient, GoogleBigQueryConfiguration config)
        {
            string uri = config.RelativeUri;

            if (!string.IsNullOrEmpty(config.PageToken)) //if have a page token use it
            {
                AddUriParameter(ref uri, "pageToken", config.PageToken);
            }
            else if (config.LastIndex > 0) //only use start index if failed in the middle of collecting and no longer have page token
            {
                AddUriParameter(ref uri, "startIndex", config.LastIndex.ToString());
            }

            Logger.Info($"Google BigQuery Retriever Job start data retrieval - Job: {config.Job.Id}, Uri: {config.RelativeUri}, Index:{config.LastIndex}, Total:{config.TotalRows}");

            using (Stream memoryStream = new MemoryStream())
            {
                using (HttpResponseMessage response = await GetBigQueryResponseAsync(httpClient, uri))
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                {
                    contentStream.CopyTo(memoryStream);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                if (ResponseHasRows(memoryStream, config))
                {
                    //manufacture target key
                    string targetKey = $"{config.S3DropStep.TriggerKey}{config.Job.JobOptions.TargetFileName}_{config.TableId}_{config.LastIndex}_{config.ExecutionKey}.json";

                    //upload to S3
                    Logger.Info($"Google BigQuery Retriever Job start S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
                    _s3ServiceProvider.UploadDataFile(memoryStream, config.S3DropStep.TriggerBucket, targetKey);
                    Logger.Info($"Google BigQuery Retriever Job end S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");

                    //update execution parameters
                    SaveProgress(config, config.Job);
                    Logger.Info($"Google BigQuery Retriever Job progress saved - Job: {config.Job.Id}, Uri: {config.RelativeUri}, Index:{config.LastIndex}, Total:{config.TotalRows}");
                }
            }
        }

        private bool ResponseHasRows(Stream contentStream, GoogleBigQueryConfiguration config)
        {
            int rowCount = 0;
            int totalRows = 0;
            string pageToken = null;

            using (StreamReader streamReader = new StreamReader(contentStream, Encoding.UTF8, true, 1024, true))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.String)
                    {
                        if (jsonReader.Path == "pageToken")
                        {
                            pageToken = jsonReader.Value.ToString();
                        }
                        else if (jsonReader.Path == "totalRows")
                        {
                            totalRows = int.Parse(jsonReader.Value.ToString());
                        }
                    }
                    else if (jsonReader.TokenType == JsonToken.StartObject && jsonReader.Path.StartsWith("rows"))
                    {
                        rowCount++;
                        jsonReader.Skip();
                    }
                }
            }

            config.TotalRows = totalRows;
            config.PageToken = pageToken;
            config.LastIndex += rowCount;

            return rowCount > 0;
        }

        private void AddUriParameter(ref string uri, string parameterKey, string parameterValue)
        {
            uri += (uri.Contains("?") ? "&" : "?") + $"{parameterKey}={Uri.EscapeDataString(parameterValue)}";
        }

        private async Task<HttpResponseMessage> GetBigQueryResponseAsync(HttpClient httpClient, string uri)
        {
            HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.Info($"Google BigQuery request to {uri} returned NotFound. {responseString}");
                    response.Dispose();
                    throw new GoogleBigQueryNotFoundException();
                }

                Logger.Error($"Google BigQuery request to {uri} failed. {responseString}");
                response.Dispose();
                throw new GoogleBigQueryJobProviderException(responseString);
            }
        }

        private void SaveProgress(GoogleBigQueryConfiguration config, RetrieverJob job)
        {
            job.RelativeUri = config.RelativeUri;
            job.AddOrUpdateExecutionParameter(ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX, config.LastIndex.ToString());
            job.AddOrUpdateExecutionParameter(ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS, config.TotalRows.ToString());
            
            _datasetContext.SaveChanges();
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
