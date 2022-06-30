using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Infrastructure;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    public class DataFlowMetricController : BaseController
    {
        private readonly DataFlowMetricService _dataFlowMetricService;

        public DataFlowMetricController(DataFlowMetricService dataFlowMetricService)
        {
            _dataFlowMetricService = dataFlowMetricService;
        }
    }
}