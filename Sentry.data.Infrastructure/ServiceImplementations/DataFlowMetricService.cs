using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;

namespace Sentry.data.Infrastructure
{
    public class DataFlowMetricService: IDataFlowMetricService
    {
        private readonly IDataFlowMetricService _dataFlowMetricService;
        public DataFlowMetricService(IDataFlowMetricService dataFlowMetricService)
        {
            _dataFlowMetricService = dataFlowMetricService;
        }

    }
}
