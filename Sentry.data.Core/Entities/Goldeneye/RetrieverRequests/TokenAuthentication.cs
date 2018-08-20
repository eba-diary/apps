using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class TokenAuthentication : AuthenticationType
    {
        public override string AuthType
        {
            get
            {
                return "TokenAuth";
            }
        }

        public override NetworkCredential GetCredentials(RetrieverJob Job)
        {
            return System.Net.CredentialCache.DefaultNetworkCredentials;
        }
    }
}
