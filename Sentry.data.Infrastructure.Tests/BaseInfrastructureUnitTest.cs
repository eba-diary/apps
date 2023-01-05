using Newtonsoft.Json.Linq;
using System.IO;

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
    }
}
