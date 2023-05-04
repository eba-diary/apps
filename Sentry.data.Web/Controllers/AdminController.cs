﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Admin;
using Sentry.data.Web.Extensions;
using Sentry.data.Web.Helpers;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Sentry.data.Web;
using Sentry.data.Web.Models.AdminPage;


namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
    public class AdminController : BaseController
    {
        private readonly IKafkaConnectorService _connectorService;
        private readonly IDatasetService _datasetService;
        private readonly IAuditService _auditSerivce;
        private readonly IDeadSparkJobService _deadSparkJobService;
        private readonly ISupportLinkService _supportLinkService;
        private readonly IDataFeatures _dataFeatures;

        public AdminController(IDatasetService datasetService, IDeadSparkJobService deadSparkJobService, IKafkaConnectorService connectorService, ISupportLinkService supportLinkService, IAuditService auditSerivce, IDataFeatures dataFeatures)
        {
            _connectorService = connectorService;
            _datasetService = datasetService;
            _auditSerivce = auditSerivce;
            _deadSparkJobService = deadSparkJobService;
            _supportLinkService = supportLinkService;
            _dataFeatures = dataFeatures;
        }

      
        [HttpPost]
        public async Task<JObject> GetConnectorConfig(string ConnectorId)
        {
            return await _connectorService.GetS3ConnectorConfigJSONAsync(ConnectorId);
        }

        [System.Web.Http.HttpPost]
        public async Task<ActionResult> GetConnectorStatus(string ConnectorId)
        {
            JObject JConnectorStatus = await _connectorService.GetS3ConnectorStatusJSONAsync(ConnectorId);

            string json = JsonConvert.SerializeObject(JConnectorStatus, Formatting.Indented);

            return Content(json, "application/json");
        }

        [HttpPost]
        public ActionResult GetAuditTableResults(int datasetId, int schemaId, int auditId, int searchTypeId, string searchParameter)
        {
            // find audit and search enum types 
            AuditType auditType = Utility.FindEnumFromId<AuditType>(auditId);

            AuditSearchType auditSearchType = Utility.FindEnumFromId<AuditSearchType>(searchTypeId);

            BaseAuditModel tableModel = new BaseAuditModel();
            
            searchParameter = searchParameter == "All Files" ? "%" : searchParameter;

            string viewPath = "";
            try
            {
                switch (auditType)
                {
                    case AuditType.NonParquetFiles:
                        tableModel = _auditSerivce.GetNonParquetFiles(datasetId, schemaId, searchParameter, auditSearchType).MapToModel();
                        viewPath = "_NonParquetFilesTable";
                        break;
                    case AuditType.RowCountCompare:
                        tableModel = _auditSerivce.GetComparedRowCount(datasetId, schemaId, searchParameter, auditSearchType).MapToModel();
                        viewPath = "_RowCountCompareTable";
                        break;
                } 

                return PartialView(viewPath, tableModel);
            } 
            catch (ArgumentException ex)
            {
                return this.Json( new { failure = true, message = ex.Message });
            }
        }

        public AuditSelectionModel GetAuditSelectionModel()
        {
            AuditSelectionModel model = new AuditSelectionModel(); 

            // Define specific AuditType enum id's that will have added search features
            model.AuditAddedSearchKey = new int[]{ 0,1 };

            List<DatasetSchemaDto> datasetDtoList = _datasetService.GetAllDatasetDto();

            model.AllDatasets = new List<SelectListItem>();

            model.AllDatasets.Add(new SelectListItem() { Disabled=true, Text = "Please select Dataset", Value="-1"});
            datasetDtoList.ForEach(d => model.AllDatasets.Add(new SelectListItem { Text = d.DatasetName, Value = d.DatasetId.ToString() }));

            model.AllAuditTypes = Utility.BuildSelectListFromEnum<AuditType>(0);
            model.AllAuditTypes.Insert(0, new SelectListItem() { Disabled = true, Text = "Please select Audit Type", Value = "-1" });

            model.AllAuditSearchTypes = Utility.BuildSelectListFromEnum<AuditSearchType>(0);
            model.AllAuditSearchTypes.Insert(0, new SelectListItem() { Disabled = true, Text = "Please select a Search Type", Value = "-1" });

            return model;
        }

        [Route("Admin/DeadJobTable")]
        [HttpGet]
        public ActionResult DeadJobTable()
        {
            return PartialView("_DeadJobTable");
        }

        [Route("Admin/GetDeadJobsForGrid/{selectedDate?}")]
        [HttpPost]
        public JsonResult GetDeadJobsForGrid(string selectedDate)
        {
            // Convert selectedDate string to a DateTime object
            DateTime date = DateTime.ParseExact(selectedDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            List<DeadSparkJobDto> deadSparkJobDtoList = _deadSparkJobService.GetDeadSparkJobDtos(date);

            List<DeadSparkJobModel> deadSparkJobModelList = deadSparkJobDtoList.MapToModelList();

            return Json(new { data = deadSparkJobModelList } );
        }

        //method for generating a dataset selection model, which contains a list of all Active datasets
        private DatasetSelectionModel GetDatasetSelectionModel()
        {
            DatasetSelectionModel model = new DatasetSelectionModel();
            List<DatasetSchemaDto> dtoList = _datasetService.GetAllActiveDatasetDto();
            dtoList = dtoList.OrderBy(x => x.DatasetName).ToList();
            model.AllDatasets = new List<SelectListItem>();

            model.AllDatasets.Add(new SelectListItem()
            {
                Value = "0",
                Text = "Please select a dataset...",
                Selected = true
            });


            foreach (DatasetSchemaDto dto in dtoList)
            {
                SelectListItem item = new SelectListItem();
                item.Text = dto.DatasetName;
                item.Value = dto.DatasetId.ToString();
                item.Selected = false;
                model.AllDatasets.Add(item);
            }


            return model;
        }

        [HttpGet]
        public ActionResult CreateSupportLink()
        {
            SupportLinkModel supportLinkModel = new SupportLinkModel();
            return PartialView("_SupportLinkForm", supportLinkModel);
        }


        // adds a support link to the link farm
        [HttpPost]
        public ActionResult AddSupportLink(SupportLinkModel supportLinkModel)
        {
            if (supportLinkModel.Name == null || supportLinkModel.Url == null)
            {
                if (supportLinkModel.Name == null)
                {
                    return Content(System.Net.HttpStatusCode.BadRequest.ToString(), "Name was not submitted");
                }
                if (supportLinkModel.Url == null)
                {
                    return Content(System.Net.HttpStatusCode.BadRequest.ToString(), "Url was not submitted");
                }
            }

            SupportLinkDto supportLinkDto = supportLinkModel.ToDto();

            try
            {
                _supportLinkService.AddSupportLink(supportLinkDto);
                return Json(new { redirectToUrl = Url.Action("SupportLinks", "Admin") });
            }
            catch (Exception)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError.ToString(), "Adding Support Link failed");
            }

            return Content(System.Net.HttpStatusCode.OK.ToString(), "Support Link was successfully added to database");
        }

        // removes a support link from the link farm
        [HttpPost]
        public ActionResult RemoveSupportLink(SupportLinkModel supportLinkModel)
        {
            try
            {
                _supportLinkService.RemoveSupportLink(supportLinkModel.SupportLinkId);
                return Json(new { redirectToUrl = Url.Action("SupportLinks", "Admin") });
            }
            catch (Exception)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError.ToString(), "Removing Support Link failed");
            }
        }

        //below methods all return admin page views
        public ActionResult Index()
        {
            // service method that returns the number of completed files
            int totalCompletedFiles = 1051;

            // service method that return the number of inflight files
            int totalInFlightFiles = 306;

            // service method that return the number of failed files
            int totalFailedFiles = 2;

            // boolean variable that captures feature flag
            bool CLA4553_featureFlag = _dataFeatures.CLA4553_PlatformActivity.GetValue();
            bool CLA5112_featureFlag = _dataFeatures.CLA5112_PlatformActivity_TotalFiles_ViewPage.GetValue();

            AdminElasticFileModel adminElasticFileModel = new AdminElasticFileModel()
            {
                CompletedFiles = totalCompletedFiles,
                InFlightFiles = totalInFlightFiles,
                FailedFiles = totalFailedFiles,
                CLA4553_FeatureFlag = CLA4553_featureFlag,
                CLA5112_FeatureFlag = CLA5112_featureFlag
            };

            return View(adminElasticFileModel);
        }

        public ActionResult DataFileReprocessing()
        {
            DatasetSelectionModel dataReprocessingModel = GetDatasetSelectionModel();
           
            return View(dataReprocessingModel);
        }
        public ActionResult DataFlowMetrics()
        {
            DatasetSelectionModel flowMetricsModel = GetDatasetSelectionModel();
            return View(flowMetricsModel);
        }

        public async Task<ActionResult> ConnectorStatus()
        {
            List<ConnectorDto> connectorDtos = await _connectorService.GetS3ConnectorsDTOAsync();
            return View(connectorDtos.MapToModelList());
        }

        public ActionResult ReprocessDeadSparkJobs()
        {
            ReprocessDeadSparkJobModel reprocessDeadSparkJobModel = new ReprocessDeadSparkJobModel();
            return View(reprocessDeadSparkJobModel);
        }

        [HttpGet]
        public ActionResult SupportLinks()
        {
            List<SupportLinkDto> supportLinkDtos = _supportLinkService.GetSupportLinks();
            List<SupportLinkModel> supportLinks = new List<SupportLinkModel>();
            foreach(SupportLinkDto link in supportLinkDtos)
            {
                supportLinks.Add(link.ToModel());
            }
            SupportLinkWrapper supportLinkWrapper = new SupportLinkWrapper()
            {
                AllSupportLinks = supportLinks,
            };
            return View(supportLinkWrapper);
        }
       
        public ActionResult RawqueryParquetAudit()
        {
            AuditSelectionModel model = GetAuditSelectionModel();
            return View(model);
        }

        public ActionResult ProcessActivityResults(string status)
        {
            ProcessActivityModel processActivityModel = new ProcessActivityModel();

            switch (status)
            {
                case "TotalFiles":
                    processActivityModel.PageTitle = "Total Files";
                    processActivityModel.TitleId = "TotalFiles";
                    break;
                case "FailedFiles":
                    processActivityModel.PageTitle = "Failed Files";
                    processActivityModel.TitleId = "FailedFiles";
                    break;
                case "InFlightFiles":
                    processActivityModel.PageTitle = "In Flight Files";
                    processActivityModel.TitleId = "InFlightFiles";
                    break;
            }

            return View(processActivityModel);
        }

        [Route("Admin/GetProcessActivityTable")]
        [HttpGet]
        public ActionResult GetProcessActivityTable()
        {
            return PartialView("_ProcessActivityResultsTable");
        }

        [Route("Admin/GetDatasetProcessingActivityForGrid/{status?}")]
        [HttpPost]
        public JsonResult GetDatasetProcessingActivityForGrid(string status)
        {
            List<DatasetProcessActivityModel> processActivityResultsModels = new List<DatasetProcessActivityModel>() { 
                new DatasetProcessActivityModel() { Dataset="Dataset",  DatasetId = 1, FileCount=1,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset2", DatasetId = 2, FileCount=3,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset3", DatasetId = 3, FileCount=4,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset4", DatasetId = 4, FileCount=6,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset5", DatasetId = 5, FileCount=8,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset6", DatasetId = 6, FileCount=32, RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset7", DatasetId = 7, FileCount=2,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset8", DatasetId = 8, FileCount=6,  RecentRun = DateTime.Now},
                new DatasetProcessActivityModel() { Dataset="Dataset9", DatasetId = 9, FileCount=1,  RecentRun = DateTime.Now}
            };

            return Json(new { data = processActivityResultsModels });
        }

        [Route("Admin/GetSchemaProcessingActivityForGrid/{status?}/{datasetId?}")]
        [HttpPost]
        public JsonResult GetSchemaProcessingActivityForGrid(string status, int datasetId)
        {
            List<SchemaProcessActivityModel> processActivityResultsModels = new List<SchemaProcessActivityModel>() {
                new SchemaProcessActivityModel() { Schema="Schema4", SchemaId = 4, DatasetId = datasetId, FileCount=1, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema",  SchemaId = 4, DatasetId = datasetId, FileCount=3, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema1", SchemaId = 4, DatasetId = datasetId, FileCount=5, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema",  SchemaId = 4, DatasetId = datasetId, FileCount=1, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema",  SchemaId = 4, DatasetId = datasetId, FileCount=1, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema",  SchemaId = 4, DatasetId = datasetId, FileCount=1, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema3", SchemaId = 4, DatasetId = datasetId, FileCount=8, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema",  SchemaId = 4, DatasetId = datasetId, FileCount=8, RecentRun = DateTime.Now},
                new SchemaProcessActivityModel() { Schema="Schema5", SchemaId = 4, DatasetId = datasetId, FileCount=0, RecentRun = DateTime.Now}
            };


            return Json(new { data = processActivityResultsModels });
        }

        [Route("Admin/GetDatasetFileProcessingActivityForGrid/{status?}/{schemaId?}/{datasetId?}")]
        [HttpPost]
        public JsonResult GetDatasetFileProcessingActivityForGrid(string status, int schemaId, int datasetId)
        {
            List<DatasetFileProcessActivityModel> processActivityResultsModels = new List<DatasetFileProcessActivityModel>() {
                new DatasetFileProcessActivityModel() { FileName="DatasetFile4", FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now},
                new DatasetFileProcessActivityModel() { FileName="DatasetFile",  FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now},
                new DatasetFileProcessActivityModel() { FileName="DatasetFile8", FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now},
                new DatasetFileProcessActivityModel() { FileName="DatasetFile7", FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now},
                new DatasetFileProcessActivityModel() { FileName="DatasetFile5", FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now},
                new DatasetFileProcessActivityModel() { FileName="DatasetFile1", FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now},
                new DatasetFileProcessActivityModel() { FileName="DatasetFile2", FlowExecutionGuid="20230502105411", LastFlowStep = "ConvertToParquet", LastEventTime = DateTime.Now}
            };


            return Json(new { data = processActivityResultsModels });
        }
    }
}