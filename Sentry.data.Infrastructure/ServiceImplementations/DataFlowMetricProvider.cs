using Nest;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class DataFlowMetricProvider : IDataFlowMetricProvider
    {
        private readonly IElasticDocumentClient _elasticDocumentClient;

        public DataFlowMetricProvider(IElasticDocumentClient elasticDocumentClient)
        {
            _elasticDocumentClient = elasticDocumentClient;
        }

        public ElasticResult<DataFlowMetric> GetAllTotalFilesByDataset(int datasetId)
        {
            return GetTotalFiles(datasetId: datasetId);
        }

        public ElasticResult<DataFlowMetric> GetAllTotalFilesBySchema(int schemaId)
        {
            return GetTotalFiles(schemaId: schemaId);
        }

        public ElasticResult<DataFlowMetric> GetAllTotalFiles()
        {
            return GetTotalFiles();
        }

        public ElasticResult<DataFlowMetric> GetAllFailedFilesByDataset(int datasetId)
        {
            return GetFailedFiles(datasetId: datasetId);
        }

        public ElasticResult<DataFlowMetric> GetAllFailedFilesBySchema(int schemaId)
        {
            return GetFailedFiles(schemaId: schemaId);
        }

        public ElasticResult<DataFlowMetric> GetAllFailedFiles()
        {
            return GetFailedFiles();
        }

        public ElasticResult<DataFlowMetric> GetAllInFlightFilesByDataset(int datasetId)
        {
            return GetInFlightFiles(datasetId: datasetId);
        }

        public ElasticResult<DataFlowMetric> GetAllInFlightFilesBySchema(int schemaId)
        {
            return GetInFlightFiles(schemaId: schemaId);
        }

        public ElasticResult<DataFlowMetric> GetAllInFlightFiles()
        {
            return GetInFlightFiles();
        }

        //returns list of data flow metrics matching searchdto criteria
        public List<DataFlowMetric> GetDataFlowMetrics(DataFlowMetricSearchDto dto)
        {
            // List of query container decriptors to filter elastic search
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> filters = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();

            // Filters added to query container list to search for selecteds dataset and schema id's
            filters.Add(fq => fq.Terms(t => t.Field(f => f.DatasetId).Terms(dto.DatasetId.ToString())));
            filters.Add(fq => fq.Terms(t => t.Field(f => f.SchemaId).Terms(dto.SchemaId.ToString())));

            // Checks if a specific dataset file has been selected
            if (!dto.DatasetFileIds.Contains(-1))
            {
                // Loops through all dataset file ids associated with the selected dataset file
                foreach (int datasetFileId in dto.DatasetFileIds)
                {
                    filters.Add(fq => fq.Terms(c => c.Field(p => p.DatasetFileId).Terms<int>(dto.DatasetFileIds)));
                }
            }

            ElasticResult<DataFlowMetric> elasticResult = _elasticDocumentClient.SearchAsync<DataFlowMetric>(s => s
                                            .Query(q => q.Bool(bq => bq.Filter(filters)))
                                            .Size(ElasticQueryValues.Size.MAX).Sort(sq => sq.Descending(dq => dq.EventMetricId))).Result;

            

            return elasticResult.Documents.ToList();
        }

        /// GetTotalFiles allows for different amounts of parameters to be passed in. 
        /// This allows for managemnet of scope for the elastic query
        private ElasticResult<DataFlowMetric> GetTotalFiles(int? datasetId = null, int? schemaId = null)
        {
            // list of query container descriptors to filter elastic search
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> Filter = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> MustNot = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> Must = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();

            // defines the aggregation filter to allow aggregation on Dataset Id
            Expression<Func<DataFlowMetric, int>> aggregation_field = o => o.DatasetId;

            // adds a filter to get files that have executionorder == maxexecutionorder
            Filter.Add(GetMaxExecutionOrderFilter());

            // Add data range must statemnet that look for jobs in a range of -19hrs from current time to 24hrs from current time
            Must.Add(GetDateRangeFilter());

            // adds a filter to ensure that files with a null run instance guid are returned
            MustNot.Add(fq => fq.Exists(t => t.Field(f => f.RunInstanceGuid)));

            // checking if filtering on datasetId is required
            if (datasetId.HasValue)
            {
                // If a datasetId is provided, override the aggregation_field group by Scehma Id instead 
                aggregation_field = o => o.SchemaId;

                Filter.Add(fq => fq.Terms(t => t.Field(f => f.DatasetId).Terms(datasetId.Value)));
            }

            // checking if filtering on schemaId is required
            if (schemaId.HasValue)
            {
                Filter.Add(fq => fq.Terms(t => t.Field(f => f.SchemaId).Terms(schemaId.Value)));
            }

            // Elastic Search Query
            ElasticResult<DataFlowMetric> elasticResult = _elasticDocumentClient.SearchAsync<DataFlowMetric>(s => s.Query(q => q.Bool(b => b
                                                                                        .Filter(Filter)
                                                                                        .MustNot(MustNot)
                                                                                        .Must(Must)))
                                                                                .Aggregations(aggregations => aggregations.Terms(FilterCategoryNames.DataFlowMetric.DOC_COUNT, df => df.Field(aggregation_field)))
                                                                                .Size(ElasticQueryValues.Size.MAX).Sort(sq => sq.Descending(dq => dq.EventMetricId))).Result;


            return elasticResult;
        }

        private ElasticResult<DataFlowMetric> GetFailedFiles(int? datasetId = null, int? schemaId = null)
        {
            // list of query container descriptors to filter elastic search
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> Filter = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> Must = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();

            // defines the aggregation filter to allow aggregation on Dataset Id
            Expression<Func<DataFlowMetric, int>> aggregation_field = o => o.DatasetId;

            // Add term must statemnet that look for jobs with a F (failed) statuscode
            Must.Add(fq => fq.Terms(t => t.Field(f => f.StatusCode).Terms("F")));

            // Add data range must statemnet that look for jobs in a range of -19hrs from current time to 24hrs from current time
            Must.Add(GetDateRangeFilter());

            // checking if filtering on datasetId is required
            if (datasetId.HasValue)
            {
                // If a datasetId is provided, override the aggregation_field group by Scehma Id instead 
                aggregation_field = o => o.SchemaId;

                Filter.Add(fq => fq.Terms(t => t.Field(f => f.DatasetId).Terms(datasetId.Value)));
            }

            // checking if filtering on schemaId is required
            if (schemaId.HasValue)
            {
                Filter.Add(fq => fq.Terms(t => t.Field(f => f.SchemaId).Terms(schemaId.Value)));
            }

            // Elastic Search Query
            ElasticResult<DataFlowMetric> elasticResult = _elasticDocumentClient.SearchAsync<DataFlowMetric>(s => s.Query(q => q.Bool(b => b
                                                                                        .Filter(Filter)
                                                                                        .Must(Must)))
                                                                                .Aggregations(aggregations => aggregations.Terms(FilterCategoryNames.DataFlowMetric.DOC_COUNT, df => df.Field(aggregation_field)))
                                                                                .Size(ElasticQueryValues.Size.MAX).Sort(sq => sq.Descending(dq => dq.EventMetricId))).Result;


            return elasticResult;
        }

        private ElasticResult<DataFlowMetric> GetInFlightFiles(int? datasetId = null, int? schemaId = null)
        {
            // list of query container descriptors to filter elastic search
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> Filter = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> MustNot = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();
            List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>> Must = new List<Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer>>();

            // defines the aggregation filter to allow aggregation on Dataset Id
            Expression<Func<DataFlowMetric, int>> aggregation_field = o => o.DatasetId;

            // adds a filter to get files that have executionorder == maxexecutionorder
            Filter.Add(GetMaxExecutionOrderFilter());

            // Add data range must statemnet that look for jobs in a range of -19hrs from current time to 24hrs from current time
            Must.Add(GetDateRangeFilter());

            // adds a filter to ensure that files with a null run instance guid are returned`
            MustNot.Add(fq => fq.Exists(t => t.Field(f => f.RunInstanceGuid)));

            // Elastic Search Query
            ElasticResult<DataFlowMetric> comepletedResults = _elasticDocumentClient.SearchAsync<DataFlowMetric>(s => s.Query(q => q.Bool(b => b
                                                                                        .Filter(Filter)
                                                                                        .MustNot(MustNot)
                                                                                        .Must(Must)))
                                                                                .Aggregations(aggregations => aggregations.Terms(FilterCategoryNames.DataFlowMetric.DOC_COUNT, df => df.Field(aggregation_field)))
                                                                                .Size(ElasticQueryValues.Size.MAX).Sort(sq => sq.Descending(dq => dq.EventMetricId))).Result;

            List<int> CompletedDatasetIds = new List<int>();

            foreach (var item in comepletedResults.Aggregations.Terms(FilterCategoryNames.DataFlowMetric.DOC_COUNT).Buckets)
            {
                int.TryParse(item.Key, out int key);

                CompletedDatasetIds.Add(key);
            }

            MustNot.Clear();

            MustNot.Add(fq => fq.Terms(t => t.Terms(CompletedDatasetIds)));

            ElasticResult<DataFlowMetric> elasticResult = _elasticDocumentClient.SearchAsync<DataFlowMetric>(s => s.Query(q => q.Bool(b => b
                                                                                        .Filter(Filter)
                                                                                        .MustNot(MustNot)))
                                                                                .Aggregations(aggregations => aggregations.Terms(FilterCategoryNames.DataFlowMetric.DOC_COUNT, df => df.Field(aggregation_field)))
                                                                                .Size(ElasticQueryValues.Size.MAX).Sort(sq => sq.Descending(dq => dq.EventMetricId))).Result;

            return elasticResult;
        }

        private Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer> GetMaxExecutionOrderFilter()
        {
            return fq => fq.Script(t => t.Script(f => f.Source("doc['executionorder'].value == doc['maxexecutionorder'].value")));
        }

        private Func<QueryContainerDescriptor<DataFlowMetric>, QueryContainer> GetDateRangeFilter(string lessThan = "19h", string greaterThan = "1d")
        {
            return fq => fq.DateRange(t => t.Field(f => f.FileCreatedDateTime)
                                                .GreaterThanOrEquals(DateMath.Now.Subtract(lessThan))
                                                .LessThan(DateMath.Now.Add(greaterThan))
                                                .Format("yyyyMMdd'T'HHmmss.SSSZ"));
        }
    }
}
