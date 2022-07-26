using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private readonly IKafkaConnectorService _connectorService;
        private readonly IDatasetService _datasetService;
        private readonly IDeadSparkJobService _deadSparkJobService;

        public AdminController(IDatasetService datasetService, IDeadSparkJobService deadSparkJobService, IKafkaConnectorService connectorService)
        {
            _connectorService = connectorService;
            _datasetService = datasetService;
            _deadSparkJobService = deadSparkJobService;
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
            myDict.Add("2", "File Processing Logs");
            myDict.Add("3", "Parquet Null Rows");
            myDict.Add("4", "General Raw Query Parquet");
            myDict.Add("5", "Connector Status");
            myDict.Add("6", "Reprocess Dead Jobs");

            return View(myDict);
        }

        [Route("Admin/GetDeadJobs/{selectedDate?}")]
        [HttpGet]
        public ActionResult GetDeadJobs(string selectedDate)
        {
            // Convert tim
            DateTime date = DateTime.ParseExact(selectedDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            List<DeadSparkJobDto> deadSparkJobDtoList = _deadSparkJobService.GetDeadSparkJobDtos(date);

            List<DeadSparkJobModel> deadSparkJobModelList = deadSparkJobDtoList.MapToModelList();

            return PartialView("_DeadJobTable", deadSparkJobModelList);
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
                    viewPath = "_AdminTest3";
                    break;
                case "3":
                    viewPath = "_AdminTest3";
                    break;
                case "4":
                    viewPath = "_AdminTest4";
                    break;
                case "5":
                    List<ConnectorDto> connectorDtos = await _connectorService.GetS3ConnectorsDTOAsync();

                    return PartialView("_ConnectorStatus", connectorDtos.MapToModelList());
                case "6":
                    ReprocessDeadSparkJobModel reprocessDeadSparkJobModel = new ReprocessDeadSparkJobModel();
                    return PartialView("_ReprocessDeadSparkJobs", reprocessDeadSparkJobModel);
            }

            return PartialView(viewPath);
        }
    }
}