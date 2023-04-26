using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataSourceService : BaseDomainService<DataSourceService>, IDataSourceService
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        private readonly IEncryptionService _encryptionService;
        private readonly IMotiveProvider _motiveProvider;
        private readonly HttpClient client;
        private readonly IEmailService _emailService;
        #endregion

        #region Constructor
        public DataSourceService(IDatasetContext datasetContext,
                            IEncryptionService encryptionService,
                            HttpClient httpClient,
                            IMotiveProvider motiveProvider,
                            IEmailService emailService, DomainServiceCommonDependency<DataSourceService> commonDependencies) : base(commonDependencies)
        {
            _datasetContext = datasetContext;
            _encryptionService = encryptionService;
            client = httpClient;
            _motiveProvider = motiveProvider;
            _emailService = emailService;
        }
        #endregion

        #region IDataSourceService Implementations
        public List<DataSourceTypeDto> GetDataSourceTypeDtosForDropdown()
        {
            List<string> dropdownTypes = new List<string>
            {
                DataSourceDiscriminator.FTP_SOURCE,
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
            _logger.LogInformation($"Attempting to add token {authToken} to datasource {dataSource.Name} with payload: {stringContent}");
            try
            {
                using (var response = await client.PostAsync("https://api.gomotive.com/oauth/token", jsonPostContent))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response {response.StatusCode}: {responseContent}");
                    JObject responseAsJson = JObject.Parse(responseContent);
                    string accessToken = responseAsJson.Value<string>("access_token");
                    string refreshToken = responseAsJson.Value<string>("refresh_token");
                    if(String.IsNullOrEmpty(accessToken) || String.IsNullOrEmpty(refreshToken))
                    {
                        _logger.LogError($"Unable to parse response tokens from JSON: {responseContent}");
                        return false;
                    }

                    var newToken = new DataSourceToken()
                    {
                        CurrentToken = _encryptionService.EncryptString(accessToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)dataSource).IVKey).Item1,
                        RefreshToken = _encryptionService.EncryptString(refreshToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)dataSource).IVKey).Item1,
                        ParentDataSource = ((HTTPSSource)dataSource),
                        TokenExp = 7200,
                        TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret",
                        Enabled = false,
                        BackfillComplete = false
                    };

                    try
                    {
                        _logger.LogInformation("Attempting to onboard new token.");
                        await _motiveProvider.MotiveOnboardingAsync((HTTPSSource)dataSource, newToken, int.Parse(Configuration.Config.GetHostSetting("MotiveCompaniesDataFlowId")));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Onboarding new token failed with message.");
                    }

                    ((HTTPSSource)dataSource).AllTokens.Add(newToken);
                    _datasetContext.SaveChanges();
                    _logger.LogInformation($"Successfully saved new token.");

                    if (_dataFeatures.CLA4931_SendMotiveEmail.GetValue())
                    {
                        _emailService.SendNewMotiveTokenAddedEmail(newToken);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Token exchanged failed with Auth Token {authToken}.");
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

        public async Task<bool> KickOffMotiveOnboarding(int tokenId)
        {
            try
            {
                _logger.LogInformation("Attempting to onboard token.");
                var dataSource = _datasetContext.DataSources.FirstOrDefault(ds => ds.Id == int.Parse(Configuration.Config.GetHostSetting("MotiveDataSourceId")));
                var token = ((HTTPSSource)dataSource).AllTokens.First(t => t.Id == tokenId);
                await _motiveProvider.MotiveOnboardingAsync((HTTPSSource)dataSource, token, int.Parse(Configuration.Config.GetHostSetting("MotiveCompaniesDataFlowId")));
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Onboarding token failed with message.");
                return false;
            }
        }

        public bool KickOffMotiveBackfill(int tokenId)
        {
            try
            {
                var token = _datasetContext.GetById<DataSourceToken>(tokenId);
                _motiveProvider.EnqueueBackfillBackgroundJob(token);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Backfilling token {tokenId} failed with message.");
                return false;
            }
        }

        #endregion
    }
}
