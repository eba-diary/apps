using Newtonsoft.Json.Linq;
using System.IO;

namespace Sentry.data.Infrastructure.Tests
{
    public class BaseInfrastructureUnitTest
    {
        protected JObject GetData(string fileName)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\{fileName}"))
            {
                return JObject.Parse(rdr.ReadToEnd().Replace("\r\n", string.Empty));
            }
        }
    }
}
