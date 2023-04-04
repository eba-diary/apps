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
        private readonly PagingHttpsJobProvider _backfillJobProvider;

        public MotiveProvider(HttpClient httpClient, IS3ServiceProvider s3ServiceProvider, IDatasetContext datasetContext, 
                              IDataFlowService dataFlowService, IAuthorizationProvider authorizationProvider, IDataFeatures featureFlags, IEmailService emailService, PagingHttpsJobProvider backfillJobProvider)
        {
            client = httpClient;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetContext = datasetContext;
            _dataFlowService = dataFlowService;
            _authorizationProvider = authorizationProvider;
            _featureFlags = featureFlags;
            _emailService = emailService;
            _backfillJobProvider = backfillJobProvider;
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
                                    if (!string.IsNullOrEmpty(existingToken.ForeignId) && string.Equals(existingToken.ForeignId, token.ForeignId) && existingToken.Id != token.Id)
                                    {
                                        existingToken.Enabled = false;
                                        _emailService.SendMotiveDuplicateTokenEmail(token, existingToken);
                                    }
                                }
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
            client.DefaultRequestHeaders.Remove("Authorization"); //Clean the Auth Header out 
        }

        public bool MotiveTokenBackfill(DataSourceToken tokenToBackfill)
        {
            try
            {
                var motiveDataset = _datasetContext.GetById<Dataset>(int.Parse(Config.GetHostSetting("MotiveDatasetId")));
                var schemaIdList = _datasetContext.DatasetFileConfigs.Where(dsfc => dsfc.ParentDataset == motiveDataset).Select(df => df.Schema.SchemaId).ToList();
                var dataflows = _datasetContext.DataFlow.Where(df => schemaIdList.Contains(df.SchemaId)).ToList();
                //Get all of our jobs for schemas that do not create a current view - current view backfill would lead to duplicate data
                var jobs = _datasetContext.Jobs.Where(j => schemaIdList.Contains(j.FileSchema.SchemaId) && !j.FileSchema.CreateCurrentView).ToList();

                HTTPSSource source = _datasetContext.GetById<HTTPSSource>(int.Parse(Config.GetHostSetting("MotiveDataSourceId")));
                
                //Disable all other tokens and make a list to enable them again later
                List<int> tokensToEnable = new List<int>();
                foreach (var token in source.AllTokens.Where(t => t.Enabled))
                {
                    tokensToEnable.Add(token.Id);
                    token.Enabled = false;
                }

                tokenToBackfill.Enabled = true;

                //change start date
                foreach (var job in jobs)
                {
                    var dateParameter = job.RequestVariables.First(rv => rv.VariableName == "dateValue");
                    var currentDateValue = dateParameter.VariableValue; //hold onto old value
                    dateParameter.VariableValue = Config.GetHostSetting("MotiveBackfillDate");
                    _backfillJobProvider.Execute(job);
                    dateParameter.VariableValue = currentDateValue;
                }

                //clean up
                //reset retriever job params
                foreach (var token in source.AllTokens.Where(t => tokensToEnable.Contains(t.Id)))
                {
                    token.Enabled = true;
                }

                tokenToBackfill.BackfillComplete = true;
                _datasetContext.SaveChanges();

                return true;
            }
            catch(Exception e)
            {
                Common.Logging.Logger.Fatal($"Backfill on token {tokenToBackfill.TokenName} failed.", e);
                return false;
            }
        }
    }
}
