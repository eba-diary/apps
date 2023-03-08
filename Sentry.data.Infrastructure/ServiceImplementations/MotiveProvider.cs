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
        private readonly IDataFeatures _featureFlags;
        private readonly IEmailService _emailService;


        public MotiveProvider(HttpClient httpClient, IS3ServiceProvider s3ServiceProvider, IDatasetContext datasetContext, IDataFlowService dataFlowService, IAuthorizationProvider authorizationProvider, IDataFeatures featureFlags, IEmailService emailService)
        {
            client = httpClient;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetContext = datasetContext;
            _dataFlowService = dataFlowService;
            _authorizationProvider = authorizationProvider;
            _featureFlags = featureFlags;
            _emailService = emailService;
        }

        public async Task MotiveOnboardingAsync(DataSource motiveSource, DataSourceToken token, int companiesDataflowId)
        {
            var motiveCompaniesUrl = Config.GetHostSetting("MotiveCompaniesUrl");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authorizationProvider.GetOAuthAccessToken((HTTPSSource)motiveSource, token)}");
            Sentry.Common.Logging.Logger.Info($"Attempting to grab companies from {motiveCompaniesUrl}");

            using (HttpResponseMessage response = await client.GetAsync(motiveCompaniesUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (StreamReader streamReader = new StreamReader(contentStream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    Sentry.Common.Logging.Logger.Info($"Companies request result message: {response.Content.ReadAsStreamAsync()}");

                    try
                    {
                        JObject responseObject = JObject.Load(jsonReader);
                        if (string.IsNullOrEmpty(responseObject.Value<string>("error")))
                        {
                            JArray companies = (JArray)responseObject["companies"];
                            JObject firstCompany = (JObject)companies[0];
                            token.TokenName = firstCompany.GetValue("company").Value<string>("name");
                            token.ForeignId = firstCompany.GetValue("company").Value<string>("company_id");
                            if(((HTTPSSource)motiveSource).AllTokens != null)
                            {
                                foreach (var existingToken in ((HTTPSSource)motiveSource).AllTokens)
                                {
                                    if (!string.IsNullOrEmpty(existingToken.ForeignId) && string.Equals(existingToken.ForeignId, token.ForeignId))
                                    {
                                        existingToken.Enabled = false;
                                        _emailService.SendMotiveDuplicateTokenEmail(token, existingToken);
                                    }
                                }
                            }
                            if (_featureFlags.CLA4485_DropCompaniesFile.GetValue())
                            {
                                var s3Drop = _dataFlowService.GetDataFlowStepForDataFlowByActionType(companiesDataflowId, DataActionType.S3Drop);
                                _s3ServiceProvider.UploadDataFile(contentStream, s3Drop.TriggerBucket, s3Drop.TriggerKey);
                            }
                        }
                        _datasetContext.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Sentry.Common.Logging.Logger.Error("Parsing companies response failed.", e);
                    }

                }
            }
        }
    }
}
