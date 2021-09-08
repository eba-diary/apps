using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core.Interfaces;
using System.Threading.Tasks;
using Sentry.data.Core.Entities;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_VIEW)]
    public class DataFlowController : BaseController
    {
        private readonly IDataFlowService _dataFlowService;
        private readonly IDatasetService _datasetService;
        private readonly IConfigService _configService;
        private readonly ISecurityService _securityService;
        private readonly ISAIDService _saidService;

        public DataFlowController(IDataFlowService dataFlowService, IDatasetService datasetService, IConfigService configService,
            ISecurityService securityService, ISAIDService saidService)
        {
            _dataFlowService = dataFlowService;
            _datasetService = datasetService;
            _configService = configService;
            _securityService = securityService;
            _saidService = saidService;
        }

        // GET: DataFlow
        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
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

            model.UserSecurity = _securityService.GetUserSecurity(null, SharedContext.CurrentUser);

            return View(model);
        }

        [HttpGet]
        [Route("DataFlow/Search/DataFlowStep/")]
        public void DataStepSearch(string key)
        {
            List<DataFlowStepDto> stepDtoList = _dataFlowService.GetDataFlowStepDtoByTrigger("temp-file/s3drop/12/");
        }

        [HttpGet]
        public async Task<ViewResult> Create()
        {

            UserSecurity us = _securityService.GetUserSecurity(null, SharedContext.CurrentUser);

            if (!us.CanCreateDataFlow)
            {
                return View("Forbidden");
            }

            DataFlowModel model = new DataFlowModel();
            model.CompressionDropdown = Utility.BuildCompressionDropdown(model.IsCompressed);
            model.PreProcessingRequiredDropdown = Utility.BuildPreProcessingDropdown(model.IsPreProcessingRequired);
            model.PreProcessingOptionsDropdown = Utility.BuildPreProcessingOptionsDropdown(model.PreprocessingOptions);

            CreateDropDownSetup(model.RetrieverJob);
            //Every dataflow requires at least one schemamap, therefore, load a default empty schemamapmodel
            SchemaMapModel schemaModel = new SchemaMapModel
            {
                SelectedDataset = 0
            };
            model.SchemaMaps.Add(schemaModel);
            model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode).ConfigureAwait(false);

            var namedEnvironments = await BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment).ConfigureAwait(false);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType),namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            return View("DataFlowForm", model);

        }

        [HttpPost]
        public async Task<ActionResult> DataFlowForm(DataFlowModel model)
        {
            AddCoreValidationExceptionsToModel(model.Validate());

            DataFlowDto dfDto = model.ToDto();

            AddCoreValidationExceptionsToModel(await _dataFlowService.Validate(dfDto).ConfigureAwait(true));

            try
            {
                if (ModelState.IsValid)
                {
                    int newFlowId = 0;

                    if (dfDto.Id == 0)
                    {
                        newFlowId = _dataFlowService.CreateandSaveDataFlow(dfDto);
                        if (newFlowId != 0)
                        {
                            return RedirectToAction("Detail", "DataFlow", new { id = newFlowId });
                        }
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (DatasetUnauthorizedAccessException dsEx)
            {
                //User does not have access to push data to one or more mapped dataset schemas
                ValidationResults results = new ValidationResults();
                results.Add(dsEx.Message);
                AddCoreValidationExceptionsToModel(new ValidationException(results));
            }
            catch (DataFlowUnauthorizedAccessException)
            {
                //User should not get to this point via UI since navigating to Create page should give them Forbidden error
                // However, just in case we are going to throw the forbidden here as well.
                return View("Forbidden");
            }
            catch (ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }

            model.CompressionDropdown = Utility.BuildCompressionDropdown(model.IsCompressed);
            model.PreProcessingRequiredDropdown = Utility.BuildPreProcessingDropdown(model.IsPreProcessingRequired);
            model.PreProcessingOptionsDropdown = Utility.BuildPreProcessingOptionsDropdown(model.PreprocessingOptions);
            if (model.RetrieverJob != null)
            {
                CreateDropDownSetup(model.RetrieverJob);
            }
            if (model.SchemaMaps != null && model.SchemaMaps.Count > 0)
            {
                foreach (SchemaMapModel mapModel in model.SchemaMaps)
                {
                    SetSchemaModelLists(mapModel);
                }
            }
            model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode).ConfigureAwait(false);

            var namedEnvironments = await BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment).ConfigureAwait(false);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList; 
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            return View("DataFlowForm", model);
        }

        /// <summary>
        /// THis method utilizes a wrapper partial view to ensure the new schemamap is
        ///   added to the collection correctly using EditorForMany helper.  The EditorForMany
        ///   adds Guid (Index field) to each model, therefore, validations can be traked across
        ///   a collection of models.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DataFlow/NewSchemaMap/")]
        public PartialViewResult _AjaxMakeSchemaMap()
        {
            DataFlowModel dfModel = new DataFlowModel();
            SchemaMapModel schemaModel = new SchemaMapModel
            {
                SelectedDataset = 0
            };
            dfModel.SchemaMaps.Add(schemaModel);

            return PartialView(dfModel);
        }

        [HttpGet]
        [Route("DataFlow/NewCompressionJob/")]
        public PartialViewResult NewCompressionJob()
        {
            CompressionModel model = new CompressionModel();

            CreateDropDownList(model);

            return PartialView("_CompressionJob", model);

        }

        private void SetSchemaModelLists(SchemaMapModel model)
        {
            List<SelectListItem> dsList = new List<SelectListItem>();
            List<SelectListItem> scmList = new List<SelectListItem>();
            var groupedDatasets = _datasetService.GetDatasetsForQueryTool().GroupBy(x => x.DatasetCategories.First());

            if (model.SelectedDataset == 0)
            {
                dsList.Add(new SelectListItem() { Text = "Create New Dataset", Value = "-1", Selected = true });
                dsList.Add(new SelectListItem() { Text = "Select Dataset", Value = "0", Selected = true });
            }

            foreach (var group in groupedDatasets)
            {
                SelectListGroup curGroup = new SelectListGroup()
                {
                    Name = group.Key.Name
                };

                dsList.AddRange(group.OrderBy(o => o.DatasetName).Select(m => new SelectListItem()
                {
                    Text = m.DatasetName,
                    Value = m.DatasetId.ToString(),
                    Group = curGroup,
                    Selected = (m.DatasetId == model.SelectedDataset)
                }
                ));
            }

            model.AllDatasets = dsList;

            if (model.SelectedDataset > 0 && model.SelectedSchema == 0)
            {
                scmList.Add(new SelectListItem() { Text = "Select Schema", Value = "0", Selected = true });
            }

            if (model.SelectedDataset > 0)
            {
                var datasetSchemaList = _configService.GetDatasetFileConfigDtoByDataset(model.SelectedDataset).Where(w => !w.DeleteInd).OrderBy(o => o.Name);

                foreach (var scm in datasetSchemaList)
                {
                    scmList.Add(new SelectListItem()
                    {
                        Text = scm.Name,
                        Value = scm.Schema.SchemaId.ToString(),
                        Selected = (scm.Schema.SchemaId == model.SelectedSchema)
                    });
                }

                model.AllSchemas = scmList;
            }
        }

        public PartialViewResult NewRetrieverJob(JobModel model)
        {
            CreateDropDownSetup(model);

            return PartialView("_RetrieverJob", model);
        }

        public PartialViewResult _SchemaMapDetail(int dataflowId)
        {
            List<SchemaMapDetailDto> dtoList = _dataFlowService.GetMappedSchemaByDataFlow(dataflowId);
            List<SchemaMapDetailModel> modelList = dtoList.ToDetailModelList();
            return PartialView("~/Views/Dataflow/_SchemaMapDetail.cshtml", modelList);
        }

        [HttpGet]
        [Route("DataFlow/NamedEnvironment")]
        public async Task<PartialViewResult> _NamedEnvironment(string assetKeyCode, string namedEnvironment)
        {
            DataFlowModel model = new DataFlowModel()
            {
                SAIDAssetKeyCode = assetKeyCode,
                NamedEnvironment = namedEnvironment
            };

            var namedEnvironments = await BuildNamedEnvironmentDropDowns(assetKeyCode, namedEnvironment).ConfigureAwait(false);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            return PartialView(model);
        }

        private void CreateDropDownSetup(JobModel model)
        {
            var temp = _dataFlowService.GetDataSourceTypes()
                .Where(w => w.DiscrimatorValue == GlobalConstants.DataSoureDiscriminator.FTP_SOURCE ||
                    w.DiscrimatorValue == GlobalConstants.DataSoureDiscriminator.GOOGLE_API_SOURCE ||
                    w.DiscrimatorValue == GlobalConstants.DataSoureDiscriminator.HTTPS_SOURCE)
                .Select(v => new SelectListItem { Text = v.Name, Value = v.DiscrimatorValue })
                .ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick a Source Type",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            model.SourceTypesDropdown = temp.OrderBy(o => o.Value);

            List<SelectListItem> temp2 = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(model.SelectedSourceType) && model.SelectedDataSource != "0")
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

            model.RequestMethodDropdown = Utility.BuildRequestMethodDropdown(model.SelectedRequestMethod);

            model.RequestDataFormatDropdown = Utility.BuildRequestDataFormatDropdown(model.SelectedRequestDataFormat);

            model.FtpPatternDropDown = Utility.BuildFtpPatternSelectList(model.FtpPattern);

            int s;
            int pickerval;
            if (int.TryParse(model.SchedulePicker, out s))
            {
                pickerval = s;
            }
            else
            {
                pickerval = 0;
            }
            model.SchedulePickerDropdown = Utility.BuildSchedulePickerDropdown(((RetrieverJobScheduleTypes)pickerval).GetDescription());
        }

        private void CreateDropDownList(CompressionModel model)
        {
            model.CompressionTypesDropdown = Enum.GetValues(typeof(CompressionTypes)).Cast<CompressionTypes>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
        }

        private async Task<List<SelectListItem>> BuildSAIDAssetDropDown(string keyCode)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            //SAIDAsset asset = await _saidService.GetAssetByKeyCode("DATA").ConfigureAwait(false);
            List<SAIDAsset> assetList = await _saidService.GetAllAssets().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(keyCode) || !assetList.Any(a => a.SaidKeyCode == keyCode))
            {
                output.Add(new SelectListItem
                {
                    Value = "None",
                    Text = "Select Asset",
                    Selected = true,
                    Disabled = true
                });
            }

            //Filtering out assets not assigned a SaidKeyCode
            foreach (SAIDAsset asset in assetList.Where(w => !string.IsNullOrWhiteSpace(w.SaidKeyCode)).OrderBy(o => o.Name))
            {
                output.Add(new SelectListItem
                {
                    Value = asset.SaidKeyCode,
                    Text = $"{asset.Name} ({asset.SaidKeyCode})",
                    Selected = (!string.IsNullOrWhiteSpace(keyCode) && asset.SaidKeyCode == keyCode)
                });
            }

            return output;
        }

        private async Task<(List<SelectListItem> namedEnvironmentList, List<SelectListItem> namedEnvironmentTypeList)> BuildNamedEnvironmentDropDowns(string keyCode, string namedEnvironment)
        {
            //if no keyCode has been selected yet, skip the call to Quartermaster
            List<NamedEnvironmentDto> qNamedEnvironmentList = new List<NamedEnvironmentDto>();
            if (!string.IsNullOrWhiteSpace(keyCode))
            {
                qNamedEnvironmentList = await _dataFlowService.GetNamedEnvironmentsAsync(keyCode).ConfigureAwait(true);
            }

            List<SelectListItem> namedEnvironmentList = BuildNamedEnvironmentDropDown(namedEnvironment, qNamedEnvironmentList);

            List<SelectListItem> namedEnvironmentTypeList = BuildNamedEnvironmentTypeDropDown(namedEnvironment, qNamedEnvironmentList);

            return (namedEnvironmentList, namedEnvironmentTypeList);
        }

        private static List<SelectListItem> BuildNamedEnvironmentDropDown(string namedEnvironment, List<NamedEnvironmentDto> qNamedEnvironmentList)
        {
            //convert the list of Quartermaster environments into SelectListItems
            return qNamedEnvironmentList.Select(env => new SelectListItem()
            {
                Value = env.NamedEnvironment,
                Text = env.NamedEnvironment,
                Selected = (!string.IsNullOrWhiteSpace(namedEnvironment) && env.NamedEnvironment == namedEnvironment)
            }).ToList();
        }

        private static List<SelectListItem> BuildNamedEnvironmentTypeDropDown(string namedEnvironment, List<NamedEnvironmentDto> qNamedEnvironmentList)
        {
            //figure out the correct NamedEnvironmentType for the selected NamedEnvironment
            string namedEnvironmentType = NamedEnvironmentType.NonProd.ToString();

            //if an Environment Type filter is configured, create the filter and default to that environment type
            var environmentTypeFilter = Configuration.Config.GetHostSetting("QuartermasterNamedEnvironmentTypeFilter");
            Func<string, bool> filter = envType => true;
            if (!string.IsNullOrWhiteSpace(environmentTypeFilter))
            {
                filter = envType => envType == environmentTypeFilter;
                namedEnvironmentType = environmentTypeFilter;
            }

            //if there are named environments, select the correct namedEnvironmentType for the chosen environment
            //(the DataFlowService will already have filtered them down to only the appropriate namedEnvironmentTypes)
            if (qNamedEnvironmentList.Any())
            {
                if (string.IsNullOrWhiteSpace(namedEnvironment))
                {
                    namedEnvironmentType = qNamedEnvironmentList.First().NamedEnvironmentType.ToString();
                }
                else if (qNamedEnvironmentList.Any(e => e.NamedEnvironment == namedEnvironment))
                {
                    namedEnvironmentType = qNamedEnvironmentList.First(e => e.NamedEnvironment == namedEnvironment).NamedEnvironmentType.ToString();
                }
            }

            //convert the list of named environment types into SelectListLitems
            var namedEnvironmentTypeList = Enum.GetNames(typeof(NamedEnvironmentType)).Where(filter).Select(env => new SelectListItem()
            {
                Value = env,
                Text = env,
                Selected = namedEnvironmentType == env
            }).ToList();

            return namedEnvironmentTypeList;
        }

        private List<SelectListItem> DataSourcesByType(string sourceType, string selectedId)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(selectedId) || selectedId != "0")
            {
                output.Add(new SelectListItem() { Text = "Pick a Data Source", Value = "0" });
            }

            switch (sourceType)
            {
                case "FTP":
                    List<DataSource> fTpList = _dataFlowService.GetDataSources().Where(x => x is FtpSource).ToList();
                    output.AddRange(fTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                case "SFTP":
                    List<DataSource> sfTpList = _dataFlowService.GetDataSources().Where(x => x is SFtpSource).ToList();
                    output.AddRange(sfTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                case "DFSBasic":
                    List<DataSource> dfsBasicList = _dataFlowService.GetDataSources().Where(x => x is DfsBasic).ToList();
                    output.AddRange(dfsBasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                case "DFSCustom":
                    List<DataSource> dfsCustomList = _dataFlowService.GetDataSources().Where(x => x is DfsCustom).ToList();
                    output.AddRange(dfsCustomList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                case "S3Basic":
                    List<DataSource> s3BasicList = _dataFlowService.GetDataSources().Where(x => x is S3Basic).ToList();
                    output.AddRange(s3BasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                case "HTTPS":
                    List<DataSource> HttpsList = _dataFlowService.GetDataSources().Where(x => x is HTTPSSource).ToList();
                    output.AddRange(HttpsList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                case "GOOGLEAPI":
                    List<DataSource> GApiList = _dataFlowService.GetDataSources().Where(x => x is GoogleApiSource).ToList();
                    output.AddRange(GApiList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id.ToString() }).ToList());
                    break;
                default:
                    throw new NotImplementedException();
            }

            return output;
        }

        protected override void AddCoreValidationExceptionsToModel(ValidationException ex)
        {
            foreach (ValidationResult vr in ex.ValidationResults.GetAll())
            {
                switch (vr.Id)
                {
                    //Base Model Errors                    
                    case DataFlow.ValidationErrors.nameIsBlank:
                    case DataFlow.ValidationErrors.nameMustBeUnique:
                    case DataFlow.ValidationErrors.nameContainsReservedWords:
                        ModelState.AddModelError("Name", vr.Description);
                        break;
                    case DataFlow.ValidationErrors.saidAssetIsBlank:
                        ModelState.AddModelError("SAIDAssetKeyCode", vr.Description);
                        break;
                    case DataFlow.ValidationErrors.namedEnvironmentInvalid:
                        ModelState.AddModelError(nameof(DataFlowModel.NamedEnvironment), vr.Description);
                        break;
                    case DataFlow.ValidationErrors.namedEnvironmentTypeInvalid:
                        ModelState.AddModelError(nameof(DataFlowModel.NamedEnvironmentType), vr.Description);
                        break;
                    case "PreprocessingOptions":
                    case SchemaMap.ValidationErrors.schemamapMustContainDataset:
                    case SchemaMap.ValidationErrors.schemamapMustContainSchema:
                    case var sd when (sd.EndsWith("SelectedDataset")):
                    case var ss when (ss.EndsWith("SelectedSchema")):
                        ModelState.AddModelError(vr.Id, vr.Description);
                        break;
                    //Nested model errors
                    case "SelectedDataSource":
                    case "SelectedSourceType":
                    case "SchedulePicker":
                        ModelState.AddModelError($"RetrieverJob.{vr.Id}", vr.Description);
                        break;
                    case RetrieverJob.ValidationErrors.googleApiRelativeUriIsBlank:
                        ModelState.AddModelError("RetrieverJob.RelativeUri", vr.Description);
                        break;
                    case RetrieverJob.ValidationErrors.httpsRequestBodyIsBlank:
                        ModelState.AddModelError("RetrieverJob.HttpRequestBody", vr.Description);
                        break;
                    case RetrieverJob.ValidationErrors.httpsRequestDataFormatNotSelected:
                        ModelState.AddModelError("RetrieverJob.SelectedRequestDataFormat", vr.Description);
                        break;
                    case RetrieverJob.ValidationErrors.httpsRequestMethodNotSelected:
                        ModelState.AddModelError("RetrieverJob.SelectedRequestMethod", vr.Description);
                        break;
                    case RetrieverJob.ValidationErrors.httpsTargetFileNameIsBlank:
                        ModelState.AddModelError("RetrieverJob.TargetFileName", vr.Description);
                        break;
                    case RetrieverJob.ValidationErrors.ftpPatternNotSelected:
                        ModelState.AddModelError("RetrieverJob.FtpPattern", vr.Description);
                        break;

                        break;
                    case DataFlow.ValidationErrors.stepsContainsAtLeastOneSchemaMap:
                    default:
                        ModelState.AddModelError(string.Empty, vr.Description);
                        break;
                }
            }
        }
    }

    public class DataFlowStepFile
    {
        public string Bucket { get; set; }
        public string Key { get; set; }
    }
}