using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class LambdaClientConfig
    {
        public string AWSRegion { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string ProxyHost { get; set; }
        public string ProxyPort { get; set; }
    }
}
