using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class AnonymousAuthentication : AuthenticationType
    {
        public AnonymousAuthentication() { }
        public override string AuthType
        {
            get
            {
                return "AnonAuth";
            }
        }

        public override NetworkCredential GetCredentials(RetrieverJob Job)
        {
            return new NetworkCredential(Configuration.Config.GetHostSetting("RTJob_AnonAuth_UserName"), Configuration.Config.GetHostSetting("RTJob_AnonAuth_Password"));
        }
    }
}
