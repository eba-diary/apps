using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Sentry.Common.Logging;

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
                List<string> uriParts = job.RelativeUri.Split('/').ToList();
                string projectId = uriParts[1];
                string datasetId = uriParts[3];
                string tableId = uriParts[5];

                //handle everything with the parameters in code here first then determine if a shared or usable version is possible
                //The first value desired will either get entered through UI or get put in the brackets with the variable name
                //if parameters are null, use that value (will only need to check for null if don't add an "Add Parameter" to UI
                if (job.ExecutionParameters == null)
                {

                }

                //else increment the parameter value needed accordingly

                //update DSC schema
                JArray bigQueryFields = GetBigQueryFields(httpClient, job);
                _googleBigQueryService.UpdateSchemaFields(job.DataFlow.SchemaId, bigQueryFields);

                //build the request
                //use execution parameters
                //parameters will have record count and last consumed date
                //if parameters are null, get 
                //loop over each day since last successful execution

                //make request

                //track last record in response (in case of failure to start from where left off)

                //upload to S3 (async? would need to handle if fails upload)

                //loop until no more pages

                //update execution parameters
            }
        }

        #region Private
        private JArray GetBigQueryFields(HttpClient httpClient, RetrieverJob job)
        {
            List<string> uriParts = job.RelativeUri.Split('/').ToList();
            string projectId = uriParts[1];
            string datasetId = uriParts[3];
            string tableId = uriParts[5];

            string tableMetadataUri = $"{job.DataSource.BaseUri}{projectId}/datasets/{datasetId}/tables/{tableId}";

            HttpResponseMessage tableMetadataResponse = httpClient.GetAsync(tableMetadataUri).Result;
            string tableMetadataResponseString = tableMetadataResponse.Content.ReadAsStringAsync().Result;
            if (!tableMetadataResponse.IsSuccessStatusCode)
            {
                Logger.Error($"Google Big Query Api Provider failed to retrieve table metadata. {tableMetadataResponseString}");
                throw new GoogleBigQueryJobProviderException(tableMetadataResponseString);
            }

            JObject tableMetadata = JObject.Parse(tableMetadataResponseString);
            return (JArray)tableMetadata.SelectToken("schema.fields");
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
