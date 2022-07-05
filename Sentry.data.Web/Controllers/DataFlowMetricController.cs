using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class DataFlowMetricController : BaseController
    {
        private readonly DataFlowMetricService _dataFlowMetricService;
        public DataFlowMetricSearchDto searchDto;

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
        [HttpPost]
        public void GetSearchDto(DataFlowMetricSearchDto searchDtoData)
        {
            searchDto = new DataFlowMetricSearchDto();
            searchDto.DatasetToSearch = searchDtoData.DatasetToSearch;
            searchDto.SchemaToSearch = searchDtoData.SchemaToSearch;
            searchDto.FileToSearch = searchDtoData.FileToSearch;
        }
        public ActionResult GetDataFlowMetricAccordionView()
        {
            /*
            List<DataFlowMetricEntity> entityList = _dataFlowMetricService.GetDataFlowMetricEntities(searchDto);
            List<DataFlowMetricDto> metricDtoList = _dataFlowMetricService.GetMetricList(entityList);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(metricDtoList);
            DataFlowMetricAccordionModel dataFlowAccordionModel = GetDataFlowMetricAccordionModel(fileGroups);
            */
            //uncomment above and add dataFlowAccordionModel to below return statement, in current test environment, this throws expected error
            return PartialView("_DataFlowMetricAccordion");
        }
    }
}