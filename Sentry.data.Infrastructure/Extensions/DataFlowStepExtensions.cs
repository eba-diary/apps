using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public static class DataFlowMetricExtensions
    {
        public static DataFlowMetricSearchResultDto ToDto(this ElasticResult<DataFlowMetric> rootResult)
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = new DataFlowMetricSearchResultDto();

            dataFlowMetricSearchResultDto.SearchTotal = rootResult.SearchTotal;
            dataFlowMetricSearchResultDto.DataFlowMetricResults = rootResult.Documents.ToList();

            List<DataFlowMetricSearchAggregateDto> termsAggregates = new List<DataFlowMetricSearchAggregateDto>();

            foreach (var item in rootResult.Aggregations.Terms(FilterCategoryNames.DataFlowMetric.DOCCOUNT).Buckets)
            {
                int.TryParse(item.Key, out int key);

                termsAggregates.Add(new DataFlowMetricSearchAggregateDto() { key = key, docCount = (long)item.DocCount });
            }

            dataFlowMetricSearchResultDto.TermAggregates = termsAggregates;

            return dataFlowMetricSearchResultDto;
        }
    }
}
