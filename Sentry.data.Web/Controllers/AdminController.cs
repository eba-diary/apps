using Newtonsoft.Json;
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
using Sentry.data.Core.Interfaces;

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
        private readonly IDataFlowMetricService _dataFlowMetricService;

        public AdminController(IDatasetService datasetService, IDeadSparkJobService deadSparkJobService, IKafkaConnectorService connectorService, 
                                ISupportLinkService supportLinkService, IAuditService auditSerivce, IDataFeatures dataFeatures,
                                IDataFlowMetricService dataFlowMetricService)
        {
            _connectorService = connectorService;
            _datasetService = datasetService;
            _auditSerivce = auditSerivce;
            _deadSparkJobService = deadSparkJobService;
            _supportLinkService = supportLinkService;
            _dataFeatures = dataFeatures;
            _dataFlowMetricService = dataFlowMetricService;
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

        [Route("Admin/GetDeadJobsForGrid/{startDate?}/{endDate?}")]
        [HttpPost]
        public JsonResult GetDeadJobsForGrid(string startDate, string endDate)
        {
            // Convert selectedDate string to a DateTime object
            DateTime startDateTimeParsed = DateTime.ParseExact(startDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            // If the end date is empty, set to the current DateTime.
            DateTime endDateTimeParsed = endDate != "" ? DateTime.ParseExact(endDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) : DateTime.Now;

            List<DeadSparkJobDto> deadSparkJobDtoList = _deadSparkJobService.GetDeadSparkJobDtos(startDateTimeParsed, endDateTimeParsed);

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
            long totalCompletedFiles = _dataFlowMetricService.GetAllTotalFilesCount();

            // service method that return the number of inflight files
            long totalInFlightFiles = 0;

            // service method that return the number of failed files
            long totalFailedFiles = _dataFlowMetricService.GetAllFailedFilesCount();

            AdminElasticFileModel adminElasticFileModel = new AdminElasticFileModel()
            {
                TotalCompletedFiles = totalCompletedFiles,
                TotalInFlightFiles = totalInFlightFiles,
                TotalFailedFiles = totalFailedFiles,
                CLA4553_FeatureFlag = _dataFeatures.CLA4553_PlatformActivity.GetValue(),
                CLA5112_FeatureFlag = _dataFeatures.CLA5112_PlatformActivity_TotalFiles_ViewPage.GetValue(),
                CLA5260_FeatureFlag = _dataFeatures.CLA5260_PlatformActivity_FileFailures_ViewPage.GetValue()
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

        public ActionResult ProcessActivityResults(string selectedPage)
        {
            ProcessActivityModel processActivityModel = new ProcessActivityModel();

            switch (selectedPage)
            {
                case "TotalFiles":
                    processActivityModel.PageTitle = ProcessActivityType.TOTAL_FILES.GetDescription();
                    processActivityModel.ActivityType = Convert.ToInt32(ProcessActivityType.TOTAL_FILES);
                    break;
                case "TotalFailedFiles":
                    processActivityModel.PageTitle = ProcessActivityType.FAILED_FILES.GetDescription();
                    processActivityModel.ActivityType = Convert.ToInt32(ProcessActivityType.FAILED_FILES);
                    break;
                case "TotalInFlightFiles":
                    processActivityModel.PageTitle = ProcessActivityType.IN_FLIGHT_FILES.GetDescription();
                    processActivityModel.ActivityType = Convert.ToInt32(ProcessActivityType.IN_FLIGHT_FILES);
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

        [Route("Admin/GetDatasetProcessingActivityForGrid/{activityType?}")]
        [HttpPost]
        public JsonResult GetDatasetProcessingActivityForGrid(int activityType)
        {
            ProcessActivityType processActivityType = Utility.FindEnumFromId<ProcessActivityType>(activityType);

            List<DatasetProcessActivityDto> datasetProcessActivityDtos = null;

            switch (processActivityType)
            {
                case ProcessActivityType.TOTAL_FILES:
                    datasetProcessActivityDtos = _dataFlowMetricService.GetAllTotalFiles();
                    break;
                case ProcessActivityType.FAILED_FILES:
                    datasetProcessActivityDtos = _dataFlowMetricService.GetAllFailedFiles();
                    break;
                case ProcessActivityType.IN_FLIGHT_FILES:
                    break;
            }
             

            List<DatasetProcessActivityModel> datasetProcessActivityModels = datasetProcessActivityDtos.MapToModelList();

            return Json(new { data = datasetProcessActivityModels });
        }

        [Route("Admin/GetSchemaProcessingActivityForGrid/{activityType?}/{datasetId?}")]
        [HttpPost]
        public JsonResult GetSchemaProcessingActivityForGrid(int activityType, int datasetId)
        {
            ProcessActivityType processActivityType = Utility.FindEnumFromId<ProcessActivityType>(activityType);

            List<SchemaProcessActivityDto> schemaProcessActivityDtos = null;

            switch (processActivityType)
            {
                case ProcessActivityType.TOTAL_FILES:
                    schemaProcessActivityDtos = _dataFlowMetricService.GetAllTotalFilesByDataset(datasetId);
                    break;
                case ProcessActivityType.FAILED_FILES:
                    schemaProcessActivityDtos = _dataFlowMetricService.GetAllFailedFilesByDataset(datasetId);
                    break;
                case ProcessActivityType.IN_FLIGHT_FILES:
                    break;
            }


            List<SchemaProcessActivityModel> schemaProcessActivityModels = schemaProcessActivityDtos.MapToModelList();

            return Json(new { data = schemaProcessActivityModels });
        }

        [Route("Admin/GetDatasetFileProcessingActivityForGrid/{activityType?}/{schemaId?}")]
        [HttpPost]
        public JsonResult GetDatasetFileProcessingActivityForGrid(int activityType, int schemaId)
        {
            ProcessActivityType processActivityType = Utility.FindEnumFromId<ProcessActivityType>(activityType);

            List<DatasetFileProcessActivityDto> datasetFileProcessActivityDtos = null;

            switch (processActivityType)
            {
                case ProcessActivityType.TOTAL_FILES:
                    datasetFileProcessActivityDtos = _dataFlowMetricService.GetAllTotalFilesBySchema(schemaId);
                    break;
                case ProcessActivityType.FAILED_FILES:
                    datasetFileProcessActivityDtos = _dataFlowMetricService.GetAllFailedFilesBySchema(schemaId);
                    break;
                case ProcessActivityType.IN_FLIGHT_FILES:
                    break;
            }


            List<DatasetFileProcessActivityModel> datasetFileProcessActivityModels = datasetFileProcessActivityDtos.MapToModelList();

            return Json(new { data = datasetFileProcessActivityModels });
        }
    }
}