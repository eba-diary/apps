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
        private IHttpClientGenerator _httpClientGenerator;
        private IS3ServiceProvider _s3ServiceProvider;
        private IDatasetContext _datasetContext;
        private IDataFlowService _dataFlowService;
        private IEncryptionService _encryptionService;
        private IAuthorizationProvider _authorizationProvider;


        public MotiveProvider(IHttpClientGenerator httpClientGenerator, IS3ServiceProvider s3ServiceProvider, IDatasetContext datasetContext, IDataFlowService dataFlowService, IEncryptionService encryptionService, IAuthorizationProvider authorizationProvider)
        {
            _httpClientGenerator = httpClientGenerator;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetContext = datasetContext;
            _dataFlowService = dataFlowService;
            _encryptionService = encryptionService;
            _authorizationProvider = authorizationProvider;
        }

        public async Task MotiveOnboardingAsync(DataSource motiveSource, DataSourceToken token, int companiesDataflowId)
        {
            var motiveCompaniesUrl = Config.GetHostSetting("MotiveCompaniesUrl");
            HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(motiveCompaniesUrl);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authorizationProvider.GetOAuthAccessToken((HTTPSSource)motiveSource, token)}");

            using (HttpResponseMessage response = await httpClient.GetAsync(motiveCompaniesUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (StreamReader streamReader = new StreamReader(contentStream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JObject responseObject = JObject.Load(jsonReader);
                    token.TokenName = responseObject.Value<string>("companies[0].company");
                    var s3Drop = _dataFlowService.GetDataFlowStepForDataFlowByActionType(companiesDataflowId, DataActionType.S3Drop);
                    _s3ServiceProvider.UploadDataFile(contentStream, s3Drop.TriggerBucket, s3Drop.TriggerKey);
                }
                _datasetContext.SaveChanges();
            }
        }
    }
}
