using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Web.Helpers;
using Sentry.data.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Controllers
{
    public class ConfigController : BaseController
    {
        #region Fields
        private readonly IAssociateInfoProvider _associateInfoProvider;
        private readonly IDatasetContext _datasetContext;
        private readonly IConfigService _configService;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IDatasetService _DatasetService;
        private readonly IObsidianService _obsidianService;
        private readonly ISecurityService _securityService;
        private readonly ISchemaService _schemaService;
        private readonly IDataFeatures _featureFlags;
        private readonly Lazy<IDataApplicationService> _dataApplicationService;
        private readonly IDataSourceService _dataSourceService;
        #endregion

        #region Constructor
        public ConfigController(IDatasetContext dsCtxt, IUserService userService, IAssociateInfoProvider associateInfoService,
            IConfigService configService, IEventService eventService, IDatasetService datasetService, 
            IObsidianService obsidianService, ISecurityService securityService, ISchemaService schemaService, 
            IDataFeatures dataFeatures, Lazy<IDataApplicationService> dataApplicationService, IDataSourceService dataSourceService)
        {
            _datasetContext = dsCtxt;
            _userService = userService;
            _associateInfoProvider = associateInfoService;
            _configService = configService;
            _eventService = eventService;
            _DatasetService = datasetService;
            _obsidianService = obsidianService;
            _securityService = securityService;
            _schemaService = schemaService;
            _featureFlags = dataFeatures;
            _dataApplicationService = dataApplicationService;
            _dataSourceService = dataSourceService;
        }
        #endregion

        private IDataApplicationService DataApplicationService
        {
            get { return _dataApplicationService.Value; }
        }

        #region Controller Methods
        [HttpGet]
        [Route("Config/Dataset/{id}")]
        public ActionResult Index(int id)
        {

            Dataset ds = _datasetContext.GetById(id);

            UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());

            if (us != null && us.CanManageSchema)
            {
                DatasetSchemaDto dsDto = _DatasetService.GetDatasetSchemaDto(id);

                List<DatasetFileConfigsModel> configModelList = new List<DatasetFileConfigsModel>();
                foreach (DatasetFileConfig config in ds.DatasetFileConfigs)
                {
                    DatasetFileConfigsModel model = new DatasetFileConfigsModel(config, true, false);

                    //Add DataFlow RetrieverJobs to Model
                    Tuple<DataFlowDetailDto, List<RetrieverJob>> jobs2 = _configService.GetDataFlowForSchema(config);
                    model.DataFlowJobs = (jobs2.Item1 != null) ? jobs2.ToModel() : null;

                    //Add External RetrieverJobs associated with DataFlows to Model
                    model.ExternalDataFlowJobs = _configService.GetExternalDataFlowsBySchema(config).ToModel();
                    configModelList.Add(model);
                }

                ManageConfigsModel mcm = new ManageConfigsModel()
                {
                    DatasetId = dsDto.DatasetId,
                    DatasetName = dsDto.DatasetName,
                    CategoryColor = dsDto.CategoryColor,
                    DatasetFileConfigs = configModelList,
                    DisplayDataflowMetadata = _featureFlags.Expose_Dataflow_Metadata_CLA_2146.GetValue(),
                    DisplayDataflowEdit = _featureFlags.CLA1656_DataFlowEdit_ViewEditPage.GetValue()
                };

                mcm.Security = _DatasetService.GetUserSecurityForDataset(id);

                CreateEvent("Viewed Dataset Configuration Page", ds.DatasetId);

                if (_featureFlags.CLA3878_ManageSchemasAccordion.GetValue())
                {
                    return View(mcm);
                }

                return View("Index_v1", mcm);
            }
            else
            {
                return View("Forbidden");
            }
        }

        [HttpGet]
        [Route("Config/Dataset/{id}/Create")]
        public ActionResult Create(int id)
        {
            var userSecurity = _DatasetService.GetUserSecurityForDataset(id);
            if (userSecurity.CanManageSchema)
            {
                return View(GetDatasetFileConfigsModel(id));
            }
            return View("Forbidden");
        }

        [Route("Config/Dataset/ShowFileUpload/{configId}")]
        [HttpGet]
        public bool ShowFileUpload(int configId)
        {
            var userSecurity = _configService.GetUserSecurityForConfig(configId);
            var uploadFlag = _featureFlags.CLA4152_UploadFileFromUI.GetValue();
            var schemaId = _datasetContext.DatasetFileConfigs.Where(c => c.ConfigId == configId).First().Schema.SchemaId;
            var hasDataflow = _datasetContext.DataFlow.Any(df => df.SchemaId == schemaId);
            return userSecurity.CanUploadToDataset && uploadFlag && hasDataflow;
        }

        [HttpGet]
        public PartialViewResult _DatasetFileConfigCreate(int id)
        {
            var userSecurity = _DatasetService.GetUserSecurityForDataset(id);
            if (userSecurity.CanManageSchema)
            {
                return PartialView("_DatasetFileConfigCreate", GetDatasetFileConfigsModel(id));
            }
            return PartialView("Forbidden");
        }

        [HttpPost]
        [Route("Config/DatasetFileConfigForm")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult DatasetFileConfigForm(DatasetFileConfigsModel dfcm)
        {
            DatasetFileConfigDto dto = dfcm.ToDto();

            AddCoreValidationExceptionsToModel(_configService.Validate(dto));

            if (ModelState.IsValid)
            {
                FileSchemaDto schemaDto = dfcm.ToSchema();

                if (dto.ConfigId == 0) //Create Dataset File Config
                {                     
                    int newSchemaId = _schemaService.CreateAndSaveSchema(schemaDto);
                    if (newSchemaId != 0)
                    {
                        dto.SchemaId = newSchemaId;

                        if (_configService.CreateAndSaveDatasetFileConfig(dto))
                        {
                            _schemaService.PublishSchemaEvent(schemaDto.ParentDatasetId, newSchemaId);
                            return Json(new { Success = true, dataset_id = dto.ParentDatasetId, schema_id = dto.SchemaId });
                        }
                    }
                }
                else if (_schemaService.UpdateAndSaveSchema(schemaDto) && _configService.UpdateAndSaveDatasetFileConfig(dto)) //Edit Dataset File Config
                {
                    return RedirectToAction("Index", new { id = dto.ParentDatasetId });
                }
            }

            AddCommonProperties(dfcm);

            if (dto.ConfigId == 0)
            {
                return PartialView("_DatasetFileConfigCreate", dfcm);
            }
            else
            {
                return View("Edit", dfcm);
            }
        }

        [HttpDelete]
        [Route("Config/{id}")]
        public JsonResult Delete(int id)
        {
            try
            {
                UserSecurity us = _configService.GetUserSecurityForConfig(id);

                if (us != null && us.CanEditDataset)
                {
                    bool IsDeleted = DataApplicationService.DeleteDatasetFileConfig(new List<int> { id }, SharedContext.CurrentUser);
                    if (!IsDeleted)
                    {
                        return Json(new { Success = false, Message = "Schema was not deleted" });
                    }
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.DELETE_DATASET_SCHEMA, "Deleted Dataset Schema", id);
                    return Json(new { Success = true, Message = "Schema was successfully deleted" });
                }
                return Json(new { Success = false, Message = "You do not have permissions to delete schema" });
            }
            catch (DatasetFileConfigDeletedException ex)
            {
                return Json(new { Success = false, ex.Message });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete dataset schema - DatasetId:{id} RequestorId:{SharedContext.CurrentUser.AssociateId} RequestorName:{SharedContext.CurrentUser.DisplayName}", ex);
                return Json(new { Success = false, Message = "We failed to delete schema.  Please try again later.  Please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a> if problem persists." });
            }
        }

        [HttpGet]
        [Route("Config/Manage")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult Manage()
        {
            DatasetFileConfigsModel edfc = new DatasetFileConfigsModel();
            CreateEvent("Viewed Configuration Management Page");
            return View(edfc);
        }

        [HttpGet]
        [Route("Config/Edit/{configId}")]
        public ActionResult Edit(int configId)
        {
            try
            {
                UserSecurity us = _DatasetService.GetUserSecurityForConfig(configId);

                DatasetFileConfigDto dto = _configService.GetDatasetFileConfigDto(configId);

                //Users are unable to edit 
                if (!us.CanManageSchema || dto.DeleteInd)
                {
                    return View("Forbidden");
                }

                DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel(dto);

                dfcm.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext, dfcm.DatasetScopeTypeID);
                dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
                dfcm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, _featureFlags, dfcm.FileExtensionID);
                dfcm.DatasetScopeReadonly = dfcm.AllDatasetScopeTypes.Where(s => s.Value == dfcm.DatasetScopeTypeID.ToString()).Select(n => n.Text).FirstOrDefault();
                dfcm.FileExtensionReadonly = dfcm.ExtensionList.Where(s => s.Value == dfcm.FileExtensionID.ToString()).Select(n => n.Text).FirstOrDefault();

                //ViewBag.ModifyType = "Edit";

                _eventService.PublishSuccessEventByConfigId(GlobalConstants.EventType.VIEWED, "Viewed Configuration Edit Page", configId);

                return View(dfcm);
            }
            catch (SchemaUnauthorizedAccessException)
            {
                return View("Unauthorized");
            }
            catch (Exception)
            {
                return View("ServerError");
            }
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult GetEditConfigPartialView(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.getDatasetFileConfigs(configId);
            EditDatasetFileConfigModel edfc = new EditDatasetFileConfigModel(dfc)
            {
                DatasetId = dfc.ParentDataset.DatasetId
            };
            edfc.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext, edfc.DatasetScopeTypeID);
            edfc.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            edfc.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, _featureFlags, edfc.FileExtensionID);
            edfc.DatasetScopeReadonly = edfc.AllDatasetScopeTypes.Where(s => s.Value == edfc.DatasetScopeTypeID.ToString()).Select(n => n.Text).FirstOrDefault();
            ViewBag.ModifyType = "Edit";

            return PartialView("_EditConfigFile", edfc);
        }

        [HttpGet]
        [Route("Config/{configId}/Job/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateRetrievalJob(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);

            //Users are unable to delete config already marked as deleted
            if (dfc.DeleteInd)
            {
                return View("Forbidden");
            }

            ViewBag.DatasetId = dfc.ParentDataset.DatasetId;
            CreateJobModel cjm = new CreateJobModel(dfc.ConfigId, dfc.ParentDataset.DatasetId)
            {
                Security = _securityService.GetUserSecurity(null, _userService.GetCurrentUser())
            };

            cjm = CreateDropDownSetup(cjm);

            CreateEvent("Viewed Retrieval Creation Page", dfc.ParentDataset.DatasetId, dfc.ConfigId);

            return View(cjm);
        }

        [HttpPost]
        [Route("Config/{configId}/Job/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateRetrievalJob(CreateJobModel cjm)
        {
            try
            {
                AddCoreValidationExceptionsToModel(cjm.Validate());

                if (ModelState.IsValid)
                {
                    DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(cjm.DatasetConfigID);
                    DataSource dataSource = _datasetContext.GetById<DataSource>(cjm.SelectedDataSource);
                    List<RetrieverJob> jobList = dfc.RetrieverJobs.ToList();

                    Compression compression = new Compression()
                    {
                        IsCompressed = cjm.IsSourceCompressed,
                        CompressionType = cjm.CompressionType,
                        FileNameExclusionList = cjm.NewFileNameExclusionList != null ? cjm.NewFileNameExclusionList.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList() : new List<string>()
                    };

                    RetrieverJobOptions rjo = new RetrieverJobOptions()
                    {
                        OverwriteDataFile = cjm.OverwriteDataFile,
                        TargetFileName = cjm.TargetFileName,
                        CreateCurrentFile = cjm.CreateCurrentFile,
                        IsRegexSearch = cjm.IsRegexSearch,
                        SearchCriteria = cjm.SearchCriteria,
                        CompressionOptions = compression,
                        FtpPattern = cjm.FtpPattern
                    };

                    if (dataSource.Is<GoogleApiSource>())
                    {
                        HttpsOptions ho = new HttpsOptions()
                        {
                            Body = cjm.HttpRequestBody,
                            RequestMethod = cjm.SelectedRequestMethod,
                            RequestDataFormat = cjm.SelectedRequestDataFormat
                        };

                        rjo.HttpOptions = ho;
                    }


                    RetrieverJob rj = new RetrieverJob()
                    {

                        Schedule = cjm.Schedule,
                        TimeZone = "Central Standard Time",
                        RelativeUri = cjm.RelativeUri,
                        DataSource = dataSource,
                        DatasetConfig = dfc,
                        Created = DateTime.Now,
                        Modified = DateTime.Now,
                        IsGeneric = false,
                        JobOptions = rjo,
                        ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active
                    };

                    rj.DataSource.CalcRelativeUri(rj);

                    jobList.Add(rj);
                    dfc.RetrieverJobs = jobList;

                    _datasetContext.Merge<DatasetFileConfig>(dfc);
                    _datasetContext.SaveChanges();

                    return RedirectToAction("Index", new { id = cjm.DatasetID });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }
            catch (System.UriFormatException uriEx)
            {
                ModelState.AddModelError("RelativeUri", uriEx.Message);
            }

            cjm = CreateDropDownSetup(cjm);
            cjm.Security = _securityService.GetUserSecurity(null, _userService.GetCurrentUser());
            return View(cjm);
        }

        [HttpGet]
        [Route("Config/{configId}/Job/Edit/{jobId}")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult EditRetrievalJob(int configId, int jobId)
        {
            RetrieverJob retrieverJob = _datasetContext.GetById<RetrieverJob>(jobId);

            //do not allow editing of retriever jobs which associated schema is logically deleted
            if ((retrieverJob.DatasetConfig != null && retrieverJob.DatasetConfig.DeleteInd) ||
                (retrieverJob.FileSchema != null && retrieverJob.FileSchema.DeleteInd))
            {
                return View("Forbidden");
            }

            EditJobModel ejm = new EditJobModel(retrieverJob)
            {
                Security = _securityService.GetUserSecurity(null, _userService.GetCurrentUser()),
                SelectedDataSource = retrieverJob.DataSource.Id,
                SelectedSourceType = retrieverJob.DataSource.SourceType
            };

            ejm = EditDropDownSetup(ejm, retrieverJob);

            CreateEvent("Viewed Retrieval Edit Page");

            return View(ejm);
        }

        [HttpPost]
        [Route("Config/{configId}/Job/Edit/{jobId}")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult EditRetrievalJob(EditJobModel ejm)
        {
            RetrieverJob rj = _datasetContext.GetById<RetrieverJob>(ejm.JobID);

            try
            {
                if ((rj.DatasetConfig != null && rj.DatasetConfig.DeleteInd) ||
                (rj.FileSchema != null && rj.FileSchema.DeleteInd))
                {
                    throw new DatasetFileConfigDeletedException("Parent Schema is marked for deletion");
                }

                AddCoreValidationExceptionsToModel(ejm.Validate());

                if (ModelState.IsValid)
                {
                    DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(ejm.DatasetConfigID);
                    DataSource dataSource = _datasetContext.GetById<DataSource>(ejm.SelectedDataSource);

                    Compression compression = new Compression()
                    {
                        IsCompressed = ejm.IsSourceCompressed,
                        CompressionType = ejm.CompressionType,
                        FileNameExclusionList = ejm.NewFileNameExclusionList.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList()
                    };

                    HttpsOptions hOptions = new HttpsOptions()
                    {
                        Body = ejm.HttpRequestBody,
                        RequestMethod = ejm.SelectedRequestMethod,
                        RequestDataFormat = ejm.SelectedRequestDataFormat
                    };

                    rj.JobOptions = new RetrieverJobOptions()
                    {
                        OverwriteDataFile = ejm.OverwriteDataFile,
                        TargetFileName = ejm.TargetFileName,
                        CreateCurrentFile = ejm.CreateCurrentFile,
                        IsRegexSearch = ejm.IsRegexSearch,
                        SearchCriteria = ejm.SearchCriteria,
                        CompressionOptions = compression,
                        HttpOptions = hOptions,
                        FtpPattern = ejm.FtpPattern
                    };

                    rj.Schedule = ejm.Schedule;
                    rj.TimeZone = "Central Standard Time";
                    rj.RelativeUri = ejm.RelativeUri;
                    if (!rj.IsGeneric)
                    {
                        rj.DataSource = dataSource;
                    }

                    rj.DatasetConfig = dfc;
                    rj.Modified = DateTime.Now;

                    rj.DataSource.CalcRelativeUri(rj);

                    _datasetContext.SaveChanges();

                    return RedirectToAction("Index", new { id = ejm.DatasetID });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }
            catch (System.ArgumentNullException ex)
            {
                ModelState.AddModelError("RelativeUri", ex.Message);
            }
            catch (System.UriFormatException ex)
            {
                ModelState.AddModelError("RelativeUri", ex.Message);
            }

            ejm = EditDropDownSetup(ejm, rj);
            return View(ejm);
        }

        [HttpGet]
        [Route("Config/Source/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateSource()
        {
            DataSourceModel dsm = CreateSourceDropDown(new DataSourceModel());
            dsm.ReturnUrl = "/";
            dsm.CLA2868_APIPaginationSupport = _featureFlags.CLA2868_APIPaginationSupport.GetValue();
            CreateEvent("Viewed Data Source Creation Page");
            return View("CreateDataSource", dsm);
        }

        [Route("Config/HeaderEntryRow")]
        public ActionResult HeaderEntryRow()
        {
            return PartialView("_Headers");
        }
        
        [Route("Config/AddAcceptableError")]
        public ActionResult AddAcceptableError()
        {
            return PartialView("DataSource/_AcceptableError");
        }

        [Route("Config/AddToken")]
        public ActionResult AddToken()
        {
            return PartialView("_DataSourceToken");
        }

        public ActionResult FieldEntryRow()
        {
            return PartialView("_Fields", new FieldRowModel());
        }

        public ActionResult ExtensionEntryRow()
        {
            return PartialView("_ExtensionMapping");
        }

        [HttpPost]
        [Route("Config/DataSourceForm")]
        [AuthorizeByPermission(PermissionCodes.DATASET_MODIFY)]
        public ActionResult DataSourceForm(DataSourceModel model)
        {
            DataSourceDto dto = model.ToDto();

            List<string> validations = _configService.Validate(dto);
            AddCoreValidationExceptionsToModel(validations);

            if (ModelState.IsValid)
            {
                bool isSuccessful;
                if (dto.OriginatingId == 0)
                {
                    isSuccessful = _configService.CreateAndSaveNewDataSource(dto);
                }
                else
                {
                    isSuccessful = _configService.UpdateAndSaveDataSource(dto);
                }

                if (isSuccessful)
                {
                    return Redirect(string.IsNullOrWhiteSpace(model.ReturnUrl) ? "/": model.ReturnUrl);
                }
            }

            if (model.Id == 0)
            {
                model = CreateSourceDropDown(model);
                return View("CreateDataSource", model);
            }
            else
            {
                EditSourceDropDown(model);
                return View("EditDataSource", model);
            }
        }

        [HttpGet]
        [Route("Config/Source/Edit/{sourceID}")]
        [AuthorizeByPermission(PermissionCodes.DATASET_MODIFY)]
        public ActionResult EditSource(int sourceID)
        {
            UserSecurity us = _configService.GetUserSecurityForDataSource(sourceID);
            if (us != null && us.CanEditDataSource)
            {
                DataSourceDto dto = _configService.GetDataSourceDto(sourceID);
                DataSourceModel model = new DataSourceModel(dto);
                model.ReturnUrl = "/DataFlow/Create";
                model.CLA2868_APIPaginationSupport = _featureFlags.CLA2868_APIPaginationSupport.GetValue();

                EditSourceDropDown(model);

                _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, "Viewed Data Source Edit Page");

                return View("EditDataSource", model);
            }

            return View("Forbidden");
        }

        [HttpGet]
        [Route("Config/Extension/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateExtensionMapping()
        {
            CreateExtensionMapModel cem = new CreateExtensionMapModel();
            cem.ExtensionMappings = _datasetContext.MediaTypeExtensions.ToList();

            return View("CreateExtensionMapping", cem);
        }

        [HttpPost]
        [Route("Config/Extension/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateExtensionMapping(CreateExtensionMapModel cem)
        {
            int changeCnt = 0;
            try
            {
                List<MediaTypeExtension> currentExtensions = _datasetContext.MediaTypeExtensions.ToList();

                foreach (MediaTypeExtension extension in cem.ExtensionMappings)
                {
                    if (!currentExtensions.Any(x => x.Key == extension.Key && x.Value == extension.Value))
                    {
                        _datasetContext.Merge(extension);
                        Logger.Info($"Adding Extension Mapping - Key:{extension.Key} Value:{extension.Value} Requestor:{SharedContext.CurrentUser.AssociateId}");
                        changeCnt++;
                    }
                }

                List<MediaTypeExtension> deleteList = currentExtensions.Except(cem.ExtensionMappings, new MediaTypeExtension()).ToList();

                foreach (MediaTypeExtension extension in deleteList)
                {
                    _datasetContext.RemoveById<MediaTypeExtension>(extension.Id);

                    Logger.Info($"Deleting Extension Mapping - Key:{extension.Key} Value:{extension.Value} Requestor:{SharedContext.CurrentUser.AssociateId}");
                    changeCnt++;
                }

                if (changeCnt > 0)
                {
                    _datasetContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Configuration.Logging.Logger.Logger.Error("Failed to save extension changes", ex);
                return Json(new { Success = false, Message = "Failed to save changes! Contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a> if problem persists." });
            }

            if (changeCnt == 0)
            {
                return Json(new { Success = true, Message = "No changes detected" });
            }
            else
            {
                return Json(new { Success = true, Message = $"Successfully saved {changeCnt} changes" });
            }
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Config/SourcesByType/")]
        public JsonResult SourcesByType(string sourceType)
        {
            return Json(DataSourcesByType(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Config/AuthenticationByType/")]

        public JsonResult AuthenticationByType(string sourceType)
        {
            return Json(AuthenticationTypesByType(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Config/RequestMethodByType/{sourceType}")]
        public JsonResult RequestMethodByType(string sourceType)
        {
            return Json(GetRequestMethods(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Config/RequestDataFormatByType/{sourceType}")]
        public JsonResult RequestDataFormatByType(string sourceType)
        {
            return Json(GetRequestDataFormat(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        [Route("Config/SourceTypeDescription/")]
        public JsonResult SourceTypeDescription(string sourceType)
        {
            var temp = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == sourceType).Select(x => x.Description).FirstOrDefault();

            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        [Route("Config/DataSourceDescription/")]
        public JsonResult DataSourceDescription(int sourceId)
        {
            var temp = _datasetContext.DataSources.Where(x => x.Id == sourceId).Select(x => new { Description = x.Description, BaseUri = x.BaseUri }).FirstOrDefault();

            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        [Route("Config/AuthTypeDescription/")]
        public JsonResult AuthTypeDescription(int AuthID)
        {
            var temp = _datasetContext.AuthTypes.Where(x => x.AuthID == AuthID).Select(x => x.Description).FirstOrDefault();

            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Config/{configId}/Schema/{schemaId}/Fields")]
        public ActionResult Fields(int configId, int schemaId)
        {
            try
            {
                DatasetFileConfigDto configDto = _configService.GetDatasetFileConfigDto(configId);

                UserSecurity us = _DatasetService.GetUserSecurityForConfig(configId);

                if (!us.CanManageSchema || configDto.DeleteInd)
                {
                    return View("Forbidden");
                }

                if (configDto.Schema.SchemaId == schemaId)
                {
                    ObsoleteDatasetModel bdm = new ObsoleteDatasetModel(_datasetContext.GetById<Dataset>(configDto.ParentDatasetId), _associateInfoProvider, _datasetContext)
                    {
                        CanEditDataset = us.CanEditDataset,
                        CanUpload = us.CanUploadToDataset,
                        Security = us
                    };

                    CreateEvent("Viewed Edit Fields", configDto.ParentDatasetId, configDto.ConfigId);

                    ViewBag.Schema = _datasetContext.GetById<FileSchema>(schemaId);
                    ViewBag.Date_Default = GlobalConstants.Datatypes.Defaults.DATE_DEFAULT;
                    ViewBag.Timestamp_Default = GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT;

                    return View(bdm);
                }
                else
                {
                    return RedirectToAction("Index", new { id = configDto.ParentDatasetId });
                }
            }
            catch (SchemaUnauthorizedAccessException)
            {
                return View("Forbidden");
            }
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/{schemaId}/UpdateFields")]
        public JsonResult UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows)
        {
            try
            {
                List<BaseFieldDto> schemaRowsDto = schemaRows.ToDto();

                _schemaService.ValidateCleanedFields(schemaId, schemaRowsDto);
                _schemaService.CreateAndSaveSchemaRevision(schemaId, schemaRowsDto, "UpdateFromUI");

            }
            catch (SchemaUnauthorizedAccessException)
            {
                return Json(new { Success = false, Message = "Unauthorized access"}, JsonRequestBehavior.AllowGet);
            }
            catch (ValidationException vEx)
            {
                return Json(new { Success = false, Message = "Failed schema validation. ", Errors = vEx.ValidationResults.GetAll() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to update schema - ConfigId:{configId} DataElementId:{schemaId}", ex);
                return Json(new { Success = false, Message = "Failed to update schema"});
            }

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("Config/Schema/{schemaId}/ValidateField")]
        public JsonResult ValidateField(int schemaId, SchemaRow schemaRow)
        {
            try
            {
                List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
                dtoList.Add(schemaRow.ToDto(false));

                _schemaService.ValidateCleanedFields(schemaId, dtoList);

                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (ValidationException vEx)
            {
                return Json(new { Success = false, Message = "Failed schema validation", Errors = vEx.ValidationResults.GetAll() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Failed to validate schema rows" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("Config/{configId}/Schema/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateSchema(int configId)
        {
            //NEED To revisit with merging of metadata repository tables

            //Schema schema = _datasetContext.GetById<Schema>(schemaId);

            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);


            CreateSchemaModel createSchemaModel = new CreateSchemaModel()
            {
                DatasetId = dfc.ParentDataset.DatasetId
            };

            CreateEvent("Viewed Retrieval Edit Page");

            return View(createSchemaModel);
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateSchema(int configId, CreateSchemaModel csm)
        {
            //throw new NotImplementedException();
            //NEED To revisit with merging of metadata repository tables

            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);


            try
            {
                if (ModelState.IsValid)
                {
                    

                    _datasetContext.SaveChanges();

                    return RedirectToAction("Index", new { id = csm.DatasetId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }
            catch (System.ArgumentNullException ex)
            {
                ModelState.AddModelError("RelativeUri", ex.Message);
            }
            catch (System.UriFormatException ex)
            {
                ModelState.AddModelError("RelativeUri", ex.Message);
            }

            return View(csm);
        }

        [HttpGet]
        [Route("Config/{configId}/Schema/{schemaId}/Edit")]
        public ActionResult EditSchema(int configId, int schemaId)
        {
            try
            {
                UserSecurity us = _DatasetService.GetUserSecurityForConfig(configId);

                DatasetFileConfigDto config = _configService.GetDatasetFileConfigDto(configId);

                if (!us.CanManageSchema || config.DeleteInd)
                {
                    return View("Forbidden");
                }

                FileSchemaDto schema = (config.Schema.SchemaId == schemaId) ? config.Schema : null;
                //DataElement schema = _datasetContext.GetById<DataElement>(schemaId);
                if (schema != null)
                {
                    EditSchemaModel editSchemaModel = new EditSchemaModel()
                    {
                        Name = schema.Name,
                        Description = schema.Description,
                        IsForceMatch = false,
                        IsPrimary = false,
                        DatasetId = config.ParentDatasetId,
                        Delimiter = schema.Delimiter,
                        Schema_Id = schema.SchemaId,
                        HasHeader = schema.HasHeader,
                        FileTypeId = schema.FileExtensionId
                    };

                    CreateEvent("Viewed Retrieval Edit Page");

                    return View(editSchemaModel);
                }
                else
                {
                    return RedirectToAction("Index", new { id = config.ParentDatasetId });
                }
            }
            catch (SchemaUnauthorizedAccessException)
            {
                return View("Forbidden");
            }
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/{schemaId}/Edit")]
        public ActionResult EditSchema(int configId, int schemaId, EditSchemaModel esm)
        {
            UserSecurity us = _schemaService.GetUserSecurityForSchema(schemaId);

            if (!us.CanManageSchema)
            {
                return View("Forbidden");
            }

            FileSchemaDto fileDto = _schemaService.GetFileSchemaDto(schemaId);
            FileSchemaDto dto = esm.ToDto(fileDto);
            try
            {
                AddCoreValidationExceptionsToModel(_configService.Validate(dto));

                if (ModelState.IsValid)
                {
                    _schemaService.UpdateAndSaveSchema(dto);

                    return RedirectToAction("Index", new { id = esm.DatasetId });
                }
            }
            catch (SchemaUnauthorizedAccessException)
            {
                return View("Forbidden");
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }
            catch (System.ArgumentNullException ex)
            {
                ModelState.AddModelError("RelativeUri", ex.Message);
            }
            catch (System.UriFormatException ex)
            {
                ModelState.AddModelError("RelativeUri", ex.Message);
            }

            return View(esm);
        }

        [HttpGet]
        public ContentResult SourceDetails (int Id)
        {            
            DataSourceDto dto = _configService.GetDataSourceDto(Id);
            DataSourceModel model = new DataSourceModel(dto);
            var jsonSource = JsonConvert.SerializeObject(model,
                Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Content(jsonSource, "application/json");
        }

        [HttpGet]
        public ActionResult DataSourceAccessRequest(int dataSourceId)
        {
            DataSourceAccessRequestModel model = _configService.GetDataSourceAccessRequest(dataSourceId).Result.ToDataSourceModel();
            model.AllAdGroups = _obsidianService.GetAdGroups("").Select(x => new SelectListItem() { Text = x, Value = x }).ToList();
            return PartialView("DataSourceAccessRequest", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAccessRequest(DataSourceAccessRequestModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = null;

            try
            {
                ticketId = await _configService.RequestAccessToDataSource(ar);
            }
            catch (Exception ex)
            {
                Logger.Error("Failure to submit data source access request", ex);
            }

            return string.IsNullOrEmpty(ticketId) 
                ? PartialView("_Success", new SuccessModel("There was an error processing your request.", "", false)) 
                : PartialView("_Success", new SuccessModel("Data Source access was successfully requested.", "Change Id: " + ticketId, true));
        }

        [HttpGet]
        [Route("Config/GetDatatypesByFileExtension/{id}")]
        public JsonResult GetDatatypesByFileExtension(int id)
        {
            FileExtension fe = _datasetContext.GetById<FileExtension>(id);
            ValidDatatypesModel model = new ValidDatatypesModel
            {
                FileExtensionName = fe.Name,
                FileExtension = fe
            };

            switch (fe.Name)
            {
                case ExtensionNames.CSV:
                    model.IsPositional = true;
                    model.IsFixedWidth = false;
                    break;
                case ExtensionNames.ANY:
                case ExtensionNames.DELIMITED:
                case ExtensionNames.TXT:
                    model.IsPositional = false;
                    model.IsFixedWidth = false;
                    break;
                case ExtensionNames.FIXEDWIDTH:
                    model.IsPositional = true;
                    model.IsFixedWidth = true;
                    break;
                case ExtensionNames.JSON:
                case ExtensionNames.XML:
                case ExtensionNames.PARQUET:
                    model.IsPositional = false;
                    model.IsFixedWidth = false;
                    model.ValidDatatypes.Add(new DataTypeModel(Datatypes.STRUCT, "A struct", "Complex Data Types"));
                    break;
                default:
                    break;
            }

            //Common datatypes across all FileExtensions
            model.ValidDatatypes.Add(new DataTypeModel(Datatypes.VARCHAR, "A varying-length character string.", "String Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(Datatypes.INTEGER, "A signed four-byte integer.", "Numeric Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(Datatypes.BIGINT, "A signed eight-byte integer, from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807.", "Numeric Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(Datatypes.DECIMAL, "A fixed-point decimal number, with 38 digits precision.", "Numeric Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(Datatypes.DATE, "An ANSI SQL date type. YYYY-MM-DD", "Date Time Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(Datatypes.TIMESTAMP, "A UNIX timestamp with optional nanosecond precision. YYYY-MM-DD HH:MM:SS.sss", "Date Time Data Types"));

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _RetrieverJob(int jobId)
        {
            RetrieverJob job = _datasetContext.GetById<RetrieverJob>(jobId);

            ViewData["EnableJobControls"] = "false";
            ViewData["Color"] = "blue";
            return PartialView("~/Views/RetrieverJob/_RetrieverJob.cshtml", job);
        }

        [HttpGet]
        [Route("Config/GetDataScopeTypeDescription/{id}")]
        public JsonResult GetDataScopeTypeDescription(int id)
        {
            string description = _datasetContext.DatasetScopeTypes.Where(w => w.ScopeTypeId == id).Select(s => s.Description).FirstOrDefault();

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json(new { Description = description }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Methods
        private void CreateEvent(string reason, int? datasetId = null, int? configId = null)
        {
            Event e = new Event()
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = reason,
                Dataset = datasetId,
                DataConfig = configId
            };

            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
        }

        private DatasetFileConfigsModel GetDatasetFileConfigsModel(int datasetId)
        {
            Dataset parent = _datasetContext.GetById<Dataset>(datasetId);

            DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel
            {
                DatasetId = datasetId,
                ParentDatasetName = parent.DatasetName,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active
            };

            AddCommonProperties(dfcm);

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Configuration Creation Page", dfcm.DatasetId);

            return dfcm;
        }

        private void AddCommonProperties(DatasetFileConfigsModel dfcm)
        {
            dfcm.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext);
            dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            dfcm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, _featureFlags);
            dfcm.Security = _securityService.GetUserSecurity(null, SharedContext.CurrentUser);
        }

        private CreateJobModel CreateDropDownSetup(CreateJobModel cjm)
        {
            var temp = _datasetContext.DataSourceTypes.Select(v
                => new SelectListItem { Text = v.Name, Value = v.DiscrimatorValue }).ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick a Source Type",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            cjm.SourceTypesDropdown = temp.Where(x =>
                x.Value != GlobalConstants.DataSourceDiscriminator.DEFAULT_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSourceDiscriminator.JAVA_APP_SOURCE &&
                x.Value != GlobalConstants.DataSourceDiscriminator.DEFAULT_HSZ_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSourceDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION).OrderBy(x => x.Value);

            List<SelectListItem> temp2 = new List<SelectListItem>();

            if (cjm.SelectedSourceType != null && cjm.SelectedDataSource != 0)
            {
                temp2 = DataSourcesByType(cjm.SelectedSourceType, cjm.SelectedDataSource);
            }

            temp2.Add(new SelectListItem()
            {
                Text = "Pick a Source Type Above",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            cjm.SourcesForDropdown = temp2.OrderBy(x => x.Value);

            cjm.CompressionTypesDropdown = Enum.GetValues(typeof(CompressionTypes)).Cast<CompressionTypes>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            if (cjm.NewFileNameExclusionList != null)
            {
                cjm.FileNameExclusionList = cjm.NewFileNameExclusionList.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
            }
            else
            {
                cjm.NewFileNameExclusionList = "";
                cjm.FileNameExclusionList = new List<string>();
            }

            cjm.RequestMethodDropdown = Utility.BuildRequestMethodDropdown(cjm.SelectedRequestMethod);

            cjm.RequestDataFormatDropdown = Utility.BuildRequestDataFormatDropdown(cjm.SelectedRequestDataFormat);

            cjm.FtpPatternDropDown = Utility.BuildFtpPatternSelectList(cjm.FtpPattern);

            cjm.SchedulePickerDropdown = Utility.BuildSchedulePickerDropdown(null);

            return cjm;
        }

        private EditJobModel EditDropDownSetup(EditJobModel ejm, RetrieverJob retrieverJob)
        {

            List<SelectListItem> temp;
            List<SelectListItem> temp2;


            if (retrieverJob.DataSource.SourceType == DataSourceDiscriminator.DEFAULT_DROP_LOCATION)
            {
                temp = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == DataSourceDiscriminator.DEFAULT_DROP_LOCATION).Select(v
                   => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.DiscrimatorValue,
                       Disabled = v.DiscrimatorValue == DataSourceDiscriminator.DEFAULT_DROP_LOCATION
                   }).ToList();

                temp2 = DataSourcesByType(ejm.SelectedSourceType, ejm.SelectedDataSource).Where(x => x.Text == retrieverJob.DataSource.Name).ToList();
            }
            else if (retrieverJob.DataSource.SourceType == DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION)
            {
                temp = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION).Select(v
                   => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.DiscrimatorValue,
                       Disabled = v.DiscrimatorValue == DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION
                   }).ToList();

                temp2 = DataSourcesByType(ejm.SelectedSourceType, ejm.SelectedDataSource).Where(x => x.Text == retrieverJob.DataSource.Name).ToList();
            }
            else
            {
                temp = _datasetContext.DataSourceTypes.Select(v
                   => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.DiscrimatorValue,
                       Disabled = v.DiscrimatorValue == DataSourceDiscriminator.DEFAULT_DROP_LOCATION || v.DiscrimatorValue == DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION
                   }).ToList();

                temp.Add(new SelectListItem()
                {
                    Text = "Pick a Source Type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });

                temp2 = DataSourcesByType(ejm.SelectedSourceType, ejm.SelectedDataSource);

                temp2.Add(new SelectListItem()
                {
                    Text = "Pick a Source",
                    Value = "0"
                });
            }

            ejm.SourceTypesDropdown = temp.OrderBy(x => x.Value);
            ejm.SourcesForDropdown = temp2.OrderBy(x => x.Value);

            ejm.ScheduleOptions = Utility.BuildSchedulePickerDropdown(retrieverJob.ReadableSchedule);
            ejm.SchedulePicker = ejm.ScheduleOptions.Where(w => w.Selected).Select(s => Int32.Parse(s.Value)).FirstOrDefault();

            ejm.CompressionTypesDropdown = Enum.GetValues(typeof(CompressionTypes)).Cast<CompressionTypes>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();


            if (ejm.NewFileNameExclusionList != null)
            {
                if (ejm.FileNameExclusionList.Any())
                {
                    //I honestly have no idea where this is being added from.
                    ejm.FileNameExclusionList = ejm.FileNameExclusionList
                        .Union(ejm.NewFileNameExclusionList.Split('|')
                            .Where(x => !String.IsNullOrWhiteSpace(x)).ToList())
                        .Where(x => x != "System.Collections.Generic.List`1[System.String]").ToList();
                }
                else
                {
                    ejm.FileNameExclusionList = ejm.NewFileNameExclusionList.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
                }
            }
            else
            {
                ejm.NewFileNameExclusionList = "";
            }

            ejm.RequestMethodDropdown = Utility.BuildRequestMethodDropdown(ejm.SelectedRequestMethod);

            ejm.RequestDataFormatDropdown = Utility.BuildRequestDataFormatDropdown(ejm.SelectedRequestDataFormat);

            ejm.FtpPatternDropDown = Utility.BuildFtpPatternSelectList(ejm.FtpPattern);

            return ejm;
        }

        private void EditSourceDropDown(DataSourceModel model)
        {
            var temp = _datasetContext.DataSourceTypes.Select(v => new SelectListItem { Text = v.Name, Value = v.DiscrimatorValue }).ToList();

            //set selected for current value
            temp.ForEach(x => x.Selected = model.SourceType.Equals(x.Value));

            model.SourceTypesDropdown = temp.Where(x => x.Value != DataSourceDiscriminator.DEFAULT_DROP_LOCATION && x.Value != DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION && x.Value != DataSourceDiscriminator.JAVA_APP_SOURCE).OrderBy(x => x.Value);

            var temp2 = AuthenticationTypesByType(model.SourceType, int.TryParse(model.AuthID, out int intvalue) ? (int?)intvalue : null);

            model.AuthTypesDropdown = temp2.OrderBy(x => x.Value);
        }

        private List<SelectListItem> GetRequestDataFormat(string sourceType, int? selectedId)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            if (selectedId == null)
            {
                output.Add(new SelectListItem()
                {
                    Text = "Pick a Data Format",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            List<HttpDataFormat> methodList = new List<HttpDataFormat>();

            switch (sourceType)
            {
                case DataSourceDiscriminator.HTTPS_SOURCE:
                    methodList = new HTTPSSource().ValidHttpDataFormats;
                    break;
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    methodList = new GoogleApiSource().ValidHttpDataFormats;
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    methodList = new GoogleBigQueryApiSource().ValidHttpDataFormats;
                    break;
                default:
                    break;
            }

            foreach (HttpDataFormat methodType in methodList)
            {
                output.Add(new SelectListItem { Text = methodType.ToString().ToUpper(), Value = ((int)methodType).ToString(), Selected = selectedId == (int)methodType });
            }

            return output;
        }

        private List<SelectListItem> AuthenticationTypesByType(string sourceType, int? selectedId)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            if (selectedId == null)
            {
                output.Add(new SelectListItem()
                {
                    Text = "Pick an Authentication Type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            List<AuthenticationTypeDto> authenticationTypeDtos = _dataSourceService.GetValidAuthenticationTypeDtosByType(sourceType);
            List<SelectListItem> authenticationTypeItems = authenticationTypeDtos.Select(x => 
                new SelectListItem() 
                { 
                    Text = x.AuthName,
                    Value = x.AuthID.ToString(),
                    Selected = selectedId == x.AuthID
                })
                .ToList();

            output.AddRange(authenticationTypeItems);

            return output;
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
                case DataSourceDiscriminator.FTP_SOURCE:
                    List<DataSource> fTpList = _datasetContext.DataSources.Where(x => x is FtpSource).ToList();
                    output.AddRange(fTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.SFTP_SOURCE:
                    List<DataSource> sfTpList = _datasetContext.DataSources.Where(x => x is SFtpSource).ToList();
                    output.AddRange(sfTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.DEFAULT_DROP_LOCATION:
                    List<DataSource> dfsBasicList = _datasetContext.DataSources.Where(x => x is DfsBasic).ToList();
                    output.AddRange(dfsBasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.DFS_CUSTOM:
                    List<DataSource> dfsCustomList = _datasetContext.DataSources.Where(x => x is DfsCustom).ToList();
                    output.AddRange(dfsCustomList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION:
                    List<DataSource> s3BasicList = _datasetContext.DataSources.Where(x => x is S3Basic).ToList();
                    output.AddRange(s3BasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.HTTPS_SOURCE:
                    List<DataSource> HttpsList = _datasetContext.DataSources.Where(x => x is HTTPSSource).ToList();
                    output.AddRange(HttpsList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    List<DataSource> GApiList = _datasetContext.DataSources.Where(x => x is GoogleApiSource).ToList();
                    output.AddRange(GApiList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    List<DataSource> bigQueryList = _datasetContext.DataSources.Where(x => x is GoogleBigQueryApiSource).ToList();
                    output.AddRange(bigQueryList.Select(v => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                default:
                    throw new NotImplementedException();
            }

            return output;
        }

        private DataSourceModel CreateSourceDropDown(DataSourceModel csm)
        {
            List<SelectListItem> sourceTypeDropdown = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Text = "Pick a Source Type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                }
            };

            List<DataSourceTypeDto> dataSourceTypeDtos = _dataSourceService.GetDataSourceTypeDtosForDropdown();
            List<SelectListItem> dataSourceTypeItems = dataSourceTypeDtos.Select(x => new SelectListItem { Text = x.Name, Value = x.DiscrimatorValue }).ToList();

            sourceTypeDropdown.AddRange(dataSourceTypeItems);

            csm.SourceTypesDropdown = sourceTypeDropdown;

            List<SelectListItem> authTypesDropdown = new List<SelectListItem> {
                new SelectListItem()
                {
                    Text = "Pick an Authentication Type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                }
            };

            List<SelectListItem> authenticationTypeItems;

            if (csm.SourceType == null)
            {
                List<AuthenticationTypeDto> authenticationTypeDtos = _dataSourceService.GetAuthenticationTypeDtos();
                authenticationTypeItems = authenticationTypeDtos.Select(x => new SelectListItem { Text = x.AuthName, Value = x.AuthID.ToString() }).ToList();
            }
            else
            {
                authTypesDropdown.First().Selected = false;
                authenticationTypeItems = AuthenticationTypesByType(csm.SourceType, int.TryParse(csm.AuthID, out int intvalue) ? (int?)intvalue : null);
            }

            authTypesDropdown.AddRange(authenticationTypeItems);
            csm.AuthTypesDropdown = authTypesDropdown;

            return csm;
        }

        private List<SelectListItem> GetRequestMethods(string sourceType, int? selectedId)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            if (selectedId == null)
            {
                output.Add(new SelectListItem()
                {
                    Text = "Pick a Request Method type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            List<HttpMethods> methodList = new List<HttpMethods>();

            switch (sourceType)
            {
                case DataSourceDiscriminator.HTTPS_SOURCE:
                    methodList = new HTTPSSource().ValidHttpMethods;
                    break;
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    methodList = new GoogleApiSource().ValidHttpMethods;
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    methodList = new GoogleBigQueryApiSource().ValidHttpMethods;
                    break;
                default:
                    break;
            }

            foreach (HttpMethods methodType in methodList)
            {
                output.Add(new SelectListItem { Text = methodType.ToString().ToUpper(), Value = ((int)methodType).ToString(), Selected = selectedId == (int)methodType });
            }

            return output;
        }

        protected override void AddCoreValidationExceptionsToModel(Sentry.Core.ValidationException ex)
        {
            foreach (ValidationResult vr in ex.ValidationResults.GetAll())
            {
                switch (vr.Id)
                {
                    case GlobalConstants.ValidationErrors.S3KEY_IS_BLANK:
                        ModelState.AddModelError("Key", vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.NAME_IS_BLANK:
                        ModelState.AddModelError("Name", vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetCreatedByRequired:
                        ModelState.AddModelError("CreationUserName", vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetDateRequired:
                        ModelState.AddModelError("DatasetDate", vr.Description);
                        break;
                    case SFtpSource.SftpValidationErrors.portNumberValueNonZeroValue:
                        ModelState.AddModelError("PortNumber", vr.Description);
                        break;
                    default:
                        ModelState.AddModelError(vr.Id, vr.Description);
                        break;
                }
            }
        }
        #endregion
    }
}