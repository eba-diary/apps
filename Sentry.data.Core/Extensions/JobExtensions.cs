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

        public static RetrieverJobDto ToDto(this RetrieverJob job)
        {
            RetrieverJobDto dto = new RetrieverJobDto
            {
                JobId = job.Id,
                Schedule = job.Schedule,
                ReadableSchedule = job.ReadableSchedule,
                RelativeUri = job.RelativeUri,
                HttpRequestBody = job.JobOptions?.HttpOptions?.Body,
                SearchCriteria = job.JobOptions?.SearchCriteria,
                TargetFileName = job.JobOptions?.TargetFileName,
                CreateCurrentFile = job.JobOptions?.CreateCurrentFile ?? false,
                DataSourceId = job.DataSource.Id,
                DataSourceType = job.DataSource.SourceType,
                DatasetFileConfig = job.DatasetConfig?.ConfigId ?? 0,
                DataFlow = job.DataFlow.Id,
                RequestMethod = job.JobOptions?.HttpOptions?.RequestMethod ?? null,
                RequestDataFormat = job.JobOptions?.HttpOptions?.RequestDataFormat ?? null,
                FtpPattern = job.JobOptions?.FtpPattern ?? null,
                ExecutionParameters = job.ExecutionParameters,
                PagingType = job.JobOptions?.HttpOptions?.PagingType ?? PagingType.None,
                PageParameterName = job.JobOptions?.HttpOptions?.PageParameterName,
                RequestVariables = job.RequestVariables.Select(x => x.ToDto()).ToList()
            };

            return dto;
        }

        public static RequestVariable ToEntity(this RequestVariableDto dto)
        {
            return new RequestVariable
            {
                VariableName = dto.VariableName,
                VariableValue = dto.VariableValue,
                VariableIncrementType = dto.VariableIncrementType
            };
        }

        public static RequestVariableDto ToDto(this RequestVariable entity)
        {
            return new RequestVariableDto
            {
                VariableName = entity.VariableName,
                VariableValue = entity.VariableValue,
                VariableIncrementType = entity.VariableIncrementType
            };
        }
    }
}
