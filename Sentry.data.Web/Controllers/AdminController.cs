using Sentry.data.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private readonly IDatasetService _datasetService;
        private readonly IDeadSparkJobService _deadSparkJobService;
        public AdminController(IDatasetService datasetService, IDeadSparkJobService deadSparkJobService)
        {
            _datasetService = datasetService;
            _deadSparkJobService = deadSparkJobService;
        }

        // GET: Admin
        public ActionResult Index()
        {
            Dictionary<string, string> myDict =
            new Dictionary<string, string>();

            myDict.Add("1", "Reprocess Data Files");
            myDict.Add("2", "File Processing Logs");
            myDict.Add("3", "Parquet Null Rows");
            myDict.Add("4", "General Raw Query Parquet");
            myDict.Add("5", "Reprocess Dead Spark Jobs");

            return View(myDict);
        }


        [Route("Admin/ReprocessDeadSparkJobs")]
        public ActionResult ReprocessDeadSparkJobs()
        {
            List<DeadSparkJobDto> deadSparkJobDtoList = _deadSparkJobService.GetDeadSparkJobDtos(-10);

            List<DeadSparkJobModel> deadSparkJobModelList = deadSparkJobDtoList.MapToModelList();

            return View(deadSparkJobModelList);
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
                    viewPath = "_AdminTest3";
                    break;
                case "3":
                    viewPath = "_AdminTest3";
                    break;
                case "4":
                    viewPath = "_AdminTest4";
                    break;
                case "5":
                    List<DeadSparkJobDto> deadSparkJobDtoList = _deadSparkJobService.GetDeadSparkJobDtos(-10);
                    List<DeadSparkJobModel> deadSparkJobModelList = deadSparkJobDtoList.MapToModelList();
                    return PartialView("_ReprocessDeadSparkJobs", deadSparkJobModelList);
            }

            return PartialView(viewPath);
        }
    }
}