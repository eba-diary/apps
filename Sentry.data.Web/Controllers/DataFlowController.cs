using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
    public class DataFlowController : Controller
    {
        private readonly IDataFlowService _dataFlowService;
        private readonly IDatasetService _datasetService;

        public DataFlowController(IDataFlowService dataFlowService, IDatasetService datasetService)
        {
            _dataFlowService = dataFlowService;
            _datasetService = datasetService;
        }

        // GET: DataFlow
        [HttpGet]
        [Route("DataFlow")]
        public ActionResult Index()
        {
            List<DataFlowDto> dtoList = _dataFlowService.ListDataFlows();
            List<DFModel> modelList = dtoList.ToModelList();
            return View(modelList);
        }

        [HttpGet]
        [Route("DataFlow/{id}/Detail")]
        public ActionResult Detail(int id)
        {
            DataFlowDetailDto dto = _dataFlowService.GetDataFlowDetailDto(id);
            DataFlowDetailModel model = new DataFlowDetailModel(dto);

            return View(model);
        }

        [HttpGet]
        [Route("DataFlow/Search/DataFlowStep/")]
        public void DataStepSearch(string key)
        {
            List<DataFlowStepDto> stepDtoList = _dataFlowService.GetDataFlowStepDtoByTrigger("temp-file/s3drop/12/");
        }

        [HttpGet]
        [Route("DataFlow/Create/{schemaId}/")]
        public void Create(int schemaId)
        {
            bool success = _dataFlowService.CreateDataFlow(schemaId);
        }

        [HttpGet]
        public ViewResult Create()
        {
            DataFlowModel model = new DataFlowModel()
            {
                IsCompressed = true,
            };

            return View("DataFlowForm", model);

        }

        [HttpPost]
        public ActionResult DataFlowForm(DataFlowModel model)
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public PartialViewResult NewSchemaMap()
        {
            List<SelectListItem> sList = new List<SelectListItem>();

            var groupedDatasets = _datasetService.GetDatasetsForQueryTool().GroupBy(x => x.DatasetCategories.First());

            foreach (var ds in groupedDatasets)
            {
                sList.AddRange(ds.Select(m => new SelectListItem()
                {
                    Text = m.DatasetName,
                    Value = m.DatasetId.ToString(),
                    Group = new SelectListGroup() { Name = ds.Key.Name }
                }));
            }

            SchemaMapModel model = new SchemaMapModel()
            {
                AllDatasets = sList
            };

            return PartialView("_SchemaMap", model);
        }

        //[HttpGet]
        //[Route("DataFlow/{dataFlowId}/DataFlowExecution/{executionGuid}/DataFlowStep/{dataFlowStepId}/ProcessFile")]
        //public bool RunDataFlowStep(int dataFlowId, int dataFlowStepId, string key, string bucket, string executionGuid)
        //{
        //    DataFlowDetailDto flowDto = _dataFlowService.GetDataFlowDetailDto(dataFlowId);
        //    if (flowDto.steps.Any(w => w.Id == dataFlowStepId))
        //    {
        //        _dataFlowService.GenerateJobRequest(dataFlowStepId, bucket, key, executionGuid);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    return true;
        //}
    }

    public class DataFlowStepFile
    {
        public string Bucket { get; set; }
        public string Key { get; set; }
    }
}