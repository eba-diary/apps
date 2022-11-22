using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sentry.data.Core
{
    public class DataSourceService : IDataSourceService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly IUserService _userService;
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IDataFeatures _featureFlags;
        private readonly IDatasetFileService _datasetFileService;
        private readonly IHttpClientProvider _httpClient;

        public DataSourceService(IDatasetContext datasetContext, ISecurityService securityService,
                            IUserService userService, IConfigService configService,
                            ISchemaService schemaService,
                            IQuartermasterService quartermasterService,
                            IDataFeatures featureFlags,
                            IDatasetFileService datasetFileService,
                            IHttpClientProvider httpClient)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
            _configService = configService;
            _schemaService = schemaService;
            _quartermasterService = quartermasterService;
            _featureFlags = featureFlags;
            _datasetFileService = datasetFileService;
            _httpClient = httpClient;
        }

        public async void ExchangeAuthToken(DataSource dataSource, string authToken)
        {
            var content = new Dictionary<string, string>
            {
              {"grant_type", "authorization_code"}, {"code", authToken}, {"redirect_uri", "redirect"}, {"client_id", ((HTTPSSource)dataSource).ClientId }, {"client_secret", ((HTTPSSource)dataSource).ClientPrivateId } //todo decrypt
            };
            var jsonContent = JsonConvert.SerializeObject(content);
            var jsonPostContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.gomotive.com/oauth/token", jsonPostContent);
            JObject responseAsJson = JObject.Parse(await response.Content.ReadAsStringAsync());
            string accessToken = responseAsJson.Value<string>("access_token");
            string refreshToken = responseAsJson.Value<string>("refresh_token");
            //((HTTPSSource)dataSource).Tokens;
        }
    }
}