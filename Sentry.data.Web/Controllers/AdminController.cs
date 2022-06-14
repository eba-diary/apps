using Sentry.data.Core;
using Sentry.data.Web.Models.AdminPage;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        DataReprocessingModel dataReprocessingModel;
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
        
        public ActionResult GetAdminTest(string viewId)
        {
            string viewPath = "";
            switch (viewId)
            {
                case "1":
                    viewPath = "_DataFileReprocessing";
                    dataReprocessingModel = new DataReprocessingModel();
                    List<DatasetDto> dtoList = _datasetService.GetAllDatasetDto();
                    dataReprocessingModel.populateDatasets(dtoList);
                    dataReprocessingModel.populateDatasetIds(dtoList);
                    break;
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

            return PartialView(viewPath, dataReprocessingModel);
        }
    }
}