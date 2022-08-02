using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private readonly IKafkaConnectorService _connectorService;
        private readonly IDatasetService _datasetService;

        public AdminController(IKafkaConnectorService connectorService, IDatasetService datasetService)
        {
            _connectorService = connectorService;
            _datasetService = datasetService;
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

        public ActionResult Index()
        {
            Dictionary<string, string> myDict =
            new Dictionary<string, string>();

            myDict.Add("1", "Reprocess Data Files");
            myDict.Add("2", "Data Flow Metrics");
            myDict.Add("3", "Parquet Null Rows");
            myDict.Add("4", "General Raw Query Parquet");
            myDict.Add("5", "Connector Status");

            return View(myDict);
        }
        private DatasetSelectionModel GetDatasetSelectionModel()
        {
            DatasetSelectionModel model = new DatasetSelectionModel();
            List<DatasetDto> dtoList = _datasetService.GetAllActiveDatasetDto();
            dtoList.OrderByDescending(x => x.DatasetName);
            model.AllDatasets = new List<SelectListItem>();
            foreach(DatasetDto dto in dtoList)
            {
                SelectListItem item = new SelectListItem();
                item.Text = dto.DatasetName;
                item.Value = dto.DatasetId.ToString();
                model.AllDatasets.Add(item);
            }
            return model;
        }
        public async Task<ActionResult> GetAdminAction(string viewId)
        {
            string viewPath = "";
            switch (viewId)
            {
                case "1":
                    DatasetSelectionModel dataReprocessingModel = GetDatasetSelectionModel();    
                    return PartialView("_DataFileReprocessing", dataReprocessingModel);
                case "2":
                    DatasetSelectionModel flowMetricsModel = GetDatasetSelectionModel();
                    return PartialView("_DataFlowMetrics", flowMetricsModel);
                case "3":
                    viewPath = "_AdminTest3";
                    break;
                case "4":
                    viewPath = "_AdminTest4";
                    break;
                case "5":
                    List<ConnectorDto> connectorDtos = await _connectorService.GetS3ConnectorsDTOAsync();

                    return PartialView("_ConnectorStatus", connectorDtos.MapToModelList());
            }

            return PartialView(viewPath);
        }
    }
}