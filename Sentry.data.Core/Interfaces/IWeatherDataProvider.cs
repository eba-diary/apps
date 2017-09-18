using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Sentry.data.Core
{
    public interface IWeatherDataProvider
    {
        List<JObject> GetWeatherDatasetsList();

        RestClient CreateClient(string baseurl);
        
    }
}
