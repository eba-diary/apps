using Nest;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
                                            .Size(ElasticQueryValues.Size.MAX).Sort(sq=>sq.Descending(dq=>dq.EventMetricId))).Result;

            return elasticResult.Documents.ToList();
        }
    }
}
