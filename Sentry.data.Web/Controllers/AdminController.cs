using Sentry.data.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private IKafkaConnectorService _connectorService;

        public AdminController(IKafkaConnectorService connectorService)
        {
            _connectorService = connectorService;
        }

        [Route("Connectors")]
        public async Task<ActionResult> Connectors()
        {
            ConnectorViewModel viewModel = new ConnectorViewModel();

            List<ConnectorRootDto> connectorRootDtos = await _connectorService.GetS3ConnectorsDTO();

            viewModel.Connectors = connectorRootDtos.MapToModelList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<JObject> GetConnectorConfig(string ConnectorId)
        {
            return await _connectorService.GetS3ConnectorConfigJSON(ConnectorId);
        }

        [HttpPost]
        public async Task<ActionResult> GetConnectorStatus(string ConnectorId)
        {
            JObject JConnectorStatus = await _connectorService.GetS3ConnectorStatusJSON(ConnectorId);

            string json = JsonConvert.SerializeObject(JConnectorStatus, Formatting.Indented);

            return Content(json, "application/json");
        }

        private readonly IDatasetService _datasetService;
        public AdminController(IDatasetService datasetService)
        {
            _datasetService = datasetService;
        }

        // GET: Admin
        public async Task<ActionResult> Index()

        public ActionResult Index()
        {
            Dictionary<string, string> myDict =
            new Dictionary<string, string>();

            myDict.Add("1", "Reprocess Data Files");
            myDict.Add("2", "File Processing Logs");
            myDict.Add("3", "Parquet Null Rows");
            myDict.Add("4", "General Raw Query Parquet");

            return View(myDict);
        }
        public ActionResult GetAdminAction(string viewId)
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
                    viewPath = "_AdminTest4";
                    break;
            }

            return PartialView(viewPath);
        }
    }
}