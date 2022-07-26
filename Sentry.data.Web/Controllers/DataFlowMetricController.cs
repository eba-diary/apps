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
        public DataFlowMetricController(DataFlowMetricService dataFlowMetricService)
        {
            _dataFlowMetricService = dataFlowMetricService;
        }

        [HttpPost]
        public JsonResult PopulateTable(DataFlowMetricSearchModel searchModel)
        {
            searchModel.DatasetId = 464;
            searchModel.SchemaId = 1946;
            searchModel.DatasetFileId = -1;
            DataFlowMetricSearchDto searchDto = DataFlowMetricExtensions.ToDto(searchModel);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(searchDto);
            List<DataFlowMetricGroupModel> dataFlowMetricGroupModels = DataFlowMetricExtensions.ToModels(fileGroups);
            JsonResult result = Json(dataFlowMetricGroupModels, JsonRequestBehavior.AllowGet);
            return result;
        }
    }
}