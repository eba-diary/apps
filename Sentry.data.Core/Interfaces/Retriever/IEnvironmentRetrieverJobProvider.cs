using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDfsRetrieverJobProvider
    {
        List<string> AcceptedNamedEnvironments { get; }
        List<RetrieverJob> GetDfsRetrieverJobs(string requestingNamedEnvironment);
    }
}
