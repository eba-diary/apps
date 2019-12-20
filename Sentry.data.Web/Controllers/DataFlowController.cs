using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.data.Web.Helpers;

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
            DataFlowModel model = new DataFlowModel();
            model.CompressionDropdown = Utility.BuildCompressionDropdown(model.IsCompressed);

            //Every dataflow requires at least one schemamap, therefore, load a default empty schemamapmodel
            SchemaMapModel schemaModel = new SchemaMapModel();
            SetSchemaModelLists(schemaModel);
            model.SchemaMaps.Add(schemaModel);

            return View("DataFlowForm", model);
        }

        [HttpPost]
        public ActionResult DataFlowForm(DataFlowModel model)
        {
            DataFlowDto dfDto = model.ToDto();

            //if (ModelState.IsValid)
            //{
            //    if (dfDto.Id == 0)
            //    {
            //        _dataFlowService.CreateDataFlow();
            //    }
            //}

            return RedirectToAction("Index");
        }

        [HttpGet]
        public PartialViewResult NewSchemaMap()
        {
            List<SelectListItem> sList = new List<SelectListItem>();
            SchemaMapModel modela = new SchemaMapModel();
            SetSchemaModelLists(modela);

            var groupedDatasets = _datasetService.GetDatasetsForQueryTool().GroupBy(x => x.DatasetCategories.First());

            sList.Add(new SelectListItem() { Text = "Select Dataset", Value = "0", Group = new SelectListGroup() { Name = "Sentry" }, Selected = true });
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

        private void SetSchemaModelLists(SchemaMapModel model)
        {
            List<SelectListItem> sList = new List<SelectListItem>();
            var groupedDatasets = _datasetService.GetDatasetsForQueryTool().GroupBy(x => x.DatasetCategories.First());

            sList.Add(new SelectListItem() { Text = "Select Dataset", Value = "0", Group = new SelectListGroup() { Name = "Sentry" }, Selected = true });
            foreach (var ds in groupedDatasets)
            {
                sList.AddRange(ds.Select(m => new SelectListItem()
                {
                    Text = m.DatasetName,
                    Value = m.DatasetId.ToString(),
                    Group = new SelectListGroup() { Name = ds.Key.Name }
                }));
            }

            model.AllDatasets = sList;
        }

        public PartialViewResult NewRetrieverJob()
        {
            JobModel model = new JobModel();

            CreateDropDownSetup(model);

            return PartialView("_RetrieverJob", model);
        }

        private void CreateDropDownSetup(JobModel model)
        {
            var temp = _dataFlowService.GetDataSourceTypes().Select(v
                => new SelectListItem { Text = v.Name, Value = v.DiscrimatorValue }).ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick a Source Type",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            model.SourceTypesDropdown = temp.Where(x =>
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_S3_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSoureDiscriminator.JAVA_APP_SOURCE &&
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_HSZ_DROP_LOCATION).OrderBy(x => x.Value);

            List<SelectListItem> temp2 = new List<SelectListItem>();

            if (model.SelectedSourceType != null && model.SelectedDataSource != 0)
            {
                temp2 = DataSourcesByType(model.SelectedSourceType, model.SelectedDataSource);
            }

            temp2.Add(new SelectListItem()
            {
                Text = "Pick a Source Type Above",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            model.SourcesForDropdown = temp2.OrderBy(x => x.Value);

            model.CompressionTypesDropdown = Enum.GetValues(typeof(CompressionTypes)).Cast<CompressionTypes>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            if (model.NewFileNameExclusionList != null)
            {
                model.FileNameExclusionList = model.NewFileNameExclusionList.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
            }
            else
            {
                model.NewFileNameExclusionList = "";
                model.FileNameExclusionList = new List<string>();
            }

            model.RequestMethodDropdown = Utility.BuildRequestMethodDropdown(model.SelectedRequestMethod);

            model.RequestDataFormatDropdown = Utility.BuildRequestDataFormatDropdown(model.SelectedRequestDataFormat);

            model.FtpPatternDropDown = Utility.BuildFtpPatternSelectList(model.FtpPattern);
        }

        private List<SelectListItem> DataSourcesByType(string sourceType, int? selectedId)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            if (selectedId != null || selectedId != 0)
            {
                output.Add(new SelectListItem() { Text = "Pick a Data Source", Value = "0" });
            }

            switch (sourceType)
            {
                case "FTP":
                    List<DataSource> fTpList = _dataFlowService.GetDataSources().Where(x => x is FtpSource).ToList();
                    output.AddRange(fTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "SFTP":
                    List<DataSource> sfTpList = _dataFlowService.GetDataSources().Where(x => x is SFtpSource).ToList();
                    output.AddRange(sfTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "DFSBasic":
                    List<DataSource> dfsBasicList = _dataFlowService.GetDataSources().Where(x => x is DfsBasic).ToList();
                    output.AddRange(dfsBasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "DFSCustom":
                    List<DataSource> dfsCustomList = _dataFlowService.GetDataSources().Where(x => x is DfsCustom).ToList();
                    output.AddRange(dfsCustomList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "S3Basic":
                    List<DataSource> s3BasicList = _dataFlowService.GetDataSources().Where(x => x is S3Basic).ToList();
                    output.AddRange(s3BasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "HTTPS":
                    List<DataSource> HttpsList = _dataFlowService.GetDataSources().Where(x => x is HTTPSSource).ToList();
                    output.AddRange(HttpsList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "GOOGLEAPI":
                    List<DataSource> GApiList = _dataFlowService.GetDataSources().Where(x => x is GoogleApiSource).ToList();
                    output.AddRange(GApiList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                default:
                    throw new NotImplementedException();
            }

            return output;
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