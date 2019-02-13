using LazyCache;
using Sentry.Core;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Helpers;
using Sentry.data.Web.Models;
using Sentry.data.Web.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Sentry.Common.Logging;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Controllers
{
    public class ConfigController : BaseController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        public IConfigService _configService;
        private UserService _userService;
        private S3ServiceProvider _s3Service;
        private ISASService _sasService;
        private IAppCache _cache;
        public IEventService _eventService;


        public ConfigController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, 
            ISASService sasService, IAssociateInfoProvider associateInfoService, IConfigService configService,
            IEventService eventService)
        {
            _cache = new CachingService();
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            _associateInfoProvider = associateInfoService;
            _configService = configService;
            _eventService = eventService;
        }

        [HttpGet]
        [Route("Config/Dataset/{id}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Index(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            ObsoleteDatasetModel bdm = new ObsoleteDatasetModel(ds, _associateInfoProvider, _datasetContext)
            {
                CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive,
                CanEditDataset = SharedContext.CurrentUser.CanEditDataset,
                CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs,
                CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive,
                CanUpload = SharedContext.CurrentUser.CanUpload
            };

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.Dataset = ds.DatasetId;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Dataset Configuration Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(bdm);
        }

        [HttpGet]
        [Route("Config/Dataset/{id}/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Create(int id)
        {
            Dataset parent = _datasetContext.GetById<Dataset>(id);

            DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel();
            dfcm.DatasetId = id;
            dfcm.ParentDatasetName = parent.DatasetName;

            dfcm.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext);
            dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                    => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            dfcm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.Dataset = parent.DatasetId;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Configuration Creation Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(dfcm);
        }

        [HttpPost]
        [Route("Config/Dataset/{id}/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Create(DatasetFileConfigsModel dfcm)
        {

            Dataset parent = _datasetContext.GetById<Dataset>(dfcm.DatasetId);

            if (parent.DatasetFileConfigs.Any(x => x.Name.ToLower() == dfcm.ConfigFileName.ToLower()))
            {
                AddCoreValidationExceptionsToModel(new ValidationException("Dataset config with that name already exists within dataset"));
            }


            try
            {
                if (ModelState.IsValid)
                {
                    List<DatasetFileConfig> dfcList = parent.DatasetFileConfigs.ToList();

                    List<DataElement> deList = new List<DataElement>();
                    DataElement de = CreateNewDataElement(dfcm);

                    //Create Generic Data File Config for Dataset                    
                    DatasetFileConfig dfc = new DatasetFileConfig()
                    {
                        ConfigId = 0,
                        Name = dfcm.ConfigFileName,
                        Description = dfcm.ConfigFileDesc,
                        //DropPath = dfcm.DropPath,
                        FileTypeId = dfcm.FileTypeId,
                        //IsGeneric = false,
                        ParentDataset = parent,
                        FileExtension = _datasetContext.GetById<FileExtension>(dfcm.FileExtensionID),
                        DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dfcm.DatasetScopeTypeID),
                        Schema = deList
                    };

                    de.DatasetFileConfig = dfc;
                    deList.Add(de);
                    dfc.Schema = deList;

                    List<RetrieverJob> jobList = new List<RetrieverJob>();

                    RetrieverJob rj = Utility.InstantiateJobsForCreation(dfc, _datasetContext.DataSources.First(x => x.Name.Contains("Default Drop Location")));
                    jobList.Add(rj);

                    jobList.Add(Utility.InstantiateJobsForCreation(dfc, _datasetContext.DataSources.First(x => x.Name.Contains("Default S3 Drop Location"))));

                    dfc.RetrieverJobs = jobList;

                    dfcList.Add(dfc);
                    parent.DatasetFileConfigs = dfcList;

                    _datasetContext.Merge<Dataset>(parent);
                    _datasetContext.SaveChanges();

                    try
                    {
                        if (!System.IO.Directory.Exists(rj.GetUri().LocalPath))
                        {
                            System.IO.Directory.CreateDirectory(rj.GetUri().LocalPath);
                        }
                    }
                    catch (Exception e)
                    {

                        StringBuilder errmsg = new StringBuilder();
                        errmsg.AppendLine("Failed to Create Drop Location:");
                        errmsg.AppendLine($"DatasetId: {parent.DatasetId}");
                        errmsg.AppendLine($"DatasetName: {parent.DatasetName}");
                        errmsg.AppendLine($"DropLocation: {rj.GetUri().LocalPath}");

                        Sentry.Common.Logging.Logger.Error(errmsg.ToString(), e);
                    }

                    return RedirectToAction("Index", new { id = parent.DatasetId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _datasetContext.Clear();
                dfcm.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext);
                dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                        => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            }

            return View(dfcm);
        }

        private DataElement CreateNewDataElement(DatasetFileConfigsModel dfcm)
        {
            List<DataElementDetail> details = new List<DataElementDetail>();

            Dataset ds = _datasetContext.GetById<Dataset>(dfcm.DatasetId);

            string storageCode = _datasetContext.GetNextStorageCDE().ToString();
            DataElement de = new DataElement()
            {
                DataElementCreate_DTM = DateTime.Now,
                DataElementChange_DTM = DateTime.Now,
                DataElement_CDE = "F",
                DataElement_DSC = GlobalConstants.DataElementDescription.DATA_FILE,
                DataElement_NME = dfcm.ConfigFileName,
                LastUpdt_DTM = DateTime.Now,
                SchemaIsPrimary = true,
                SchemaDescription = dfcm.ConfigFileDesc,
                SchemaName = dfcm.ConfigFileName,
                SchemaRevision = 1,
                SchemaIsForceMatch = false,
                Delimiter = dfcm.Delimiter,
                HasHeader = dfcm.HasHeader,
                FileFormat = _datasetContext.GetById<FileExtension>(dfcm.FileExtensionID).Name.Trim(),
                StorageCode = storageCode,
                HiveDatabase = "Default",
                HiveTable = ds.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dfcm.ConfigFileName.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                HiveLocation = Configuration.Config.GetHostSetting("AWSRootBucket") + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode
            };

            return de;
        }

        [HttpGet]
        [Route("Config/Manage")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Manage()
        {
            DatasetFileConfigsModel edfc = new DatasetFileConfigsModel();

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Configuration Management Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(edfc);
        }

        [HttpGet]
        [Route("Config/Edit/{configId}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Edit(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.getDatasetFileConfigs(configId);
            EditDatasetFileConfigModel edfc = new EditDatasetFileConfigModel(dfc);
            edfc.DatasetId = dfc.ParentDataset.DatasetId;

            edfc.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext, edfc.DatasetScopeTypeID);
            edfc.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            edfc.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, edfc.FileExtensionID);

            ViewBag.ModifyType = "Edit";

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.DataConfig = dfc.ConfigId;
            e.Dataset = dfc.ParentDataset.DatasetId;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Configuration Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(edfc);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult GetEditConfigPartialView(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.getDatasetFileConfigs(configId);
            EditDatasetFileConfigModel edfc = new EditDatasetFileConfigModel(dfc);
            edfc.DatasetId = dfc.ParentDataset.DatasetId;
            edfc.AllDatasetScopeTypes = Utility.GetDatasetScopeTypesListItems(_datasetContext, edfc.DatasetScopeTypeID);
            edfc.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            edfc.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext, edfc.FileExtensionID);

            ViewBag.ModifyType = "Edit";

            return PartialView("_EditConfigFile", edfc);
        }

        [HttpPost]
        [Route("Config/Edit/{configId}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Edit(EditDatasetFileConfigModel edfc)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(edfc.ConfigId);

            try
            {
                if (ModelState.IsValid)
                {
                    dfc.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(edfc.DatasetScopeTypeID);
                    dfc.FileTypeId = edfc.FileTypeId;
                    dfc.Description = edfc.ConfigFileDesc;
                    dfc.FileExtension = _datasetContext.GetById<FileExtension>(edfc.FileExtensionID);
                    _datasetContext.SaveChanges();

                    return RedirectToAction("Index", new { id = edfc.DatasetId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }

            return View(edfc);
        }

        [HttpGet]
        [Route("Config/{configId}/Job/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateRetrievalJob(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);

            ViewBag.DatasetId = dfc.ParentDataset.DatasetId;
            CreateJobModel cjm = new CreateJobModel(dfc.ConfigId, dfc.ParentDataset.DatasetId);

            cjm = CreateDropDownSetup(cjm);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.DataConfig = dfc.ConfigId;
            e.Dataset = dfc.ParentDataset.DatasetId;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Retrieval Creation Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


            return View(cjm);
        }

        [HttpPost]
        [Route("Config/{configId}/Job/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateRetrievalJob(CreateJobModel cjm)
        {
            try
            {
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
                        CompressionOptions = compression
                    };

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

            cjm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic").Where(x => x.Value != "S3Basic" && x.Value != "JavaApp").OrderBy(x => x.Value);

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


            return cjm;
        }


        [HttpGet]
        [Route("Config/{configId}/Job/Edit/{jobId}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditRetrievalJob(int configId, int jobId)
        {
            RetrieverJob retrieverJob = _datasetContext.GetById<RetrieverJob>(jobId);

            EditJobModel ejm = new EditJobModel(retrieverJob);

            ejm.SelectedDataSource = retrieverJob.DataSource.Id;
            ejm.SelectedSourceType = retrieverJob.DataSource.SourceType;

            ejm = EditDropDownSetup(ejm, retrieverJob);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Retrieval Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


            return View(ejm);
        }

        [HttpPost]
        [Route("Config/{configId}/Job/Edit/{jobId}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditRetrievalJob(EditJobModel ejm)
        {
            RetrieverJob rj = _datasetContext.GetById<RetrieverJob>(ejm.JobID);

            try
            {
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

                    rj.JobOptions = new RetrieverJobOptions()
                    {
                        OverwriteDataFile = ejm.OverwriteDataFile,
                        TargetFileName = ejm.TargetFileName,
                        CreateCurrentFile = ejm.CreateCurrentFile,
                        IsRegexSearch = ejm.IsRegexSearch,
                        SearchCriteria = ejm.SearchCriteria,
                        CompressionOptions = compression
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
                       Disabled = v.DiscrimatorValue == "DFSBasic" ? true : false
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
                       Disabled = v.DiscrimatorValue == "S3Basic" ? true : false
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
                       Disabled = v.DiscrimatorValue == "DFSBasic" || v.DiscrimatorValue == "S3Basic" ? true : false
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

            string[] schedules = new string[5] { "Hourly", "Daily", "Weekly", "Monthly", "Yearly" };

            List<SelectListItem> ScheduleOptions = new List<SelectListItem>();
            int counter = 1;

            ScheduleOptions.Add(new SelectListItem()
            {
                Text = "Pick a Schedule",
                Value = "0",
                Selected = false,
                Disabled = true
            });



            foreach (string s in schedules)
            {
                ScheduleOptions.Add(new SelectListItem()
                {
                    Text = s,
                    Value = counter.ToString(),
                    Selected = retrieverJob.ReadableSchedule == s ? true : false,
                    Disabled = false
                });

                if (retrieverJob.ReadableSchedule == s)
                {
                    ejm.SchedulePicker = counter;
                }

                counter++;
            }

            ejm.ScheduleOptions = ScheduleOptions;

            ejm.CompressionTypesDropdown = Enum.GetValues(typeof(CompressionTypes)).Cast<CompressionTypes>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();


            List<string> a = new List<string>();



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
                //ejm.FileNameExclusionList = new List<string>();
            }

            return ejm;
        }


        [HttpGet]
        [Route("Config/Source/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateSource()
        {
            CreateSourceModel csm = new CreateSourceModel();

            csm = CreateSourceDropDown(csm);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Data Source Creation Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View("CreateDataSource", csm);
        }

        [HttpPost]
        [Route("Config/Source/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateSource(CreateSourceModel csm)
        {
            DataSource source = null;
            try
            {

                AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(csm.AuthID));

                switch (csm.SourceType)
                {
                    case "DFSBasic":
                        source = new DfsBasic();
                        break;
                    case "DFSCustom":
                        source = new DfsCustom();
                        if (_datasetContext.DataSources.Where(w => w is DfsCustom && w.Name == csm.Name).Count() > 0)
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("Name", "An DFS Custom Data Source is already exists with this name."));
                        }
                        break;
                    case "FTP":
                        source = new FtpSource();
                        if (_datasetContext.DataSources.Where(w => w is FtpSource && w.Name == csm.Name).Count() > 0)
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("Name", "An FTP Data Source is already exists with this name."));
                        }
                        if (!(csm.BaseUri.ToString().StartsWith("ftp://")))
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("BaseUri", "A valid FTP URI starts with ftp:// (i.e. ftp://foo.bar.com/base/dir)"));
                        }
                        break;
                    case "SFTP":
                        source = new SFtpSource();
                        if (_datasetContext.DataSources.Where(w => w is SFtpSource && w.Name == csm.Name).Count() > 0)
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("Name", "An SFTP Data Source is already exists with this name."));
                        }
                        if (!(csm.BaseUri.ToString().StartsWith("sftp://")))
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("BaseUri", "A valid SFTP URI starts with sftp:// (i.e. sftp://foo.bar.com//base/dir/)"));
                        }
                        break;
                    case "HTTPS":
                        source = new HTTPSSource();
                        bool valid = true;

                        if (_datasetContext.DataSources.Where(w => w is HTTPSSource && w.Name == csm.Name).Count() > 0)
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("Name", "An HTTPS Data Source is already exists with this name."));
                            valid = false;
                        }
                        if (!(csm.BaseUri.ToString().StartsWith("https://")))
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("BaseUri", "A valid HTTPS URI starts with https:// (i.e. https://foo.bar.com/base/api/)"));
                            valid = false;
                        }

                        //if token authentication, user must enter values for token header and value
                        if (auth.Is<TokenAuthentication>())
                        {

                            if (String.IsNullOrWhiteSpace(csm.TokenAuthHeader))
                            {
                                AddCoreValidationExceptionsToModel(new ValidationException("TokenAuthHeader", "Token Authenticaion requires a token header"));
                                valid = false;
                            }

                            if (String.IsNullOrWhiteSpace(csm.TokenAuthValue))
                            {
                                AddCoreValidationExceptionsToModel(new ValidationException("TokenAuthValue", "Token Authentication requires a token header value"));
                                valid = false;
                            }

                            if (valid)
                            {
                                ((HTTPSSource)source).AuthenticationHeaderName = csm.TokenAuthHeader;

                                EncryptionService encryptService = new EncryptionService();
                                Tuple<string, string> eresp = encryptService.EncryptString(csm.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"));

                                ((HTTPSSource)source).AuthenticationTokenValue = eresp.Item1;
                                ((HTTPSSource)source).IVKey = eresp.Item2;
                            }
                        }

                        foreach (RequestHeader h in csm.Headers)
                        {
                            if (String.IsNullOrWhiteSpace(h.Key) || String.IsNullOrWhiteSpace(h.Value))
                            {
                                valid = false;
                                AddCoreValidationExceptionsToModel(new ValidationException("RequestHeader", "Request headers need to contain valid values"));
                            }
                        }


                        //Process only if validations pass and headers exist
                        if (valid && csm.Headers.Any())
                        {


                            ((HTTPSSource)source).RequestHeaders = csm.Headers;
                        }

                        break;
                    default:
                        throw new NotImplementedException();
                }

                if (ModelState.IsValid)
                {

                    source.Name = csm.Name;
                    source.Description = csm.Description;
                    source.SourceAuthType = auth;
                    source.IsUserPassRequired = csm.IsUserPassRequired;
                    source.BaseUri = csm.BaseUri;
                    source.PortNumber = csm.PortNumber;

                    _datasetContext.Add(source);
                    _datasetContext.SaveChanges();

                    if (!String.IsNullOrWhiteSpace(csm.ReturnUrl))
                    {
                        return Redirect(csm.ReturnUrl);
                    }
                    else
                    {
                        return Redirect("/");
                    }
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }

            csm = CreateSourceDropDown(csm);
            return View("CreateDataSource", csm);

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

        private CreateSourceModel CreateSourceDropDown(CreateSourceModel csm)
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

            csm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic" && x.Value != "S3Basic").OrderBy(x => x.Value);

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
                int intvalue;
                var temp2 = AuthenticationTypesByType(csm.SourceType, Int32.TryParse(csm.AuthID, out intvalue) ? (int?)intvalue : null);
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


        [HttpGet]
        [Route("Config/Source/Edit/{sourceID}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditSource(int sourceID)
        {
            DataSource ds = _datasetContext.GetById<DataSource>(sourceID);
            EditSourceModel esm = new EditSourceModel(ds);

            esm = EditSourceDropDown(esm);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Data Source Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View("EditDataSource", esm);
        }

        [HttpPost]
        [Route("Config/Source/Edit/{sourceID}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditSource(EditSourceModel esm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DataSource source = null;
                    AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(esm.AuthID));

                    switch (esm.SourceType)
                    {
                        case "DFSBasic":
                            source = _datasetContext.GetById<DfsBasic>(esm.Id);
                            break;
                        case "DFSCustom":
                            source = _datasetContext.GetById<DfsCustom>(esm.Id);
                            break;
                        case "FTP":
                            source = _datasetContext.GetById<FtpSource>(esm.Id);
                            break;
                        case "S3Basic":
                            source = _datasetContext.GetById<S3Basic>(esm.Id);
                            break;
                        case "SFTP":
                            source = _datasetContext.GetById<SFtpSource>(esm.Id);
                            break;
                        case "HTTPS":
                            source = _datasetContext.GetById<HTTPSSource>(esm.Id);
                            bool valid = true;

                            //if token authentication, a token header is required.  However, a token header value is not required.  The existing
                            // token header value will be used if no value is specified.
                            if (auth.Is<TokenAuthentication>())
                            {
                                if (String.IsNullOrWhiteSpace(esm.TokenAuthHeader))
                                {
                                    AddCoreValidationExceptionsToModel(new ValidationException("TokenAuthHeader", "Token Authenticaion requires a token header"));
                                    valid = false;
                                }

                                if (valid)
                                {
                                    //If a new value was supplied, save new encrypted value and assoicated initial value
                                    if (!String.IsNullOrWhiteSpace(esm.TokenAuthValue))
                                    {
                                        EncryptionService encryptService = new EncryptionService();
                                        Tuple<string, string> eresp = encryptService.EncryptString(esm.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"));

                                        ((HTTPSSource)source).AuthenticationTokenValue = eresp.Item1;
                                        ((HTTPSSource)source).IVKey = eresp.Item2;
                                        ((HTTPSSource)source).AuthenticationHeaderName = esm.TokenAuthHeader;
                                    }
                                }
                            }

                            if (esm.Headers != null && esm.Headers.Any())
                            {
                                foreach (RequestHeader h in esm.Headers)
                                {
                                    //Check each request header\value pair to ensure they each have values
                                    if (String.IsNullOrWhiteSpace(h.Key) || String.IsNullOrWhiteSpace(h.Value))
                                    {
                                        valid = false;
                                        AddCoreValidationExceptionsToModel(new ValidationException("RequestHeader", "Request headers need to contain valid values"));
                                    }
                                }
                            }                            

                            //Replace all headers on each save
                            if (valid)
                            {
                                ((HTTPSSource)source).RequestHeaders = (esm.Headers != null && esm.Headers.Any()) ? esm.Headers : null;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            source.Description = esm.Description;
                            source.SourceAuthType = auth;
                            source.IsUserPassRequired = esm.IsUserPassRequired;
                            source.BaseUri = esm.BaseUri;
                            source.PortNumber = esm.PortNumber;

                            _datasetContext.SaveChanges();

                            if (!String.IsNullOrWhiteSpace(esm.ReturnUrl))
                            {
                                return Redirect(esm.ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                    catch (Sentry.Core.ValidationException ex)
                    {
                        AddCoreValidationExceptionsToModel(ex);
                        _datasetContext.Clear();
                    }
                }

                esm = EditSourceDropDown(esm);
                return View("EditDataSource", esm);

            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }

            esm = EditSourceDropDown(esm);
            return View("EditDataSource", esm);

        }

        [HttpGet]
        [Route("Config/Extension/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateExtensionMapping()
        {
            CreateExtensionMapModel cem = new CreateExtensionMapModel();
            cem.ExtensionMappings = _datasetContext.MediaTypeExtensions.ToList();

            return View("CreateExtensionMapping", cem);
        }

        [HttpPost]
        [Route("Config/Extension/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
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

                if(changeCnt > 0)
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

        private EditSourceModel EditSourceDropDown(EditSourceModel esm)
        {
            var temp = _datasetContext.DataSourceTypes.Select(v
              => new SelectListItem { Text = v.Name, Value = v.DiscrimatorValue }).ToList();

            //set selected for current value
            temp.ForEach(x => x.Selected = esm.SourceType.Equals(x.Value));

            esm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic" && x.Value != "S3Basic" && x.Value != "JavaApp").OrderBy(x => x.Value);

            int intvalue;
            var temp2 = AuthenticationTypesByType(esm.SourceType, Int32.TryParse(esm.AuthID, out intvalue) ? (int?)intvalue : null);
            
            esm.AuthTypesDropdown = temp2.OrderBy(x => x.Value);

            return esm;
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public JsonResult SourcesByType(string sourceType)
        {
            return Json(DataSourcesByType(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AuthenticationByType(string sourceType)
        {
            return Json(AuthenticationTypesByType(sourceType, null), JsonRequestBehavior.AllowGet);
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
                    foreach(AuthenticationType authtype in ftp.ValidAuthTypes)
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
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID ? true : false };
            }
            else if (authtype.Is<AnonymousAuthentication>())
            {
                AuthenticationType auth = _datasetContext.AuthTypes.Where(w => w is AnonymousAuthentication).First();
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID ? true : false };
            }
            else if (authtype.Is<TokenAuthentication>())
            {
                AuthenticationType auth = _datasetContext.AuthTypes.Where(w => w is TokenAuthentication).First();
                return new SelectListItem() { Text = auth.AuthName, Value = auth.AuthID.ToString(), Selected = selectedId == auth.AuthID ? true : false };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private List<SelectListItem> DataSourcesByType(string sourceType, int? selectedId)
        {
            List<SelectListItem> output;


            switch (sourceType)
            {
                case "FTP":
                    List<DataSource> fTpList = _datasetContext.DataSources.Where(x => x is FtpSource).ToList();
                    output = fTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "SFTP":
                    List<DataSource> sfTpList = _datasetContext.DataSources.Where(x => x is SFtpSource).ToList();
                    output = sfTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "DFSBasic":
                    List<DataSource> dfsBasicList = _datasetContext.DataSources.Where(x => x is DfsBasic).ToList();
                    output = dfsBasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "DFSCustom":
                    List<DataSource> dfsCustomList = _datasetContext.DataSources.Where(x => x is DfsCustom).ToList();
                    output = dfsCustomList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "S3Basic":
                    List<DataSource> s3BasicList = _datasetContext.DataSources.Where(x => x is S3Basic).ToList();
                    output = s3BasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "HTTPS":
                    List<DataSource> HttpsList = _datasetContext.DataSources.Where(x => x is HTTPSSource).ToList();
                    output = HttpsList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return output;
        }


        public JsonResult SourceTypeDescription(string sourceType)
        {
            var temp = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == sourceType).Select(x => x.Description).FirstOrDefault();

            return Json(temp, JsonRequestBehavior.AllowGet);
        }

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
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Fields(int configId, int schemaId)
        {
            DatasetFileConfig config = _datasetContext.GetById<DatasetFileConfig>(configId);

            ObsoleteDatasetModel bdm = new ObsoleteDatasetModel(config.ParentDataset, _associateInfoProvider, _datasetContext)
            {
                CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive,
                CanEditDataset = SharedContext.CurrentUser.CanEditDataset,
                CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs,
                CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive,
                CanUpload = SharedContext.CurrentUser.CanUpload
            };

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.DataConfig = config.ConfigId;
            e.Dataset = config.ParentDataset.DatasetId;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Edit Fields";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            //DataElement de = _datasetContext.GetById<DataElement>(schemaId);

            //if (de.DataObjects.Count == 0)
            //{
            //    List<DataObject> dobjList = new List<DataObject>();
            //    de.DataObjects = dobjList;
            //    //ViewBag.Schema = _datasetContext.GetById<DataElement>(schemaId);
            //    ViewBag.Schema = de;
            //}
            //else
            //{
                
            //}

            ViewBag.Schema = _datasetContext.GetById<DataElement>(schemaId);

            return View(bdm);
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/{schemaId}/UpdateFields")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public JsonResult UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows)
        {
            try
            {
                _configService.UpdateFields(configId, schemaId, schemaRows);

                //Task.Factory.StartNew(() => _eventService.PublishSuccessEventByConfigId(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Edit Fields", configId), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to update schema - ConfigId:{configId} DataElementId:{schemaId}", ex);
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Config/{configId}/Schema/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
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

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Retrieval Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(createSchemaModel);
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateSchema(int configId, CreateSchemaModel csm)
        {
            //throw new NotImplementedException();
            //NEED To revisit with merging of metadata repository tables

            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);

            DataElement maxSchemaRevision = dfc.Schema.OrderByDescending(o => o.SchemaRevision).FirstOrDefault();
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
                        HasHeader= csm.HasHeader,
                        SchemaName = csm.Name,
                        SchemaDescription = csm.Description,
                        SchemaIsForceMatch = csm.IsForceMatch,
                        SchemaIsPrimary = true,
                        SchemaRevision = (maxSchemaRevision == null) ? 0 : maxSchemaRevision.SchemaRevision + 1,
                        StorageCode = storageCode,
                        HiveDatabase = "Default",
                        HiveTable = dfc.ParentDataset.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dfc.Name.Replace(" ", "").ToUpper(),
                        HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                        HiveLocation = Configuration.Config.GetHostSetting("AWSRootBucket") + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode
                    };

                    dfc.Schema.Add(de);

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
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditSchema(int configId, int schemaId)
        {
            DataElement schema = _datasetContext.GetById<DataElement>(schemaId);

            EditSchemaModel editSchemaModel = new EditSchemaModel()
            {
                Name = schema.SchemaName,
                Description = schema.SchemaDescription,
                IsForceMatch = schema.SchemaIsForceMatch,
                IsPrimary = schema.SchemaIsPrimary,
                DatasetId = schema.DatasetFileConfig.ParentDataset.DatasetId,
                Delimiter = schema.Delimiter,
                DataElement_ID = schema.DataElement_ID,
                HasHeader = schema.HasHeader
            };

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Retrieval Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(editSchemaModel);
        }

        [HttpPost]
        [Route("Config/{configId}/Schema/{schemaId}/Edit")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditSchema(int configId, int schemaId, EditSchemaModel esm)
        {
            DataElement schema = _datasetContext.GetById<DataElement>(schemaId);

            try
            {
                if (ModelState.IsValid)
                {
                    schema.SchemaName = esm.Name;
                    schema.SchemaDescription = esm.Description;
                    schema.SchemaIsForceMatch = esm.IsForceMatch;
                    schema.SchemaIsPrimary = esm.IsPrimary;
                    schema.Delimiter = esm.Delimiter;
                    schema.DataElementChange_DTM = DateTime.Now;
                    schema.HasHeader = esm.HasHeader;

                    _datasetContext.Merge(schema);
                    _datasetContext.SaveChanges();

                    return RedirectToAction("Index", new { id = esm.DatasetId });
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

            return View(esm);
        }
    }
}