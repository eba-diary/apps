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

        public List<DataFlowMetricEntity> GetDataFlowMetricEntities(DataFlowMetricSearchDto dto)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.AddMatch<DataFlowMetricEntity>(x => x.DatasetId, dto.DatasetToSearch);
            must.AddMatch<DataFlowMetricEntity>(x => x.SchemaId, dto.SchemaToSearch);
            if(dto.FileToSearch != "-1")
            {
                must.AddMatch<DataFlowMetricEntity>(x => x.DatasetFileId, dto.FileToSearch);
            }
            BoolQuery boolQuery = new BoolQuery();
            boolQuery.Must = must;

            SearchRequest<DataFlowMetricEntity> request = new SearchRequest<DataFlowMetricEntity>()
            {
                Sort = new List<ISort>()
                {
                    new FieldSort(){Field = Infer.Field<DataFlowMetricEntity>(x => x.EventMetricId), Order = SortOrder.Descending}
                },
                Size = 10000,
                Query = boolQuery,
            };

            ElasticResult<DataFlowMetricEntity> elasticResult = _elasticContext.SearchAsync(request).Result;
            return elasticResult.Documents.ToList();
        }
    }
}
