﻿using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        private readonly ISchemaService _schemaService;
        private readonly Lazy<IDataFeatures> _dataFeatures;
        private readonly NamedEnvironmentBuilder _namedEnvironmentBuilder;

        public DataFlowController(IDataFlowService dataFlowService, IDatasetService datasetService, IConfigService configService,
            ISecurityService securityService, ISAIDService saidService, ISchemaService schemaService, Lazy<IDataFeatures> dataFeatures, 
            NamedEnvironmentBuilder namedEnvironmentBuilder)
        {
            _dataFlowService = dataFlowService;
            _datasetService = datasetService;
            _configService = configService;
            _securityService = securityService;
            _saidService = saidService;
            _schemaService = schemaService;
            _dataFeatures = dataFeatures;
            _namedEnvironmentBuilder = namedEnvironmentBuilder;
        }

        public IDataFeatures DataFeatures
        {
            get { return _dataFeatures.Value; }
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

            model.DisplayDataflowEdit = _dataFeatures.Value.CLA1656_DataFlowEdit_ViewEditPage.GetValue();
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
            model.CreatedBy = SharedContext.CurrentUser.AssociateId;
            model.CompressionDropdown = Utility.BuildCompressionDropdown(model.IsCompressed);
            model.PreProcessingRequiredDropdown = Utility.BuildPreProcessingDropdown(model.IsPreProcessingRequired);
            model.PreProcessingOptionsDropdown = Utility.BuildPreProcessingOptionsDropdown(model.PreProcessingSelection);
            model.IngestionTypeDropDown = Utility.BuildIngestionTypeDropdown(model.IngestionTypeSelection);


            CreateDropDownSetup(model.RetrieverJob);
            //Every dataflow requires at least one schemamap, therefore, load a default empty schemamapmodel
            SchemaMapModel schemaModel = new SchemaMapModel
            {
                SelectedDataset = 0,
                CLA3332_ConsolidatedDataFlows = DataFeatures.CLA3332_ConsolidatedDataFlows.GetValue()
            };
            model.SchemaMaps.Add(schemaModel);
            
            model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode);
            model.CLA3332_ConsolidatedDataFlows = DataFeatures.CLA3332_ConsolidatedDataFlows.GetValue();

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            return View("DataFlowForm", model);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ViewResult> Edit(int id)
        {
            UserSecurity us = _securityService.GetUserSecurity(null, SharedContext.CurrentUser);

            //Feature flag will only evaluate true for Admins
            if (!DataFeatures.CLA1656_DataFlowEdit_ViewEditPage.GetValue() || !us.CanCreateDataFlow)
            {
                return View("Forbidden");
            }

            DataFlowDetailDto dto = _dataFlowService.GetDataFlowDetailDto(id);
            if (dto.ObjectStatus != ObjectStatusEnum.Active)
            {
                return View("NotFound");
            }
            DataFlowModel model = ToDataFlowModel(dto);

            model.CompressionDropdown = Utility.BuildCompressionDropdown(model.IsCompressed);
            model.PreProcessingRequiredDropdown = Utility.BuildPreProcessingDropdown(model.IsPreProcessingRequired);
            model.PreProcessingOptionsDropdown = Utility.BuildPreProcessingOptionsDropdown(model.PreProcessingSelection);
            model.IngestionTypeDropDown = Utility.BuildIngestionTypeDropdown(model.IngestionTypeSelection);
            model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode);
            CreateDropDownSetup(model.RetrieverJob);

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType),namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            model.CLA3332_ConsolidatedDataFlows = DataFeatures.CLA3332_ConsolidatedDataFlows.GetValue();

            return View("DataFlowForm", model);

        }

        [HttpPost]
        public async Task<ActionResult> DataFlowForm(DataFlowModel model)
        {
            UserSecurity us = _securityService.GetUserSecurity(null, SharedContext.CurrentUser);            

            AddCoreValidationExceptionsToModel(model.Validate());

            DataFlowDto dfDto = model.ToDto();

            /*
             * After CLA3332_ConsolidatedDataflows feature flag is true and conversion of all dataflows is compelted,
             *   there will be no need for a list of schema maps since consolidated dataflows only supports 
             *   a single schema.  Therefore, refactor dataset\schema selection to be directly on DataFlowModel
             *   (https://jira.sentry.com/browse/CLA-3507).
            */
            if (DataFeatures.CLA3332_ConsolidatedDataFlows.GetValue()){
                dfDto.DatasetId = model.SchemaMaps.FirstOrDefault().SelectedDataset;
                dfDto.SchemaId = model.SchemaMaps.FirstOrDefault().SelectedSchema;
            }

            AddCoreValidationExceptionsToModel(await _dataFlowService.Validate(dfDto));

            try
            {
                if (ModelState.IsValid)
                {
                    int newFlowId = 0;

                    /************************************************
                     * Incoming Post does not have dataflow Id 
                     *    therefore, user created a new dataflow 
                     ************************************************/
                    if (dfDto.Id == 0)
                    {
                        newFlowId = _dataFlowService.CreateandSaveDataFlow(dfDto);
                        if (newFlowId != 0)
                        {
                            return RedirectToAction("Detail", "DataFlow", new { id = newFlowId });
                        }
                    }
                    /************************************************
                     *  Incoming Post has a non-zero dataflow Id, 
                     *      therefore, user updated an existing
                     *      dataflow
                    ************************************************/
                    else
                    {
                        //Feature flag will only evaluate true for Admins
                        if (!DataFeatures.CLA1656_DataFlowEdit_SubmitEditPage.GetValue() || !us.CanCreateDataFlow)
                        {
                            return View("Forbidden");
                        }

                        newFlowId = _dataFlowService.UpdateandSaveDataFlow(dfDto);

                        return RedirectToAction("Detail", "DataFlow", new { id = newFlowId });
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
            //catch (DataFlowStepNotImplementedException stepEx)
            //{
            //    //User option selection not valid for dataflowstep mappings
            //    ValidationResults results = new ValidationResults();
            //    results.Add(stepEx.Message);
            //    AddCoreValidationExceptionsToModel(new ValidationException(results));
            //}
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


            /*
             *  At this point, something has failed 
             * 
             */

                    model.CompressionDropdown = Utility.BuildCompressionDropdown(model.IsCompressed);
            model.PreProcessingRequiredDropdown = Utility.BuildPreProcessingDropdown(model.IsPreProcessingRequired);
            model.PreProcessingOptionsDropdown = Utility.BuildPreProcessingOptionsDropdown(model.PreProcessingSelection);
            model.IngestionTypeDropDown = Utility.BuildIngestionTypeDropdown(model.IngestionTypeSelection);
            if (model.RetrieverJob != null)
            {
                CreateDropDownSetup(model.RetrieverJob);
            }
            if (model.SchemaMaps != null && model.SchemaMaps.Count > 0)
            {
                foreach (SchemaMapModel mapModel in model.SchemaMaps)
                {
                    SetSchemaModelLists(mapModel);
                    mapModel.CLA3332_ConsolidatedDataFlows = DataFeatures.CLA3332_ConsolidatedDataFlows.GetValue();
                }
            }
            model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode);
            model.CLA3332_ConsolidatedDataFlows = DataFeatures.CLA3332_ConsolidatedDataFlows.GetValue();

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment);
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
                SelectedDataset = 0,
                AllDatasets = BuildDatasetDropDown(0),
                AllSchemas = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Value = "0",
                        Text = "Select Dataset First",
                        Selected = true,
                        Disabled = true
                    }
                }
            };
            dfModel.SchemaMaps.Add(schemaModel);

            return PartialView(dfModel);
        }

        [HttpGet]
        [Route("DataFlow/NewCompressionJob/")]
        public PartialViewResult NewCompressionJob()
        {
            CompressionModel model = ToCompressionModel(null);

            //CompressionModel model = new CompressionModel();

            //CreateDropDownList(model);

            return PartialView("_CompressionJob", model);

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

        public DataFlowModel ToDataFlowModel(DataFlowDetailDto dto)
        {
            DataFlowModel model = new DataFlowModel()
            {
                DataFlowId = dto.Id,
                Name = dto.Name,
                IngestionTypeSelection = dto.IngestionType,
                IsCompressed = dto.IsCompressed,
                IsPreProcessingRequired = dto.IsPreProcessingRequired,
                PreProcessingSelection = (dto.PreProcessingOption.HasValue) ? (int)dto.PreProcessingOption : 0,
                SAIDAssetKeyCode = dto.SaidKeyCode,
                CreatedBy = dto.CreatedBy,
                CreatedDTM = dto.CreateDTM,
                ObjectStatus = dto.ObjectStatus,
                StorageCode = dto.FlowStorageCode,
                SelectedDataset = dto.DatasetId,
                SelectedSchema = dto.SchemaId
            };

            if (dto.SchemaMap.Any())
            {
                List<SchemaMapModel> schemaMapModelList = new List<SchemaMapModel>();
                foreach (SchemaMapDto map in dto.SchemaMap)
                {
                    SchemaMapModel scmModel = map.ToModel();
                    schemaMapModelList.Add(scmModel);
                }
                model.SchemaMaps = schemaMapModelList;
            }

            if (dto.IsCompressed)
            {
                List<CompressionModel> modelList = new List<CompressionModel>()
                {
                    ToCompressionModel(dto)
                };
                model.CompressionJob = modelList;
            }

            if (dto.IngestionType == (int)IngestionType.DSC_Pull)
            {
                RetrieverJobDto jobDto = _dataFlowService.GetAssociatedRetrieverJobDto(dto.Id);
                model.RetrieverJob = ToJobModel(jobDto);
            }

            return model;
        }

        private JobModel ToJobModel(RetrieverJobDto dto)
        {
            JobModel model = new JobModel()
            {
                Schedule = dto.Schedule,
                SchedulePicker = dto.SchedulePicker.ToString(),
                RelativeUri = dto.RelativeUri,
                HttpRequestBody = dto.HttpRequestBody,
                SearchCriteria = dto.SearchCriteria,
                TargetFileName = dto.TargetFileName,
                CreateCurrentFile = dto.CreateCurrentFile,
                SelectedDataSource = dto.DataSourceId.ToString(),
                SelectedSourceType = dto.DataSourceType,
                SelectedRequestMethod = dto.RequestMethod ?? Core.HttpMethods.none,
                SelectedRequestDataFormat = dto.RequestDataFormat ?? Core.HttpDataFormat.none,
                FtpPattern = dto.FtpPattern ?? Core.FtpPattern.NoPattern
            };

            model.SchedulePickerDropdown = Utility.BuildSchedulePickerDropdown(dto.ReadableSchedule);
            model.SchedulePicker = model.SchedulePickerDropdown.Where(w => w.Selected).Select(s => Int32.Parse(s.Value)).FirstOrDefault().ToString();

            return model;
        }
         
        public CompressionModel ToCompressionModel(DataFlowDto dto)
        {
            CompressionModel model = new CompressionModel();
            if(dto != null)
            {
                //model.CompressionType = ((CompressionTypes)dto.CompressionType).ToString();
                model.CompressionType = (dto.CompressionType.HasValue) ? dto.CompressionType.ToString() : "0";
            }
            model.CompressionTypesDropdown = Utility.BuildCompressionTypesDropdown(model.CompressionType);

            return model;
        }

        private void SetSchemaModelLists(SchemaMapModel model)
        {
            List<SelectListItem> dsList = new List<SelectListItem>();
            List<SelectListItem> scmList = new List<SelectListItem>();
            var groupedDatasets = _datasetService.GetDatasetsForQueryTool().GroupBy(x => x.DatasetCategories.First());

            if (model.SelectedDataset == 0)
            {
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

        
        [HttpGet]
        [Route("DataFlow/NamedEnvironment")]
        public async Task<PartialViewResult> _NamedEnvironment(string assetKeyCode, string namedEnvironment)
        {
            DataFlowModel model = new DataFlowModel()
            {
                SAIDAssetKeyCode = assetKeyCode,
                NamedEnvironment = namedEnvironment
            };

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(assetKeyCode, namedEnvironment).ConfigureAwait(false);
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

        

        private async Task<List<SelectListItem>> BuildSAIDAssetDropDown(string keyCode)
        {
            List<SelectListItem> output = new List<SelectListItem>();

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

        private List<SelectListItem> BuildDatasetDropDown(int datasetSelection)
        {
            List<SelectListItem> datasetList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "-1",
                    Text = "Create Dataset",
                    Selected = false,
                    Disabled = false
                },

                new SelectListItem
                {
                    Value = "0",
                    Text = "Select Dataset",
                    Selected = (datasetSelection == 0),
                    Disabled = true
                }
            };

            foreach (KeyValuePair<int, string> item in _datasetService.GetDatasetList())
            {
                datasetList.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Selected = (datasetSelection == item.Key),
                    Disabled = false
                });
            }

            return datasetList;
        }

        private List<SelectListItem> BuildSchemaDropDown(int schemaSelection)
        {
            List<SelectListItem> schemaList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "-1",
                    Text = "Create Dataset",
                    Selected = false,
                    Disabled = false
                },

                new SelectListItem
                {
                    Value = "0",
                    Text = "Select Schema",
                    Selected = (schemaSelection == 0),
                    Disabled = true
                }
            };

            foreach (KeyValuePair<int, string> item in _schemaService.GetSchemaList())
            {
                schemaList.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Selected = (schemaSelection == item.Key),
                    Disabled = false
                });
            }

            return schemaList;
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
                    case GlobalConstants.ValidationErrors.SAID_ASSET_REQUIRED:
                        ModelState.AddModelError(nameof(DataFlowModel.SAIDAssetKeyCode), vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_INVALID:
                        ModelState.AddModelError(nameof(DataFlowModel.NamedEnvironment), vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID:
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