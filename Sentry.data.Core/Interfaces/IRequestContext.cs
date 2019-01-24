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
        IQueryable<DataSource> DataSource { get; }

        IQueryable<RetrieverJob> RetrieverJob { get; }

        IQueryable<AuthenticationType> AuthenticationType { get; }

        IQueryable<DataSourceType> DataSourceTypes { get; }

        IQueryable<ApplicationConfiguration> ApplicaitonConfigurations { get; }

        IList<RTSourceTypes> GetSourceTypes();

        IList<RTRequest> GetEnabledRequests();

        RTRequest GetRequest(int id);
    }
}
