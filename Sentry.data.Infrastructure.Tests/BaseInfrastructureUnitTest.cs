using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net;

namespace Sentry.data.Infrastructure.Tests
{
    public class BaseInfrastructureUnitTest
    {
        protected JObject GetData(string fileName)
        {
            return JObject.Parse(GetDataString(fileName));
        }

        protected string GetDataString(string fileName)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\{fileName}"))
            {
                return rdr.ReadToEnd().Replace("\r\n", string.Empty);
            }
        }

        protected HttpResponseMessage GetResponseMessage(string filename)
        {
            return CreateResponseMessage(GetDataString(filename));
        }

        protected HttpResponseMessage CreateResponseMessage(string data)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(data),
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
