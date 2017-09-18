using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Sentry.data.Infrastructure
{
    class RestAPIProvider
    {
        private RestClient _client;
        private string _baseURL;

        public RestAPIProvider()
        {
            if (_client == null)
            {
                _client = new RestClient();
            }                                        
        }
    }    
}
