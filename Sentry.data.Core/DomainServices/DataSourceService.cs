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
        private readonly IEncryptionService _encryptionService;
        private readonly IHttpClientProvider _httpClient;

        public DataSourceService(IDatasetContext datasetContext,
                            IEncryptionService encryptionService,
                            IHttpClientProvider httpClient)
        {
            _datasetContext = datasetContext;
            _encryptionService = encryptionService;
            _httpClient = httpClient;
        }

        public async void ExchangeAuthToken(DataSource dataSource, string authToken)
        {
            var content = new Dictionary<string, string>
            {
              {"grant_type", "authorization_code"}, {"code", authToken}, {"redirect_uri", "redirect"}, {"client_id", ((HTTPSSource)dataSource).ClientId }, {"client_secret", _encryptionService.DecryptString(((HTTPSSource)dataSource).ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)dataSource).IVKey) }
            };
            var jsonContent = JsonConvert.SerializeObject(content);
            var jsonPostContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.gomotive.com/oauth/token", jsonPostContent);
            JObject responseAsJson = JObject.Parse(await response.Content.ReadAsStringAsync());
            string accessToken = responseAsJson.Value<string>("access_token");
            string refreshToken = responseAsJson.Value<string>("refresh_token");
            ((HTTPSSource)dataSource).Tokens.Add(new DataSourceToken() {
                CurrentToken = accessToken,
                RefreshToken = refreshToken,
                TokenExp = 7200,
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
                //company name as token name once we start dropping comapny file.
            });
            _datasetContext.SaveChanges();
        }
    }
}