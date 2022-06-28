using Sentry.data.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private readonly IDatasetService _datasetService;
        public AdminController(IDatasetService datasetService)
        {
            _datasetService = datasetService;
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

            return View(myDict);
        }
        public ActionResult GetAdminAction(string viewId)
        {
            string viewPath = "";
            switch (viewId)
            {
                case "1":
                    DataReprocessingModel dataReprocessingModel = new DataReprocessingModel();
                    List<DatasetDto> dtoList = _datasetService.GetAllDatasetDto();
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
                    ProcessingLogsModel processingLogsModel = new ProcessingLogsModel();
                    List<DatasetDto> dtoList2 = _datasetService.GetAllDatasetDto();
                    processingLogsModel.DatasetsList = new List<SelectListItem>();
                    foreach (DatasetDto d in dtoList2)
                    {
                        SelectListItem item = new SelectListItem();
                        item.Text = d.DatasetName;
                        item.Value = d.DatasetId.ToString();
                        processingLogsModel.DatasetsList.Add(item);
                    }
                    return PartialView("_ProcessingLogs", processingLogsModel);
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