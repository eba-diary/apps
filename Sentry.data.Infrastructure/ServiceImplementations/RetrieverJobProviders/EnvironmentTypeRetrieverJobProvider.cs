using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public abstract class EnvironmentTypeRetrieverJobProvider : IDfsRetrieverJobProvider
    {
        private readonly IDatasetContext _datasetContext;

        protected EnvironmentTypeRetrieverJobProvider(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public abstract List<string> AcceptedNamedEnvironments { get; }
        protected abstract bool IsProd(string requestingNamedEnvironment);

        public List<RetrieverJob> GetDfsRetrieverJobs(string requestingNamedEnvironment)
        {
            NamedEnvironmentType environmentType;
            IQueryable<RetrieverJob> jobsQueryable = _datasetContext.RetrieverJob.Where(x => x.ObjectStatus == ObjectStatusEnum.Active);

            if (IsProd(requestingNamedEnvironment))
            {
                environmentType = NamedEnvironmentType.Prod;
                jobsQueryable = jobsQueryable.Where(x => x.DataSource is DfsProdSource);
            }
            else
            {
                environmentType = NamedEnvironmentType.NonProd;
                jobsQueryable = jobsQueryable.Where(x => x.DataSource is DfsNonProdSource);
            }

            //get retriever jobs by legacy DFS data source type and dataset environment type (prevents N+1 NHibernate lazy load issue)
            var jobInfos = _datasetContext.RetrieverJob.Join(_datasetContext.DataFlow, rj => rj.DataFlow.Id, df => df.Id, (rj, df) =>
                    new { job = rj, dataFlow = df })
                .Join(_datasetContext.DataSources, j => j.job.DataSource.Id, s => s.Id, (j, s) =>
                    new { job = j.job, dataFlow = j.dataFlow, dataSource = s })
                .Join(_datasetContext.Datasets, j => j.dataFlow.DatasetId, d => d.DatasetId, (j, d) =>
                    new { job = j.job, dataFlow = j.dataFlow, dataSource = j.dataSource, dataset = d })
                .Where(x => x.job.ObjectStatus == ObjectStatusEnum.Active &&
                    x.dataSource is DfsDataFlowBasic &&
                    x.dataset.NamedEnvironmentType == environmentType)
                .Select(x => new { job = x.job, dataFlow = x.dataFlow, dataSource = x.dataSource })
                .ToList();

            //map eager loaded dataflow and datasource to retriever job
            List<RetrieverJob> dfsDataFlowBasicJobs = new List<RetrieverJob>();
            foreach (var jobInfo in jobInfos)
            {
                jobInfo.job.DataFlow = jobInfo.dataFlow;
                jobInfo.job.DataSource = jobInfo.dataSource;

                dfsDataFlowBasicJobs.Add(jobInfo.job);
            }

            //get retriever jobs by new DFS data source type
            List<RetrieverJob> jobs = jobsQueryable.Fetch(x => x.DataSource).Fetch(x => x.DataFlow).ToList();

            //combine results
            jobs.AddRange(dfsDataFlowBasicJobs);

            return jobs;
        }
    }
}
