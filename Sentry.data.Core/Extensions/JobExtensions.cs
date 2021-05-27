using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RestSharp;
using Sentry.Core;

namespace Sentry.data.Core
{
    public static class JobExtensions
    {
        public static Submission ToSubmission(this RetrieverJob job)
        {
            return new Submission()
            {
                JobId = job,
                JobGuid = Guid.NewGuid(),
                Created = DateTime.Now,
                Serialized_Job_Options = ""
            };
        }
        public static List<RetrieverJob> FetchAllConfiguration(this IQueryable<RetrieverJob> query, IDatasetContext datasetContext)
        {
            /* Retrieve Config(s) and Schema(s) */

            var configs = datasetContext.DatasetFileConfigs.Where(x => query.Any(y => x.ConfigId == y.DatasetConfig.ConfigId));
            var schemas = datasetContext.DataElements.Where(x => configs.Any(y => x.DatasetFileConfig.ConfigId == y.ConfigId));
            var dataflows = datasetContext.DataFlow.Where(x => query.Any(y => x.Id == y.DataFlow.Id));
            schemas.FetchMany(x => x.DataElementDetails).ToFuture();
            schemas.FetchMany(x => x.DataObjects).ToFuture();
            query.Fetch(x => x.DatasetConfig).ThenFetchMany(x => x.Schemas).ToFuture();
            query.Fetch(x => x.DataFlow).ToFuture();

            var jobs = query.Fetch(f => f.DatasetConfig).ThenFetch(x => x.ParentDataset).Fetch(f => f.DataSource).ToFuture();

            return jobs.ToList();
        }

        public static void CopyToStream(this IRestResponse resp, Stream targetStream)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(resp.Content);
            using(MemoryStream stream = new MemoryStream(byteArray))
            {
                stream.CopyTo(targetStream);
            }                      
        }

        public static List<RetrieverJob> FetchParentMetadata(this IQueryable<RetrieverJob> query, IDatasetContext datasetContext)
        {
            var datasetfileconfigs = datasetContext.DatasetFileConfigs.Where(w => query.Any(d => w.ConfigId == d.DatasetConfig.ConfigId));
            var dataset = datasetContext.Datasets.Where(w => datasetfileconfigs.Any(d => w.DatasetId == d.ParentDataset.DatasetId));

            var tree = query.Fetch(f => f.DatasetConfig).ThenFetch(x => x.ParentDataset).ToFuture();

            return tree.ToList();
        }
    }
}
