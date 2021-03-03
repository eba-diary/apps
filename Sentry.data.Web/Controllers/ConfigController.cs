﻿using LazyCache;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.Core;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Web.Helpers;
using Sentry.data.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Controllers
{
    public class ConfigController : BaseController
    {
        private readonly IAssociateInfoProvider _associateInfoProvider;
        private readonly IDatasetContext _datasetContext;
        private readonly IConfigService _configService;
        private readonly UserService _userService;
        private IAppCache _cache;
        private readonly IEventService _eventService;
        private readonly IDatasetService _DatasetService;
        private readonly IObsidianService _obsidianService;
        private readonly ISecurityService _securityService;
        private readonly ISchemaService _schemaService;
        private readonly IDataFeatures _featureFlags;
        private string _bucket;
        private string _awsRegion;

        public ConfigController(IDatasetContext dsCtxt, UserService userService, IAssociateInfoProvider associateInfoService,
            IConfigService configService, IEventService eventService, IDatasetService datasetService, 
            IObsidianService obsidianService, ISecurityService securityService, ISchemaService schemaService, 
            IDataFeatures dataFeatures)
        {
            _cache = new CachingService();
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
        }

        private string RootBucket
        {
            get
            {
                if (_bucket == null)
                {
                    _bucket = Config.GetHostSetting("AWS2_0RootBucket");
                }
                return _bucket;
            }
        }

        private string AwsRegion
        {
            get
            {
                if (_awsRegion == null)
                {
                    _awsRegion = Config.GetHostSetting("AWS2_0Region");
                }
                return _awsRegion;
            }
        }

        [HttpGet]
        [Route("Config/Dataset/{id}")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult Index(int id)
        {

            Dataset ds = _datasetContext.GetById(id);

            UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());

            if (us != null && us.CanManageSchema)
            {
                DatasetDto dsDto = _DatasetService.GetDatasetDto(id);

                List<DatasetFileConfigsModel> configModelList = new List<DatasetFileConfigsModel>();
                foreach (DatasetFileConfig config in ds.DatasetFileConfigs)
                {
                    DatasetFileConfigsModel model = new DatasetFileConfigsModel(config, true, false);

                    model.RetrieverJobs = config.RetrieverJobs;
                    Tuple<DataFlowDetailDto, List<RetrieverJob>> jobs2 = _configService.GetDataFlowForSchema(config);
                    model.DataFlowJobs = (jobs2.Item1 != null) ? jobs2.ToModel() : null;

                    model.ExternalDataFlowJobs = _configService.GetExternalDataFlowsBySchema(config).ToModel();
                    configModelList.Add(model);
                }

                ManageConfigsModel mcm = new ManageConfigsModel()
                {
                    DatasetId = dsDto.DatasetId,
                    DatasetName = dsDto.DatasetName,
                    CategoryColor = dsDto.CategoryColor,
                    DatasetFileConfigs = configModelList,
                    DisplayDataflowMetadata = _featureFlags.Expose_Dataflow_Metadata_CLA_2146.GetValue()
                };

                mcm.Security = _DatasetService.GetUserSecurityForDataset(id);

                Event e = new Event
                {
                    EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                    Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                    TimeCreated = DateTime.Now,
                    TimeNotified = DateTime.Now,
                    IsProcessed = false,
                    Dataset = ds.DatasetId,
                    UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                    Reason = "Viewed Dataset Configuration Page"
                };
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return View("Index", mcm);
            }
            else
            {
                return View("Unauthorized");
            }
        }

        [HttpGet]
        [Route("Config/Dataset/{id}/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult Create(int id)
        {
            Dataset parent = _datasetContext.GetById<Dataset>(id);

            DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel
            {
                DatasetId = id,
                ParentDatasetName = parent.DatasetName,
                AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext),
                AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList(),
                ExtensionList = Utility.GetFileExtensionListItems(_datasetContext),
                Security = _securityService.GetUserSecurity(null, SharedContext.CurrentUser)
            };

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Configuration Creation Page", dfcm.DatasetId);

            return View(dfcm);
        }

        [HttpGet]
        public PartialViewResult _DatasetFileConfigCreate(int id)
        {
            Dataset parent = _datasetContext.GetById<Dataset>(id);

            DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel
            {
                DatasetId = id,
                ParentDatasetName = parent.DatasetName,
                AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext),
                AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList(),
                ExtensionList = Utility.GetFileExtensionListItems(_datasetContext),
                Security = _securityService.GetUserSecurity(null, SharedContext.CurrentUser)
            };

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Configuration Creation Page", dfcm.DatasetId);

            return PartialView("_DatasetFileConfigCreate", dfcm);
        }

        [HttpPost]
        [Route("Config/DatasetFileConfigForm")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult DatasetFileConfigForm(DatasetFileConfigsModel dfcm)
        {
            DatasetFileConfigDto dto = dfcm.ToDto();

            if (dto.ConfigId == 0)
            {
                AddCoreValidationExceptionsToModel(_configService.Validate(dto));
            }

            if (ModelState.IsValid)
            {
                FileSchemaDto schemaDto = dfcm.ToSchema();

                if (dto.ConfigId == 0)
                { //Create Dataset File Config
                    bool IsSuccessful = false;
                    int newSchemaId = _schemaService.CreateAndSaveSchema(schemaDto);
                    if (newSchemaId != 0)
                    {
                        dto.SchemaId = newSchemaId;
                        IsSuccessful = _configService.CreateAndSaveDatasetFileConfig(dto);
                    }

                    if (IsSuccessful)
                    {
                        //return RedirectToAction("Index", new { id = dto.ParentDatasetId });
                        return Json(new { Success = true, dataset_id = dto.ParentDatasetId, schema_id = dto.SchemaId });
                    }
                }
                else
                { //Edit Dataset File Config
                    bool IsSuccessful = false;
                    if (_schemaService.UpdateAndSaveSchema(schemaDto))
                    {
                        IsSuccessful = _configService.UpdateAndSaveDatasetFileConfig(dto);
                    }

                    if (IsSuccessful)
                    {
                        return RedirectToAction("Index", new { id = dto.ParentDatasetId });
                    }
                }
            }

            dfcm.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext);
            dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            dfcm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext);
            dfcm.Security = _securityService.GetUserSecurity(null, SharedContext.CurrentUser);

            if (dto.ConfigId == 0)
            {
                return PartialView("_DatasetFileConfigCreate", dfcm);
                //return View("Create", dfcm);
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
                    bool IsDeleted = _configService.Delete(id);
                    if (!IsDeleted)
                    {
                        return Json(new { Success = false, Message = "Schema was not deleted" });
                    }
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.DELETE_DATASET_SCHEMA, SharedContext.CurrentUser.AssociateId, "Deleted Dataset Schema", id);
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

            Event e = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Configuration Management Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

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
                    return View("Unauthorized");
                }

                DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel(dto);

                dfcm.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext, dfcm.DatasetScopeTypeID);
                dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
                dfcm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, dfcm.FileExtensionID);

                //ViewBag.ModifyType = "Edit";

                _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Configuration Edit Page", dfcm.DatasetId);

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
            edfc.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, edfc.FileExtensionID);

            ViewBag.ModifyType = "Edit";

            return PartialView("_EditConfigFile", edfc);
        }

        //[HttpPost]
        //[Route("Config/Edit/{configId}")]
        //[AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        //public ActionResult Edit(EditDatasetFileConfigModel edfc)
        //{
        //    DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(edfc.ConfigId);

        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            dfc.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(edfc.DatasetScopeTypeID);
        //            dfc.FileTypeId = edfc.FileTypeId;
        //            dfc.Description = edfc.ConfigFileDesc;
        //            dfc.FileExtension = _datasetContext.GetById<FileExtension>(edfc.FileExtensionID);
        //            _datasetContext.SaveChanges();

        //            return RedirectToAction("Index", new { id = edfc.DatasetId });
        //        }
        //    }
        //    catch (Sentry.Core.ValidationException ex)
        //    {
        //        AddCoreValidationExceptionsToModel(ex);
        //        _datasetContext.Clear();
        //    }

        //    return View(edfc);
        //}

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

            Event e = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                DataConfig = dfc.ConfigId,
                Dataset = dfc.ParentDataset.DatasetId,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Retrieval Creation Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


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
                        JobOptions = rjo
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
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_S3_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSoureDiscriminator.JAVA_APP_SOURCE &&
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_HSZ_DROP_LOCATION &&
                x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION).OrderBy(x => x.Value);

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

            Event e = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Retrieval Edit Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


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

        private EditJobModel EditDropDownSetup(EditJobModel ejm, RetrieverJob retrieverJob)
        {

            List<SelectListItem> temp;
            List<SelectListItem> temp2;


            if (retrieverJob.DataSource.SourceType == "DFSBasic")
            {
                temp = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == "DFSBasic").Select(v
                   => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.DiscrimatorValue,
                       Disabled = v.DiscrimatorValue == "DFSBasic"
                   }).ToList();

                temp2 = DataSourcesByType(ejm.SelectedSourceType, ejm.SelectedDataSource).Where(x => x.Text == retrieverJob.DataSource.Name).ToList();
            }
            else if (retrieverJob.DataSource.SourceType == "S3Basic")
            {
                temp = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == "S3Basic").Select(v
                   => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.DiscrimatorValue,
                       Disabled = v.DiscrimatorValue == "S3Basic"
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
                       Disabled = v.DiscrimatorValue == "DFSBasic" || v.DiscrimatorValue == "S3Basic"
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


        [HttpGet]
        [Route("Config/Source/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateSource()
        {
            DataSourceModel dsm = new DataSourceModel();

            dsm = CreateSourceDropDown(dsm);

            Event e = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Data Source Creation Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View("CreateDataSource", dsm);
        }

        public ActionResult HeaderEntryRow()
        {
            return PartialView("_Headers");
        }

        public ActionResult FieldEntryRow()
        {
            return PartialView("_Fields", new FieldRowModel());
        }

        public ActionResult ExtensionEntryRow()
        {
            return PartialView("_ExtensionMapping");
        }

        private DataSourceModel CreateSourceDropDown(DataSourceModel csm)
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

            csm.SourceTypesDropdown = temp.Where(x =>
                    x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_DROP_LOCATION &&
                    x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_S3_DROP_LOCATION &&
                    x.Value != GlobalConstants.DataSoureDiscriminator.JAVA_APP_SOURCE &&
                    x.Value != GlobalConstants.DataSoureDiscriminator.DEFAULT_HSZ_DROP_LOCATION).OrderBy(x => x.Value);

            if (csm.SourceType == null)
            {
                var temp2 = _datasetContext.AuthTypes.Select(v
             => new SelectListItem { Text = v.AuthName, Value = v.AuthID.ToString() }).ToList();

                temp2.Add(new SelectListItem()
                {
                    Text = "Pick a Authentication Type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });

                csm.AuthTypesDropdown = temp2.OrderBy(x => x.Value);
            }
            else
            {
                var temp2 = AuthenticationTypesByType(csm.SourceType, Int32.TryParse(csm.AuthID, out int intvalue) ? (int?)intvalue : null);
                temp2.Add(new SelectListItem()
                {
                    Text = "Pick a Authentication Type",
                    Value = "0",
                    Selected = false,
                    Disabled = true
                });

                csm.AuthTypesDropdown = temp2.OrderBy(x => x.Value);
            }
            return csm;
        }

        [HttpPost]
        [Route("Config/DataSourceForm")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult DataSourceForm(DataSourceModel model)
        {
            DataSourceDto dto = model.ToDto();

            AddCoreValidationExceptionsToModel(_configService.Validate(dto));

            if (ModelState.IsValid)
            {
                if (dto.OriginatingId == 0)
                {
                    bool IsSuccessful = _configService.CreateAndSaveNewDataSource(dto);
                    if (IsSuccessful)
                    {
                        _eventService.PublishSuccessEvent(GlobalConstants.EventType.CREATED_DATASOURCE, SharedContext.CurrentUser.AssociateId, dto.Name + " was created.");

                        return !String.IsNullOrWhiteSpace(model.ReturnUrl) 
                            ? Redirect(model.ReturnUrl) 
                            : Redirect("/");
                    }
                }
                else
                {
                    bool IsSuccessful = _configService.UpdateAndSaveDataSource(dto);
                    if (IsSuccessful)
                    {
                        _eventService.PublishSuccessEvent(GlobalConstants.EventType.UPDATED_DATASOURCE, SharedContext.CurrentUser.AssociateId, dto.Name + " was updated.");
                        return !String.IsNullOrWhiteSpace(model.ReturnUrl) 
                            ? Redirect(model.ReturnUrl) 
                            : Redirect("/");
                    }
                }
            }


            if (model.Id == 0)
            {
                _datasetContext.Clear();
                model = CreateSourceDropDown(model);
                return View("CreateDataSource", model);
            }
            else
            {
                _datasetContext.Clear();
                EditSourceDropDown(model);
                return View("EditDataSource", model);
            }
        }

        [HttpGet]
        [Route("Config/Source/Edit/{sourceID}")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult EditSource(int sourceID)
        {
            UserSecurity us = _configService.GetUserSecurityForDataSource(sourceID);
            if (us != null && us.CanEditDataSource)
            {
                DataSourceDto dto = _configService.GetDataSourceDto(sourceID);
                DataSourceModel model = new DataSourceModel(dto);

                EditSourceDropDown(model);

                _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Data Source Edit Page");

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

        private void EditSourceDropDown(DataSourceModel model)
        {
            var temp = _datasetContext.DataSourceTypes.Select(v
              => new SelectListItem { Text = v.Name, Value = v.DiscrimatorValue }).ToList();

            //set selected for current value
            temp.ForEach(x => x.Selected = model.SourceType.Equals(x.Value));

            model.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic" && x.Value != "S3Basic" && x.Value != "JavaApp").OrderBy(x => x.Value);

            var temp2 = AuthenticationTypesByType(model.SourceType, Int32.TryParse(model.AuthID, out int intvalue) ? (int?)intvalue : null);

            model.AuthTypesDropdown = temp2.OrderBy(x => x.Value);
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Config/SourcesByType/")]
        public JsonResult SourcesByType(string sourceType)
        {
            return Json(DataSourcesByType(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
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
                case "HTTPS":
                    HTTPSSource https = new HTTPSSource();
                    methodList = https.ValidHttpMethods;
                    break;
                case "GOOGLEAPI":
                    GoogleApiSource gapi = new GoogleApiSource();
                    methodList = gapi.ValidHttpMethods;
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

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Config/RequestDataFormatByType/{sourceType}")]
        public JsonResult RequestDataFormatByType(string sourceType)
        {
            return Json(GetRequestDataFormat(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> GetRequestDataFormat(string sourceType, int? selectedId)
        {
            List<SelectListItem> output = new List<SelectListItem>();

            if (selectedId == null)
            {
                output.Add(new SelectListItem()
                {
                    Text = "Pick a Request Data Format type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            List<HttpDataFormat> methodList = new List<HttpDataFormat>();

            switch (sourceType)
            {
                case "HTTPS":
                    HTTPSSource https = new HTTPSSource();
                    methodList = https.ValidHttpDataFormats;
                    break;
                case "GOOGLEAPI":
                    GoogleApiSource gapi = new GoogleApiSource();
                    methodList = gapi.ValidHttpDataFormats;
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
                    Text = "Pick a Authentication Type",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            switch (sourceType)
            {
                case "FTP":
                    FtpSource ftp = new FtpSource();
                    foreach (AuthenticationType authtype in ftp.ValidAuthTypes)
                    {
                        output.Add(GetAuthSelectedListItem(authtype, selectedId));
                    }
                    break;
                case "SFTP":
                    SFtpSource sftp = new SFtpSource();
                    foreach (AuthenticationType authtype in sftp.ValidAuthTypes)
                    {
                        output.Add(GetAuthSelectedListItem(authtype, selectedId));
                    }
                    break;
                case "DFSCustom":
                    DfsCustom dfscust = new DfsCustom();
                    foreach (AuthenticationType authtype in dfscust.ValidAuthTypes)
                    {
                        output.Add(GetAuthSelectedListItem(authtype, selectedId));
                    }
                    break;
                case "HTTPS":
                    HTTPSSource https = new HTTPSSource();
                    foreach (AuthenticationType authtype in https.ValidAuthTypes)
                    {
                        output.Add(GetAuthSelectedListItem(authtype, selectedId));
                    }
                    break;
                case "GOOGLEAPI":
                    GoogleApiSource gapi = new GoogleApiSource();
                    foreach (AuthenticationType authtype in gapi.ValidAuthTypes)
                    {
                        output.Add(GetAuthSelectedListItem(authtype, selectedId));
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return output;

        }

        private SelectListItem GetAuthSelectedListItem(AuthenticationType authtype, int? selectedId)
        {
            if (authtype.Is<BasicAuthentication>())
            {
                AuthenticationType auth = _datasetContext.AuthTypes.Where(w => w is BasicAuthentication).First();
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID };
            }
            else if (authtype.Is<AnonymousAuthentication>())
            {
                AuthenticationType auth = _datasetContext.AuthTypes.Where(w => w is AnonymousAuthentication).First();
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID };
            }
            else if (authtype.Is<TokenAuthentication>())
            {
                AuthenticationType auth = _datasetContext.AuthTypes.Where(w => w is TokenAuthentication).First();
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID };
            }
            else if (authtype.Is<OAuthAuthentication>())
            {
                AuthenticationType auth = _datasetContext.AuthTypes.Where(w => w is OAuthAuthentication).First();
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID };
            }
            else
            {
                throw new NotImplementedException();
            }
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
                    List<DataSource> fTpList = _datasetContext.DataSources.Where(x => x is FtpSource).ToList();
                    output.AddRange(fTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "SFTP":
                    List<DataSource> sfTpList = _datasetContext.DataSources.Where(x => x is SFtpSource).ToList();
                    output.AddRange(sfTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "DFSBasic":
                    List<DataSource> dfsBasicList = _datasetContext.DataSources.Where(x => x is DfsBasic).ToList();
                    output.AddRange(dfsBasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "DFSCustom":
                    List<DataSource> dfsCustomList = _datasetContext.DataSources.Where(x => x is DfsCustom).ToList();
                    output.AddRange(dfsCustomList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "S3Basic":
                    List<DataSource> s3BasicList = _datasetContext.DataSources.Where(x => x is S3Basic).ToList();
                    output.AddRange(s3BasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "HTTPS":
                    List<DataSource> HttpsList = _datasetContext.DataSources.Where(x => x is HTTPSSource).ToList();
                    output.AddRange(HttpsList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                case "GOOGLEAPI":
                    List<DataSource> GApiList = _datasetContext.DataSources.Where(x => x is GoogleApiSource).ToList();
                    output.AddRange(GApiList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id }).ToList());
                    break;
                default:
                    throw new NotImplementedException();
            }

            return output;
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

        public JsonResult AuthTypeDescription(int AuthID)
        {
            var temp = _datasetContext.AuthTypes.Where(x => x.AuthID == AuthID).Select(x => x.Description).FirstOrDefault();

            return Json(temp, JsonRequestBehavior.AllowGet);
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
                    case GlobalConstants.ValidationErrors.CREATION_USER_NAME_IS_BLANK:
                        ModelState.AddModelError("CreationUserName", vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.DATASET_DATE_IS_OLD:
                        ModelState.AddModelError("DatasetDate", vr.Description);
                        break;
                    case SFtpSource.ValidationErrors.portNumberValueNonZeroValue:
                        ModelState.AddModelError("PortNumber", vr.Description);
                        break;
                    default:
                        ModelState.AddModelError(vr.Id, vr.Description);
                        break;
                }
            }
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
                    return View("Unauthorized");
                }

                if (configDto.Schema.SchemaId == schemaId)
                {
                    ObsoleteDatasetModel bdm = new ObsoleteDatasetModel(_datasetContext.GetById<Dataset>(configDto.ParentDatasetId), _associateInfoProvider, _datasetContext)
                    {
                        CanEditDataset = us.CanEditDataset,
                        CanUpload = us.CanUploadToDataset,
                        Security = us
                    };

                    Event e = new Event();
                    e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
                    e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.DataConfig = configDto.ConfigId;
                    e.Dataset = configDto.ParentDatasetId;
                    e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                    e.Reason = "Viewed Edit Fields";
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

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
                return View("Unauthorized");
            }
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/{schemaId}/UpdateFields")]
        public JsonResult UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows)
        {
            try
            {
                List<BaseFieldDto> schemaRowsDto = schemaRows.ToDto();

                _schemaService.Validate(schemaId, schemaRowsDto);
                _schemaService.CreateAndSaveSchemaRevision(schemaId, schemaRowsDto, "blah");

            }
            catch (SchemaUnauthorizedAccessException)
            {
                return Json(new { Success = false, Message = "Unauthorized access"}, JsonRequestBehavior.AllowGet);
            }
            catch (ValidationException vEx)
            {
                return Json(new { Success = false, Message = "Failed schema validation", Errors = vEx.ValidationResults.GetAll() }, JsonRequestBehavior.AllowGet);
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
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public JsonResult ValidateField(int schemaId, SchemaRow schemaRow)
        {
            try
            {
                List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
                dtoList.Add(schemaRow.ToDto(false));

                _schemaService.Validate(schemaId, dtoList);

                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (ValidationException vEx)
            {
                return Json(new { Success = false, Message = "Failed schema validation", Errors = vEx.ValidationResults.GetAll() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Failed to validate schema rows" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Route("Config/{configId}/Schema/Create")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult CreateSchema(int configId)
        {
            //throw new NotImplementedException();
            //NEED To revisit with merging of metadata repository tables

            //Schema schema = _datasetContext.GetById<Schema>(schemaId);

            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);


            CreateSchemaModel createSchemaModel = new CreateSchemaModel()
            {
                DatasetId = dfc.ParentDataset.DatasetId
            };

            Event e = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Retrieval Edit Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

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

            DataElement maxSchemaRevision = dfc.Schemas.OrderByDescending(o => o.SchemaRevision).FirstOrDefault();
            //Get raw file storage code
            string storageCode = dfc.GetStorageCode();

            try
            {
                if (ModelState.IsValid)
                {
                    DataElement de = new DataElement()
                    {
                        DataElementCreate_DTM = DateTime.Now,
                        DataElementChange_DTM = DateTime.Now,
                        LastUpdt_DTM = DateTime.Now,
                        DataElement_CDE = "F",
                        DataElementCode_DSC = GlobalConstants.DataElementDescription.DATA_FILE,
                        DataElement_NME = csm.Name,
                        DataElement_DSC = csm.Description,
                        DatasetFileConfig = dfc,
                        Delimiter = csm.Delimiter,
                        HasHeader = csm.HasHeader,
                        SchemaName = csm.Name,
                        SchemaDescription = csm.Description,
                        SchemaIsForceMatch = csm.IsForceMatch,
                        SchemaIsPrimary = true,
                        SchemaRevision = (maxSchemaRevision == null) ? 0 : maxSchemaRevision.SchemaRevision + 1,
                        StorageCode = storageCode,
                        HiveDatabase = "Default",
                        HiveTable = dfc.ParentDataset.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dfc.Name.Replace(" ", "").ToUpper(),
                        HiveTableStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString(),
                        HiveLocation = RootBucket + "/" + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode
                    };

                    dfc.Schemas.Add(de);

                    if (maxSchemaRevision != null)
                    {
                        maxSchemaRevision.SchemaIsPrimary = false;
                    }

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
                    return View("Unauthorized");
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

                    Event e = new Event
                    {
                        EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                        Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                        TimeCreated = DateTime.Now,
                        TimeNotified = DateTime.Now,
                        IsProcessed = false,
                        UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                        Reason = "Viewed Retrieval Edit Page"
                    };
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                    return View(editSchemaModel);
                }
                else
                {
                    return RedirectToAction("Index", new { id = config.ParentDatasetId });
                }
            }
            catch (SchemaUnauthorizedAccessException)
            {
                return View("Unauthorized");
            }
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/{schemaId}/Edit")]
        public ActionResult EditSchema(int configId, int schemaId, EditSchemaModel esm)
        {
            UserSecurity us = _schemaService.GetUserSecurityForSchema(schemaId);

            if (!us.CanManageSchema)
            {
                return View("Unauthorized");
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
                return View("Unauthorized");
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
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public JsonResult IsHttpSource(int dataSourceId)
        {
            if (dataSourceId == 0)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            DataSourceDto dto = _configService.GetDataSourceDto(dataSourceId);
            bool result;
            switch (dto.SourceType)
            {
                case GlobalConstants.DataSoureDiscriminator.GOOGLE_API_SOURCE:
                case GlobalConstants.DataSoureDiscriminator.HTTPS_SOURCE:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SourceDetails (int Id)
        {            
            DataSourceDto dto = _configService.GetDataSourceDto(Id);
            DataSourceModel model = new DataSourceModel(dto);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DataSourceAccessRequest(int dataSourceId)
        {
            DataSourceAccessRequestModel model = _configService.GetDataSourceAccessRequest(dataSourceId).ToDataSourceModel();
            model.AllAdGroups = _obsidianService.GetAdGroups("").Select(x => new SelectListItem() { Text = x, Value = x }).ToList();
            return PartialView("DataSourceAccessRequest", model);
        }

        [HttpPost]
        public ActionResult SubmitAccessRequest(DataSourceAccessRequestModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = _configService.RequestAccessToDataSource(ar);

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
                case "CSV":
                    model.IsPositional = true;
                    model.IsFixedWidth = false;
                    break;
                case "ANY":
                case "DELIMITED":
                case "TXT":
                    model.IsPositional = false;
                    model.IsFixedWidth = false;
                    break;
                case "FIXEDWIDTH":
                    model.IsPositional = true;
                    model.IsFixedWidth = true;
                    break;
                case "JSON":
                    model.IsPositional = false;
                    model.IsFixedWidth = false;
                    model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.STRUCT, "A struct", "Complex Data Types"));
                    break;
                default:
                    break;
            }

            //Common datatypes across all FileExtensions
            model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.VARCHAR, "A varying-length character string.", "String Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.INTEGER, "A signed four-byte integer.", "Numeric Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.BIGINT, "A signed eight-byte integer, from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807.", "Numeric Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.DECIMAL, "A fixed-point decimal number, with 38 digits precision.", "Numeric Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.DATE, "An ANSI SQL date type. YYYY-MM-DD", "Date Time Data Types"));
            model.ValidDatatypes.Add(new DataTypeModel(GlobalConstants.Datatypes.TIMESTAMP, "A UNIX timestamp with optional nanosecond precision. YYYY-MM-DD HH:MM:SS.sss", "Date Time Data Types"));

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _RetrieverJob(int jobId)
        {
            RetrieverJob job = _datasetContext.GetById<RetrieverJob>(jobId);

            ViewData["EnableJobControls"] = "false";
            ViewData["Color"] = "blue";
            return PartialView("~/Views/RetrieverJob/_RetrieverJob.cshtml", job);
        }
    }
}