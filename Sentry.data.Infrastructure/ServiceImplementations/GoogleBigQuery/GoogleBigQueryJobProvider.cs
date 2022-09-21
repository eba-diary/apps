using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Sentry.Common.Logging;
using System.Globalization;
using System.IO;
using static Sentry.data.Core.GlobalConstants;
using System.Text;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure
{
    public class GoogleBigQueryJobProvider : IBaseJobProvider
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IGoogleBigQueryService _googleBigQueryService;
        private readonly IHttpClientGenerator _httpClientGenerator;

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
            //get Google token
            string accessToken = _authorizationProvider.GetOAuthAccessToken((HTTPSSource)job.DataSource);

            //get Google schema
            HttpClient httpClient = _httpClientGenerator.GenerateHttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            using (httpClient)
            {
                //need to calculate the tableId first
                GoogleBigQueryConfiguration config = GetConfig(job);

                try
                {
                    //update DSC schema
                    JArray bigQueryFields = GetBigQueryFields(httpClient, config);
                    _googleBigQueryService.UpdateSchemaFields(job.DataFlow.SchemaId, bigQueryFields);

                    //get data flow step for drop location info
                    DataFlowStep step = _datasetContext.DataFlowStep.Where(w => w.DataFlow.Id == job.DataFlow.Id && w.DataAction_Type_Id == DataActionType.ProducerS3Drop).FirstOrDefault();

                    do
                    {
                        string requestKey = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                        //make request
                        MemoryStream stream = GetBigQueryDataStream(httpClient, config);

                        //if rows to upload
                        if (stream != null)
                        {
                            using (stream)
                            {
                                //manufacture target key
                                string targetKey = $"{step.TriggerKey}{job.JobOptions.TargetFileName}_{config.TableId}_{requestKey}.json";

                                //upload to S3
                                _s3ServiceProvider.UploadDataFile(stream, step.TriggerBucket, targetKey);

                                //update execution parameters
                                SaveProgress(config, job);
                            }
                        }

                        if (string.IsNullOrEmpty(config.PageToken))
                        {
                            IncrementTableId(config);
                        }
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
                        Logger.Error($"Google Big Query Job Provider stopped before reaching the expected date partition ({expectedDate:yyyyMMdd}). Project: {config.ProjectId}, Dataset: {config.DatasetId}, Table: {config.TableId}");
                    }
                }
            }
        }

        #region Private
        private GoogleBigQueryConfiguration GetConfig(RetrieverJob job)
        {
            List<string> uriParts = job.RelativeUri.Split('/').ToList();

            GoogleBigQueryConfiguration config = new GoogleBigQueryConfiguration()
            {
                BaseUri = job.DataSource.BaseUri.ToString(),
                ProjectId = uriParts[1],
                DatasetId = uriParts[3],
                TableId = uriParts[5],
                RelativeUri = job.RelativeUri
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
            string removeSuffix = "/data";
            string uri = config.BaseUri + config.RelativeUri.Remove(config.RelativeUri.Length - removeSuffix.Length);
            JObject response = GetBigQueryResponse(httpClient, uri);
            return (JArray)response.SelectToken("schema.fields");
        }

        private MemoryStream GetBigQueryDataStream(HttpClient httpClient, GoogleBigQueryConfiguration config)
        {
            string uri = config.BaseUri + config.RelativeUri;

            if (!string.IsNullOrEmpty(config.PageToken)) //if have a page token use it
            {
                AddUriParameter(ref uri, "pageToken", config.PageToken);
            }
            else if (config.LastIndex > 0) //only use start index if failed in the middle of collecting and no longer have page token
            {
                AddUriParameter(ref uri, "startIndex", config.LastIndex.ToString());
            }

            JObject response = GetBigQueryResponse(httpClient, uri);

            //when at the end, there is no rows or pageToken field
            config.TotalRows = response.Value<int>("totalRows");
            config.PageToken = response.Value<string>("pageToken");

            if (response.ContainsKey("rows"))
            {
                config.LastIndex += ((JArray)response.SelectToken("rows")).Count();
                return new MemoryStream(Encoding.UTF8.GetBytes(response.ToString()));
            }

            return null;
        }

        private void AddUriParameter(ref string uri, string parameterKey, string parameterValue)
        {
            uri += uri.Contains("?") ? "&" : "?" + $"{parameterKey}={Uri.EscapeDataString(parameterValue)}";
        }

        private JObject GetBigQueryResponse(HttpClient httpClient, string uri)
        {
            HttpResponseMessage response = httpClient.GetAsync(uri).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.Info($"Google Big Query request to {uri} returned NotFound. {responseString}");
                    throw new GoogleBigQueryNotFoundException();
                }

                Logger.Error($"Google Big Query request to {uri} failed. {responseString}");
                throw new GoogleBigQueryJobProviderException(responseString);
            }

            return JObject.Parse(responseString);
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
