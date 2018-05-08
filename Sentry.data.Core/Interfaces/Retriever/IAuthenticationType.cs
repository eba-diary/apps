using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAuthenticationType
    {
        int AuthID { get; set; }
        string AuthType { get; set; }
        string AuthName { get; set; }
        string Description { get; set; }
    }
}
