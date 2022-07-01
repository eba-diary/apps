using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;

namespace Sentry.data.Infrastructure
{
    public class DataFlowMetricProvider : IDataFlowMetricProvider
    {
       private readonly IElasticContext _elasticContext;

        public DataFlowMetricProvider(IElasticContext elasticContext)
        {
            _elasticContext = elasticContext;
        }

        public List<DataFlowMetricEntity> GetDataFlowMetricEntities(FilterSearchDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
