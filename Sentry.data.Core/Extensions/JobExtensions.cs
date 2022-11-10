using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nest;
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
            var dataflows = datasetContext.DataFlow.Where(x => query.Any(y => x.Id == y.DataFlow.Id));
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

        public static IQueryable<RetrieverJob> WhereActive(this IQueryable<RetrieverJob> query)
        {
            return query.Where(w => w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active);
        }

        public static RetrieverJobDto ToDto(this RetrieverJob job)
        {
            RetrieverJobDto dto = new RetrieverJobDto();

            dto.JobId = job.Id;
            dto.Schedule = job.Schedule;
            dto.ReadableSchedule = job.ReadableSchedule;
            dto.RelativeUri = job.RelativeUri;
            dto.HttpRequestBody = job.JobOptions?.HttpOptions?.Body;
            dto.SearchCriteria = job.JobOptions?.SearchCriteria;
            dto.TargetFileName = job.JobOptions?.TargetFileName;
            dto.CreateCurrentFile = job.JobOptions?.CreateCurrentFile ?? false;
            dto.DataSourceId = job.DataSource.Id;
            dto.DataSourceType = job.DataSource.SourceType;
            //FileSchema = job.FileSchema?.SchemaId?? 0,
            dto.DatasetFileConfig = job.DatasetConfig?.ConfigId?? 0;
            dto.DataFlow = job.DataFlow.Id;
            dto.RequestMethod = job.JobOptions?.HttpOptions?.RequestMethod ?? null;
            dto.RequestDataFormat = job.JobOptions?.HttpOptions?.RequestDataFormat ?? null;
            dto.FtpPattern = job.JobOptions?.FtpPattern ?? null;
            dto.ExecutionParameters = job.ExecutionParameters;

            return dto;
        }
    }
}
