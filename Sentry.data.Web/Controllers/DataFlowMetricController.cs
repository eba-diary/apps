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
            dataFlowAccordionModel.DataFlowMetricGroups = dtoList;
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
            DataFlowMetricEntity entity1 = new DataFlowMetricEntity();
            entity1.DatesetFileId = 1;
            entity1.FileName = "ExampleFileNameOne";
            entity1.MetricGeneratedDateTime = DateTime.Now;
            entity1.EventContents = "Woah, look at the size of this event!";
            entity1.StatusCode = "C";
            entity1.TotalFlowSteps = 5;
            entity1.CurrentFlowStep = 4;
            entity1.EventMetricId = 1;
            DataFlowMetricEntity entity2 = new DataFlowMetricEntity();
            entity2.DatesetFileId = 1;
            entity2.FileName = "ExampleFileNameOne";
            entity2.MetricGeneratedDateTime = DateTime.Now;
            entity2.EventContents = "Woah, look at the size of this event!";
            entity2.StatusCode = "C";
            entity2.TotalFlowSteps = 5;
            entity2.CurrentFlowStep = 5;
            entity2.EventMetricId = 2;
            DataFlowMetricEntity entity3 = new DataFlowMetricEntity();
            entity3.DatesetFileId = 2;
            entity3.FileName = "ExampleFileNameTwo";
            entity3.MetricGeneratedDateTime = DateTime.Now;
            entity3.EventContents = "Woah, look at the size of this event!";
            entity3.StatusCode = "F";
            entity3.TotalFlowSteps = 5;
            entity3.CurrentFlowStep = 5;
            entity3.EventMetricId = 3;
            DataFlowMetricEntity entity4 = new DataFlowMetricEntity();
            entity4.DatesetFileId = 3;
            entity4.FileName = "ExampleFileNameThree";
            entity4.MetricGeneratedDateTime = DateTime.Now;
            entity4.EventContents = "Woah, look at the size of this event!";
            entity4.StatusCode = "C";
            entity4.TotalFlowSteps = 5;
            entity4.CurrentFlowStep = 4;
            entity4.EventMetricId = 4;
            List<DataFlowMetricEntity> dataFlowMetricEntities = new List<DataFlowMetricEntity>();
            dataFlowMetricEntities.Add(entity1);
            dataFlowMetricEntities.Add(entity2);
            dataFlowMetricEntities.Add(entity3);
            dataFlowMetricEntities.Add(entity4);

            List<DataFlowMetricDto> metricDtoList = _dataFlowMetricService.GetMetricList(dataFlowMetricEntities);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(metricDtoList);
            DataFlowMetricAccordionModel dataFlowAccordionModel = GetDataFlowMetricAccordionModel(fileGroups);
            return PartialView("_DataFlowMetricAccordion", dataFlowAccordionModel);
        }
    }
}