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
        public DataFlowMetricAccordionModel GetDataFlowMetricAccordionModel(List<DataFileFlowMetricsDto> dtoList)
        {
            DataFlowMetricAccordionModel dataFlowAccordionModel = new DataFlowMetricAccordionModel();
            foreach(DataFileFlowMetricsDto dto in dtoList)
            {
                dataFlowAccordionModel.FileNames.Add(dto.FileName);
                dataFlowAccordionModel.FirstEventTimes.Add(dto.LastEventTime);
                dataFlowAccordionModel.LastEventTimes.Add(dto.LastEventTime);
                dataFlowAccordionModel.Durations.Add(dto.Duration);
                dataFlowAccordionModel.FlowEventGroups.Add(dto.FlowEvents);
            }
            return dataFlowAccordionModel;
        }
    }
}