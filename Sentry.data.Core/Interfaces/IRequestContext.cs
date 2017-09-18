using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.data.Core
{
    public interface IRequestContext : IWritableDomainContext
    {
        IList<RTSourceTypes> GetSourceTypes();

        IList<RTRequest> GetEnabledRequests();

        RTRequest GetRequest(int id);
    }
}
