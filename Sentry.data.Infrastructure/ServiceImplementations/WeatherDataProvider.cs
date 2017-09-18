using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Sentry.data.Core;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Sentry.data.Infrastructure
{
    class WeatherDataProvider : IWeatherDataProvider
    {
        private string _baseurl;
        private RestClient _client;
        private RestRequest _request;

        public WeatherDataProvider()
        {
            //_baseurl = "https://www.ncdc.noaa.gov/cdo-web/api/v2/{endpoint}";
            //CreateClient(_baseurl);
            //CreateReuqest();
        }

        public RestClient CreateClient(string baseurl)
        {
            _client = new RestClient(baseurl);
            _client.Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"));
            _client.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            return _client;
        }

        private void CreateReuqest()
        {
            if (_request == null)
            {
                RestRequest request = new RestRequest();
                request.AddHeader("token", Configuration.Config.GetHostSetting("NOAA_APIKey"));
                
                _request = request;
            }            
        }

        public List<JObject> GetWeatherDatasetsList()
        {
            List<JObject> files = new List<JObject>();

            _request.AddParameter("endpoint", "datasets", ParameterType.UrlSegment);
            _request.AddQueryParameter("limit", "1000");

            _request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            
            IRestResponse response = _client.Execute(_request);

            JObject result = JObject.Parse(response.Content);

            var count = JObject.Parse(response.Content).Descendants().Where(x => x is Object).Where(x => x["resultset"] != null && x["count"] != null).Select(x => new { count = (int)x["count"] });

            int? cnt = result?["metadata"]?["resultset"].ToObject<JToken>().Value<int?>("count");

            files.Add(result);

            //JToken count = result.SelectToken("metadata").Select(s => s.SelectToken("resultset"));
            //result.SelectToken(@"metadata")


            return files;
        }

        public List<JObject> GetWeatherDatasetsList(string endpoint, IList<RTRequestParameters> parameters)
        {
            List<JObject> files = new List<JObject>();

            _request.AddParameter("endpoint", endpoint, ParameterType.UrlSegment);

            foreach (RTRequestParameters param in parameters)
            {
                _request.AddQueryParameter(param.ApiParameter.Name, param.Value);
            }

            //_request.AddQueryParameter("limit", "1000");

            _request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };


                IRestResponse response = _client.Execute(_request);
                if (response.ErrorException != null)
                {
                    const string message = "Error retieving response. Check inner details for more info.";
                    var e = new ApplicationException(message, response.ErrorException);
                    throw e;
                }


            JObject result = JObject.Parse(response.Content);

            var count = JObject.Parse(response.Content).Descendants().Where(x => x is Object).Where(x => x["resultset"] != null && x["count"] != null).Select(x => new { count = (int)x["count"] });

            int? cnt = result?["metadata"]?["resultset"].ToObject<JToken>().Value<int?>("count");

            files.Add(result);

            //JToken count = result.SelectToken("metadata").Select(s => s.SelectToken("resultset"));
            //result.SelectToken(@"metadata")


            return files;
        }





    }
    
}
