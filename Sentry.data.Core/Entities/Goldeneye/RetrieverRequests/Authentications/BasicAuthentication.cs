using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BasicAuthentication : AuthenticationType
    {
        public override string AuthType
        {
            get
            {
                return "BasicAuth";
            }
        }

        public override NetworkCredential GetCredentials(RetrieverJob Job)
        {
            return new NetworkCredential(Configuration.Config.GetHostSetting($"RTJob_{Job.DataSource.KeyCode}_UserName"), Configuration.Config.GetHostSetting($"RTJob_{Job.DataSource.KeyCode}_Password"));
        }
    }
}
