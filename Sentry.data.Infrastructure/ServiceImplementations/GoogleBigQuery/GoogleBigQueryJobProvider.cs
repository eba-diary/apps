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
                string accessToken = _authorizationProvider.GetOAuthAccessToken(source, source.Tokens.FirstOrDefault());
                Logger.Info($"Google BigQuery Retriever Job access token retrieved - Job: {job.Id}");

                using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(job.DataSource.BaseUri.ToString()))
                {
                    httpClient.BaseAddress = job.DataSource.BaseUri;
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                    httpClient.Timeout = new TimeSpan(0, 10, 0);

                    try
                    {
                        //update DSC schema
                        JArray bigQueryFields = GetBigQueryFields(httpClient, config);
                        Logger.Info($"Google BigQuery Retriever Job fields retrieved - Job: {job.Id}");
                        _googleBigQueryService.UpdateSchemaFields(job.DataFlow.SchemaId, bigQueryFields);
                        Logger.Info($"Google BigQuery Retriever Job fields saved - Job: {job.Id}");

                        do
                        {
                            //make request
                            GetBigQueryData(httpClient, config);

                            if (string.IsNullOrEmpty(config.PageToken))
                            {
                                IncrementTableId(config);
                            }

                            //refresh token if expired
                            accessToken = _authorizationProvider.GetOAuthAccessToken(source, source.Tokens.FirstOrDefault());
                            httpClient.DefaultRequestHeaders.Clear();
                            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        }
                        while (true); //only break loop on exception
                    }
                    catch (GoogleBigQueryNotFoundException)
                    {
                        //no more tables to collect from
                        DateTime expectedDate = DateTime.Today.AddDays(-1);
                        DateTime stopDate = DateTime.ParseExact(config.TableId.Split('_').Last(), "yyyyMMdd", CultureInfo.InvariantCulture);
                        if (stopDate < expectedDate)
                        {
                            //means there may be an unexpected gap
                            Logger.Error($"Google BigQuery Job Provider stopped before reaching the expected date partition ({expectedDate:yyyyMMdd}). Project: {config.ProjectId}, Dataset: {config.DatasetId}, Table: {config.TableId}");
                        }
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

        private JArray GetBigQueryFields(HttpClient httpClient, GoogleBigQueryConfiguration config)
        {
            List<string> uriParts = config.RelativeUri.Split('/').ToList();
            string uri = string.Join("/", uriParts.GetRange(0, 6));

            using (HttpResponseMessage response = GetBigQueryResponse(httpClient, uri))
            using (StreamReader streamReader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                JObject responseObject = JObject.Load(jsonReader);
                return (JArray)responseObject.SelectToken("schema.fields");
            }
        }

        private void GetBigQueryData(HttpClient httpClient, GoogleBigQueryConfiguration config)
        {
            string requestKey = DateTime.Now.ToString("yyyyMMddHHmmssfff");
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

            using (HttpResponseMessage response = GetBigQueryResponse(httpClient, uri))
            using (StreamReader streamReader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                if (ResponseHasRows(jsonReader, config))
                {
                    //manufacture target key
                    string targetKey = $"{config.S3DropStep.TriggerKey}{config.Job.JobOptions.TargetFileName}_{config.TableId}_{config.LastIndex}_{requestKey}.json";

                    //upload to S3
                    Logger.Info($"Google BigQuery Retriever Job start S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");
                    _s3ServiceProvider.UploadDataFile(streamReader.BaseStream, config.S3DropStep.TriggerBucket, targetKey);
                    Logger.Info($"Google BigQuery Retriever Job end S3 upload - Job: {config.Job.Id}, Bucket: {config.S3DropStep.TriggerBucket}, Key: {targetKey}");

                    //update execution parameters
                    SaveProgress(config, config.Job);
                    Logger.Info($"Google BigQuery Retriever Job progress saved - Job: {config.Job.Id}, Uri: {config.RelativeUri}, Index:{config.LastIndex}, Total:{config.TotalRows}");
                }
            }
        }

        private bool ResponseHasRows(JsonReader jsonReader, GoogleBigQueryConfiguration config)
        {
            JObject responseObject = JObject.Load(jsonReader);
            Logger.Info($"Google BigQuery Retriever Job end data retrieval - Job: {config.Job.Id}, Uri: {config.RelativeUri}, Index:{config.LastIndex}, Total:{config.TotalRows}");

            //when at the end, there is no rows or pageToken field
            config.TotalRows = responseObject.Value<int>("totalRows");
            config.PageToken = responseObject.Value<string>("pageToken");

            if (responseObject.ContainsKey("rows"))
            {
                config.LastIndex += ((JArray)responseObject.SelectToken("rows")).Count();
                return true;
            }

            return false;
        }

        private void AddUriParameter(ref string uri, string parameterKey, string parameterValue)
        {
            uri += (uri.Contains("?") ? "&" : "?") + $"{parameterKey}={Uri.EscapeDataString(parameterValue)}";
        }

        private HttpResponseMessage GetBigQueryResponse(HttpClient httpClient, string uri)
        {
            HttpResponseMessage response = httpClient.GetAsync(uri).Result;

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
