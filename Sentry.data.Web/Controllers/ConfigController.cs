using LazyCache;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Controllers
{
    public class ConfigController : BaseController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        private UserService _userService;
        private S3ServiceProvider _s3Service;
        private ISASService _sasService;
        private IAppCache _cache;

        public ConfigController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, ISASService sasService, IAssociateInfoProvider associateInfoService)
        {
            _cache = new CachingService();
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            _associateInfoProvider = associateInfoService;
        }

        [HttpGet]
        [Route("Config/Dataset/{id}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Index(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            BaseDatasetModel bdm = new BaseDatasetModel(ds, _associateInfoProvider);
            bdm.CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
            bdm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            bdm.CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs;
            bdm.CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
            bdm.CanUpload = SharedContext.CurrentUser.CanUpload;
            bdm.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(_userService.GetCurrentUser().AssociateId, id);
            bdm.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, id).Count;

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

            dfcm.DropPath = parent.DropLocation;

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

                    //Create Generic Data File Config for Dataset                    
                    DatasetFileConfig dfc = new DatasetFileConfig()
                    {
                        ConfigId = 0,
                        Name = dfcm.ConfigFileName,
                        Description = dfcm.ConfigFileDesc,
                        DropPath = dfcm.DropPath,
                        FileTypeId = dfcm.FileTypeId,
                        IsGeneric = false,
                        ParentDataset = parent,
                        FileExtension = _datasetContext.GetById<FileExtension>(dfcm.FileExtensionID),
                        DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dfcm.DatasetScopeTypeID)
                    };

                    List<RetrieverJob> jobList = new List<RetrieverJob>();

                    DataSource dataSource = _datasetContext.DataSources.Where(x => x.Name == "Default Drop Location").First();

                    Compression compression = new Compression()
                    {
                        IsCompressed = false,
                        CompressionType = null,
                        FileNameExclusionList = new List<string>()
                    };

                    RetrieverJobOptions rjo = new RetrieverJobOptions()
                    {
                        OverwriteDataFile = true,
                        TargetFileName = "",
                        CreateCurrentFile = false,
                        IsRegexSearch = true,
                        SearchCriteria = "\\.",
                        CompressionOptions = compression
                    };
                    RetrieverJob rj = new RetrieverJob()
                    {

                        Schedule = "Instant",
                        TimeZone = "Central Standard Time",
                        RelativeUri = null,
                        DataSource = dataSource,
                        DatasetConfig = dfc,
                        Created = DateTime.Now,
                        Modified = DateTime.Now,
                        IsGeneric = false,

                        JobOptions = rjo


                    };

                    jobList.Add(rj);
                    dfc.RetrieverJobs = jobList;

                    dfcList.Add(dfc);
                    parent.DatasetFileConfigs = dfcList;

                    _datasetContext.Merge<Dataset>(parent);
                    _datasetContext.SaveChanges();

                    try
                    {
                        if (!System.IO.Directory.Exists(dfcm.DropPath))
                        {
                            System.IO.Directory.CreateDirectory(dfcm.DropPath);
                        }
                    }
                    catch (Exception e)
                    {

                        StringBuilder errmsg = new StringBuilder();
                        errmsg.AppendLine("Failed to Create Drop Location:");
                        errmsg.AppendLine($"DatasetId: {parent.DatasetId}");
                        errmsg.AppendLine($"DatasetName: {parent.DatasetName}");
                        errmsg.AppendLine($"DropLocation: {parent.DropLocation}");

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

        [HttpGet]
        [Route("Config/Manage")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult Manage()
        {
            DatasetFileConfigsModel edfc = new DatasetFileConfigsModel();

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


                    RetrieverJob rj = new RetrieverJob() {
                        
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
            catch(System.UriFormatException uriEx)
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

            cjm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic").OrderBy(x => x.Value);

            List<SelectListItem> temp2 = new List<SelectListItem>();

            if(cjm.SelectedSourceType != null && cjm.SelectedDataSource != 0)
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

            if(cjm.NewFileNameExclusionList != null)
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
                    if(!rj.IsGeneric)
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
            catch (System.UriFormatException uriEx)
            {
                ModelState.AddModelError("RelativeUri", uriEx.Message);
            }

            ejm = EditDropDownSetup(ejm, rj);
            return View(ejm);
        }

        private EditJobModel EditDropDownSetup(EditJobModel ejm, RetrieverJob retrieverJob)
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

            ejm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic").OrderBy(x => x.Value);

            var temp2 = DataSourcesByType(ejm.SelectedSourceType, ejm.SelectedDataSource);

            temp2.Add(new SelectListItem()
            {
                Text = "Pick a Source",
                Value = "0"
            });

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
                if (ejm.FileNameExclusionList != null || ejm.FileNameExclusionList.Count != 0)
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

            return View("CreateDataSource", csm);
        }

        [HttpPost]
        [Route("Config/Source/Create")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult CreateSource(CreateSourceModel csm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DataSource source = null;
                    AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(csm.AuthID));

                    switch (csm.SourceType)
                    {
                        case "DFSBasic":
                            source = new DfsBasic();
                            break;
                        case "DFSCustom":
                            source = new DfsCustom();
                            break;
                        case "FTP":
                            source = new FtpSource();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    

                    source.Name = csm.Name;
                    source.Description = csm.Description;
                    source.SourceAuthType = auth;
                    source.IsUserPassRequired = csm.IsUserPassRequired;
                    source.BaseUri = csm.BaseUri;

                    _datasetContext.Add(source);
                    _datasetContext.SaveChanges();

                    if(!String.IsNullOrWhiteSpace(csm.ReturnUrl))
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

            csm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic").OrderBy(x => x.Value);

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

            return csm;
        }


        [HttpGet]
        [Route("Config/Source/Edit/{sourceID}")]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditSource(int sourceID)
        {
            DataSource ds = _datasetContext.GetById<DataSource>(sourceID);
            EditSourceModel esm = new EditSourceModel(ds);
            esm.Id = sourceID;

            esm = EditSourceDropDown(esm);

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
                        default:
                            throw new NotImplementedException();
                    }

                    source.Name = esm.Name;
                    source.Description = esm.Description;
                    source.SourceAuthType = auth;
                    source.IsUserPassRequired = esm.IsUserPassRequired;
                    source.BaseUri = esm.BaseUri;

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

            esm = EditSourceDropDown(esm);
            return View("EditDataSource", esm);

        }

        private EditSourceModel EditSourceDropDown(EditSourceModel esm)
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

            esm.SourceTypesDropdown = temp.Where(x => x.Value != "DFSBasic").OrderBy(x => x.Value);

            var temp2 = _datasetContext.AuthTypes.Select(v
             => new SelectListItem { Text = v.AuthName, Value = v.AuthID.ToString() }).ToList();

            temp2.Add(new SelectListItem()
            {
                Text = "Pick a Authentication Type",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            esm.AuthTypesDropdown = temp2.OrderBy(x => x.Value);

            return esm;
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public JsonResult SourcesByType(string sourceType)
        {
            return Json(DataSourcesByType(sourceType, null), JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> DataSourcesByType(string sourceType, int? selectedId)
        {
            List<SelectListItem> output;

            var list = _datasetContext.DataSources.ToList();

            switch (sourceType)
            {
                case "FTP":

                    List<DataSource> fTpList = new List<DataSource>();
                    foreach (var a  in list)
                    {
                        if (a.Is<FtpSource>())
                        {
                            fTpList.Add(a);
                        }
                    }
                    output = fTpList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "DFSBasic":
                    List<DataSource> dfsBasicList = new List<DataSource>();
                    foreach (var a in list)
                    {
                        if (a.Is<DfsBasic>())
                        {
                            dfsBasicList.Add(a);
                        }
                    }
                    output = dfsBasicList.Select(v
                         => new SelectListItem { Text = v.Name, Value = v.Id.ToString(), Selected = selectedId == v.Id ? true : false }).ToList();
                    break;
                case "DFSCustom":
                    List<DataSource> dfsCustomList = new List<DataSource>();
                    foreach (var a in list)
                    {
                        if (a.Is<DfsCustom>())
                        {
                            dfsCustomList.Add(a);
                        }
                    }

                    output = dfsCustomList.Select(v
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
            var temp = _datasetContext.DataSources.Where(x => x.Id == sourceId).Select(x => new { Description = x.Description, BaseUri = x.BaseUri}).FirstOrDefault();

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
                    case Dataset.ValidationErrors.s3keyIsBlank:
                        ModelState.AddModelError("Key", vr.Description);
                        break;
                    case Dataset.ValidationErrors.nameIsBlank:
                        ModelState.AddModelError("Name", vr.Description);
                        break;
                    case Dataset.ValidationErrors.creationUserNameIsBlank:
                        ModelState.AddModelError("CreationUserName", vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetDateIsOld:
                        ModelState.AddModelError("DatasetDate", vr.Description);
                        break;
                    default:
                        ModelState.AddModelError(string.Empty, vr.Description);
                        break;
                }
            }
        }
    }
}