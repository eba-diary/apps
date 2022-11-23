using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class AllDfsRetrieverJobProvider : IDfsRetrieverJobProvider
    {
        private readonly IDatasetContext _datasetContext;

        public AllDfsRetrieverJobProvider(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public List<string> AcceptedNamedEnvironments => new List<string>() { DLPPEnvironments.TEST, DLPPEnvironments.NRTEST };

        public List<RetrieverJob> GetDfsRetrieverJobs(string requestingNamedEnvironment)
        {
            List<RetrieverJob> jobs = _datasetContext.RetrieverJob.Where(x => x.ObjectStatus == ObjectStatusEnum.Active &&
                    (x.DataSource is DfsDataFlowBasic || x.DataSource is DfsProdSource || x.DataSource is DfsNonProdSource))
                .Fetch(x => x.DataSource)
                .Fetch(x => x.DataFlow)
                .ToList();

            return jobs;
        }
    }
}
