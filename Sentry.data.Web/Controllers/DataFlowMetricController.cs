using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using System.Web.Mvc;
using System.Threading;

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
        public DataFlowMetricGroupModel GetDataFlowMetricAccordionModel(List<DataFileFlowMetricsDto> dtoList)
        {
            DataFlowMetricGroupModel dataFlowMetricGroupModel = new DataFlowMetricGroupModel();
            dataFlowMetricGroupModel.DataFlowMetricGroups = dtoList;
            return dataFlowMetricGroupModel;
        }
        //below method is related to the accordion partial view, and is currently not used
        [HttpPost]
        public void GetSearchDto(DataFlowMetricSearchDto searchDtoData)
        {
            searchDto = new DataFlowMetricSearchDto();
            searchDto.DatasetToSearch = searchDtoData.DatasetToSearch;
            searchDto.SchemaToSearch = searchDtoData.SchemaToSearch;
            searchDto.FileToSearch = searchDtoData.FileToSearch;
        }
        [HttpPost]
        public JsonResult PopulateTable(DataFlowMetricSearchDto searchDto)
        {
            //delete below searchDto assignments when testing is complete
            searchDto.DatasetToSearch = "464";
            searchDto.SchemaToSearch = "1946";
            searchDto.FileToSearch = "-1";
            List<DataFlowMetric> entityList = _dataFlowMetricService.GetDataFlowMetricEntities(searchDto);
            List<DataFlowMetricDto> metricDtoList = _dataFlowMetricService.GetMetricList(entityList);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.SortFlowMetrics(_dataFlowMetricService.GetFileMetricGroups(metricDtoList));
            _dataFlowMetricService.GetFileFlowMetricsStatus(fileGroups);
            DataFlowMetricGroupModel dataFlowMetricGroupModel = GetDataFlowMetricAccordionModel(fileGroups);
            JsonResult result = Json(dataFlowMetricGroupModel.DataFlowMetricGroups, JsonRequestBehavior.AllowGet);
            return result;
        }
        //below method is related to the accordion partial view, and is currently not used
        public ActionResult GetDataFlowMetricAccordionView()
        {
            
            List<DataFlowMetric> entityList = _dataFlowMetricService.GetDataFlowMetricEntities(searchDto);
            List<DataFlowMetricDto> metricDtoList = _dataFlowMetricService.GetMetricList(entityList);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(metricDtoList);
            DataFlowMetricGroupModel dataFlowMetricGroupModel = GetDataFlowMetricAccordionModel(fileGroups);
            return PartialView("_DataFlowMetricAccordion", dataFlowMetricGroupModel);
        }
    }
}