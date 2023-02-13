using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class MotiveProvider : IMotiveProvider
    {
        private readonly HttpClient client;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDatasetContext _datasetContext;
        private readonly IDataFlowService _dataFlowService;
        private readonly IAuthorizationProvider _authorizationProvider;


        public MotiveProvider(HttpClient httpClient, IS3ServiceProvider s3ServiceProvider, IDatasetContext datasetContext, IDataFlowService dataFlowService, IAuthorizationProvider authorizationProvider)
        {
            client = httpClient;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetContext = datasetContext;
            _dataFlowService = dataFlowService;
            _authorizationProvider = authorizationProvider;
        }

        public async Task MotiveOnboardingAsync(DataSource motiveSource, DataSourceToken token, int companiesDataflowId)
        {
            var motiveCompaniesUrl = Config.GetHostSetting("MotiveCompaniesUrl");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authorizationProvider.GetOAuthAccessToken((HTTPSSource)motiveSource, token)}");

            using (HttpResponseMessage response = await client.GetAsync(motiveCompaniesUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (StreamReader streamReader = new StreamReader(contentStream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JObject responseObject = JObject.Load(jsonReader);
                    if (string.IsNullOrEmpty(responseObject.Value<string>("error")))
                    {
                        JArray companies = (JArray)responseObject["companies"];
                        JObject firstCompany = (JObject)companies[0];
                        token.TokenName = firstCompany.GetValue("company").Value<string>("name");

                        var s3Drop = _dataFlowService.GetDataFlowStepForDataFlowByActionType(companiesDataflowId, DataActionType.S3Drop);
                        _s3ServiceProvider.UploadDataFile(contentStream, s3Drop.TriggerBucket, s3Drop.TriggerKey);
                    }
                }
                _datasetContext.SaveChanges();
            }
        }
    }
}
