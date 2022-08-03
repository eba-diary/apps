using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private readonly IKafkaConnectorService _connectorService;
        private readonly IDatasetService _datasetService;
        private readonly IAuditService _auditSerivce;

        public AdminController(IKafkaConnectorService connectorService, IDatasetService datasetService, IAuditService auditSerivce)
        {
            _connectorService = connectorService;
            _datasetService = datasetService;
            _auditSerivce = auditSerivce;
        }

        [HttpPost]
        public async Task<JObject> GetConnectorConfig(string ConnectorId)
        {
            return await _connectorService.GetS3ConnectorConfigJSONAsync(ConnectorId);
        }

        [HttpPost]
        public async Task<ActionResult> GetConnectorStatus(string ConnectorId)
        {
            JObject JConnectorStatus = await _connectorService.GetS3ConnectorStatusJSONAsync(ConnectorId);

            string json = JsonConvert.SerializeObject(JConnectorStatus, Formatting.Indented);

            return Content(json, "application/json");
        }

        [HttpPost]
        public ActionResult GetAuditTableResults(int datasetId, int schemaId, int auditId)
        {
            AuditType auditType = Utility.FindEnumFromId<AuditType>(auditId);

            BaseAuditModel tableModel = new BaseAuditModel();

            tableModel.DataFlowStepId = 1;

            string viewPath = "";

            switch (auditType)
            {
                case AuditType.NonParquetFiles:
                    tableModel = _auditSerivce.GetExceptRows(datasetId, schemaId).MapToModel();
                    viewPath = "_NonParquetFilesTable";
                    break;
                case AuditType.RowCountCompare:
                    tableModel = _auditSerivce.GetRowCountCompare(datasetId, schemaId).MapToCompareModel();
                    viewPath = "_RowCountCompareTable";
                    break;
            } 

            return PartialView(viewPath, tableModel);
        }

        public AuditSelectionModel GetAuditSelectionModel()
        {
            AuditSelectionModel model = new AuditSelectionModel(); 

            // Define specific AuditType enum id's that will have added search features
            model.AuditAddedSearchKey = new int[]{ 0,1 };

            List<DatasetDto> dtoTestList = _datasetService.GetAllDatasetDto();

            model.AllDatasets = new List<SelectListItem>();

            dtoTestList.ForEach(d => model.AllDatasets.Add(new SelectListItem { Text = d.DatasetName, Value = d.DatasetId.ToString() }));

            model.AllAuditTypes = Utility.BuildSelectListFromEnum<AuditType>(0);
            model.AllAuditSearchTypes = Utility.BuildSelectListFromEnum<AuditSearchType>(0);

            return model;
        }

        public ActionResult Index()
        {
            Dictionary<string, string> myDict =
            new Dictionary<string, string>();

            myDict.Add("1", "Reprocess Data Files");
            myDict.Add("2", "File Processing Logs");
            myDict.Add("3", "Parquet Null Rows");
            myDict.Add("4", "General Raw Query Parquet");
            myDict.Add("5", "Connector Status");

            return View(myDict);
        }
        public async Task<ActionResult> GetAdminAction(string viewId)
        {
            string viewPath = "";
            switch (viewId)
            {
                case "1":
                    List<DatasetDto> dtoList = _datasetService.GetAllDatasetDto();
                    DataReprocessingModel dataReprocessingModel = new DataReprocessingModel();
                    dataReprocessingModel.AllDatasets = new List<SelectListItem>();
                    foreach(DatasetDto d in dtoList)
                    {
                        SelectListItem item = new SelectListItem();
                        item.Text = d.DatasetName;
                        item.Value = d.DatasetId.ToString();
                        dataReprocessingModel.AllDatasets.Add(item);
                    }
                    return PartialView("_DataFileReprocessing", dataReprocessingModel);
                case "2":
                    viewPath = "_AdminTest2";
                    break;
                case "3":
                    viewPath = "_AdminTest3";
                    break;
                case "4":
                    AuditSelectionModel auditSelectionModel = GetAuditSelectionModel();
                    return PartialView("_RawqueryParquetAudit", auditSelectionModel);
                case "5":
                    List<ConnectorDto> connectorDtos = await _connectorService.GetS3ConnectorsDTOAsync();

                    return PartialView("_ConnectorStatus", connectorDtos.MapToModelList());
            }

            return PartialView(viewPath);
        }
    }
}