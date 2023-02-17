﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataSourceService : IDataSourceService
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        private readonly IEncryptionService _encryptionService;
        private readonly IMotiveProvider _motiveProvider;
        private readonly HttpClient client;
        #endregion

        #region Constructor
        public DataSourceService(IDatasetContext datasetContext,
                            IEncryptionService encryptionService,
                            HttpClient httpClient,
                            IMotiveProvider motiveProvider)
        {
            _datasetContext = datasetContext;
            _encryptionService = encryptionService;
            client = httpClient;
            _motiveProvider = motiveProvider;
        }
        #endregion

        #region IDataSourceService Implementations
        public List<DataSourceTypeDto> GetDataSourceTypeDtosForDropdown()
        {
            List<string> dropdownTypes = new List<string>
            {
                DataSourceDiscriminator.FTP_SOURCE,
                DataSourceDiscriminator.SFTP_SOURCE,
                DataSourceDiscriminator.DFS_CUSTOM,
                DataSourceDiscriminator.HTTPS_SOURCE,
                DataSourceDiscriminator.GOOGLE_API_SOURCE,
                DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE
            };

            List<DataSourceTypeDto> dataSourceDtos = _datasetContext.DataSourceTypes.Where(x => dropdownTypes.Contains(x.DiscrimatorValue))
                .Select(x => new DataSourceTypeDto { Name = x.Name, Description = x.Description, DiscrimatorValue = x.DiscrimatorValue})
                .ToList();

            return dataSourceDtos;
        }

        public List<AuthenticationTypeDto> GetValidAuthenticationTypeDtosByType(string sourceType)
        {
            List<AuthenticationType> validAuthTypes;

            switch (sourceType)
            {
                case DataSourceDiscriminator.FTP_SOURCE:
                    validAuthTypes = new FtpSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.SFTP_SOURCE:
                    validAuthTypes = new SFtpSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.DFS_CUSTOM:
                    validAuthTypes = new DfsCustom().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.HTTPS_SOURCE:
                    validAuthTypes = new HTTPSSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    validAuthTypes = new GoogleApiSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    validAuthTypes = new GoogleBigQueryApiSource().ValidAuthTypes;
                    break;
                default:
                    throw new NotImplementedException();
            }

            List<string> validAuthTypeCodes = validAuthTypes.Select(x => x.AuthType).ToList();

            List<AuthenticationType> allAuthTypes = _datasetContext.AuthTypes.ToList();
            List<AuthenticationType> fullValidAuthTypes = allAuthTypes.Where(x => validAuthTypeCodes.Contains(x.AuthType)).ToList();

            List<AuthenticationTypeDto> authenticationTypeDtos = fullValidAuthTypes.Select(x => x.ToDto()).ToList();

            return authenticationTypeDtos;
        }

        public async Task<bool> ExchangeAuthToken(DataSource dataSource, string authToken)
        {
            var content = new Dictionary<string, string>
            {
              {"grant_type", "authorization_code"}, {"code", authToken}, {"redirect_uri", Configuration.Config.GetHostSetting("MotiveRedirectURI")}, {"client_id", ((HTTPSSource)dataSource).ClientId }, {"client_secret", _encryptionService.DecryptString(((HTTPSSource)dataSource).ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)dataSource).IVKey) }
            };
            var jsonContent = JsonConvert.SerializeObject(content);
            var jsonPostContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var stringContent = jsonContent.ToString();
            Sentry.Common.Logging.Logger.Info($"Attempting to add token {authToken} to datasource {dataSource.Name} with payload: {stringContent}");
            try
            {
                using (var response = await client.PostAsync("https://api.gomotive.com/oauth/token", jsonPostContent))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Sentry.Common.Logging.Logger.Info($"Response {response.StatusCode}: {responseContent}");
                    JObject responseAsJson = JObject.Parse(responseContent);
                    string accessToken = responseAsJson.Value<string>("access_token");
                    string refreshToken = responseAsJson.Value<string>("refresh_token");
                    if(String.IsNullOrEmpty(accessToken) || String.IsNullOrEmpty(refreshToken))
                    {
                        Sentry.Common.Logging.Logger.Error($"Unable to parse response tokens from JSON: {responseContent}");
                        return false;
                    }

                    var newToken = new DataSourceToken()
                    {
                        CurrentToken = _encryptionService.EncryptString(accessToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)dataSource).IVKey).Item1,
                        RefreshToken = _encryptionService.EncryptString(refreshToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)dataSource).IVKey).Item1,
                        ParentDataSource = ((HTTPSSource)dataSource),
                        TokenExp = 7200,
                        TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
                    };

                    ((HTTPSSource)dataSource).Tokens.Add(newToken);
                    _datasetContext.SaveChanges();
                    _motiveProvider.MotiveOnboardingAsync((HTTPSSource)dataSource, newToken, int.Parse(Configuration.Config.GetHostSetting("MotiveCompaniesDataFlowId")));
                }
            }
            catch (Exception e)
            {
                Logger.Fatal($"Token exchanged failed with Auth Token {authToken}. Exception {e.Message}.");
                return false;
            }
            return true;
        }

        public List<AuthenticationTypeDto> GetAuthenticationTypeDtos()
        {
            List<AuthenticationType> allAuthTypes = _datasetContext.AuthTypes.ToList();
            List<AuthenticationTypeDto> authenticationTypeDtos = allAuthTypes.Select(x => x.ToDto()).ToList();
            return authenticationTypeDtos;
        }

        #endregion
    }
}
