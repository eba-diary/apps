using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using Nest;
using Sentry.Common.Logging;
namespace Sentry.data.Infrastructure
{
    public class DataFlowMetricProvider : IDataFlowMetricProvider
    {
       private readonly IElasticContext _elasticContext;

        public DataFlowMetricProvider(IElasticContext elasticContext)
        {
            _elasticContext = elasticContext;
        }
        //returns list of data flow metrics matching searchdto criteria
        public List<DataFlowMetric> GetDataFlowMetrics(DataFlowMetricSearchDto dto)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.AddMatch<DataFlowMetric>(x => x.DatasetId, dto.DatasetId.ToString());
            must.AddMatch<DataFlowMetric>(x => x.SchemaId, dto.SchemaId.ToString());
            if(dto.DatasetFileId != -1)
            {
                must.AddMatch<DataFlowMetric>(x => x.DatasetFileId, dto.DatasetFileId.ToString());
            }
            BoolQuery boolQuery = new BoolQuery();
            boolQuery.Must = must;

            SearchRequest<DataFlowMetric> request = new SearchRequest<DataFlowMetric>()
            {
                Sort = new List<ISort>()
                {
                    new FieldSort(){Field = Infer.Field<DataFlowMetric>(x => x.EventMetricId), Order = SortOrder.Descending}
                },
                Size = 10000,
                Query = boolQuery,
            };

            ElasticResult<DataFlowMetric> elasticResult = _elasticContext.SearchAsync(request).Result;
            return elasticResult.Documents.ToList();
        }
    }
}
