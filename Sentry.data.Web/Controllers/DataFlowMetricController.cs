﻿using System;
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
        public JsonResult PopulateTable(DataFlowMetricSearchDto searchDto)
        {
            //delete below searchDto assignments when testing is complete
            searchDto.DatasetToSearch = "464";
            searchDto.SchemaToSearch = "1946";
            searchDto.FileToSearch = "-1";
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFlowMetricGroupModel dataFlowMetricGroupModel = new DataFlowMetricGroupModel();
            dataFlowMetricGroupModel.DataFlowMetricGroups = fileGroups;
            JsonResult result = Json(dataFlowMetricGroupModel.DataFlowMetricGroups, JsonRequestBehavior.AllowGet);
            return result;
        }
    }
}