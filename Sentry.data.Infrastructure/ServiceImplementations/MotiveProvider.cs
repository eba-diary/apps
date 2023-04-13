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
        private readonly IDatasetContext _datasetContext;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IEmailService _emailService;
        private readonly IBaseJobProvider _backfillJobProvider;

        public MotiveProvider(HttpClient httpClient, IDatasetContext datasetContext, 
                             IAuthorizationProvider authorizationProvider, IEmailService emailService, IBaseJobProvider backfillJobProvider)
        {
            client = httpClient;
            _datasetContext = datasetContext;
            _authorizationProvider = authorizationProvider;
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

        /// <summary>
        /// Backfills data for Motive dataset by grabbing all jobs and running with only new token enabled. Config property "MotiveBackfillDate" controls the backfill start date. 
        /// </summary>
        /// <param name="tokenToBackfill">Token we want load data for.</param>
        /// <returns></returns>
        public bool MotiveTokenBackfill(DataSourceToken tokenToBackfill)
        {
            try
            {
                var datasetFileConfigs = _datasetContext.DatasetFileConfigs.Where(dsfc => dsfc.ParentDataset.DatasetId == int.Parse(Config.GetHostSetting("MotiveDatasetId")));
                var schemaIdList = datasetFileConfigs.Select(df => df.Schema.SchemaId).ToList();

                //Get all of our jobs for schemas that do not create a current view - current view backfill would lead to duplicate data
                var jobs = _datasetContext.Jobs.Where(j => schemaIdList.Contains(j.FileSchema.SchemaId) && !j.FileSchema.CreateCurrentView && j.IsEnabled).ToList();

                HTTPSSource source = _datasetContext.GetById<HTTPSSource>(int.Parse(Config.GetHostSetting("MotiveDataSourceId")));
                
                //Disable all other tokens and make a list to enable them again later
                List<int> tokensToEnable = new List<int>();
                foreach (var token in source.AllTokens.Where(t => t.Enabled))
                {
                    tokensToEnable.Add(token.Id);
                    token.Enabled = false;
                }

                tokenToBackfill.Enabled = true;

                try
                {
                    //change start date and trigger jobs
                    foreach (var job in jobs)
                    {
                        Common.Logging.Logger.Info($"Attempting backfill of {job.DataFlow.Name} on token {tokenToBackfill}");
                        var dateParameter = job.RequestVariables.First(rv => rv.VariableName == "dateValue");
                        var currentDateValue = dateParameter.VariableValue; //hold onto old value
                        dateParameter.VariableValue = Config.GetHostSetting("MotiveBackfillDate");
                        _backfillJobProvider.Execute(job);
                        //reset retriever job param
                        dateParameter.VariableValue = currentDateValue;
                    }

                    tokenToBackfill.BackfillComplete = true;
                }
                catch (Exception e) //Catch error here so we continue on to restore tokens. 
                {
                    Common.Logging.Logger.Error($"Backfill on token {tokenToBackfill.TokenName} failed running the jobs.", e);
                }

                //clean up
                foreach (var token in source.AllTokens.Where(t => tokensToEnable.Contains(t.Id)))
                {
                    token.Enabled = true;
                }

                _datasetContext.SaveChanges();

                return true;
            }
            catch(Exception e)
            {
                Common.Logging.Logger.Error($"Backfill on token {tokenToBackfill.TokenName} failed.", e);
                return false;
            }
        }
    }
}
