using Sentry.Core;
using Sentry.data.Core;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Linq.Dynamic;
using System.Web;
using Sentry.data.Web.Helpers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Sentry.data.Infrastructure;
using Sentry.DataTables.Shared;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.QueryableAdapter;
using Sentry.data.Common;
using System.Diagnostics;
using LazyCache;
using StackExchange.Profiling;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.Metadata;
using static Sentry.data.Core.RetrieverJobOptions;
using Hangfire;
using System.Web.Script.Serialization;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class DatasetController : BaseController
    {
        public readonly IAssociateInfoProvider _associateInfoProvider;
        public readonly IDatasetContext _datasetContext;
        private readonly UserService _userService;
        private readonly S3ServiceProvider _s3Service;
        private readonly ISASService _sasService;
        private readonly IRequestContext _requestContext;

        public DatasetController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, ISASService sasService, IAssociateInfoProvider associateInfoService, IRequestContext requestContext)
        {
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            _associateInfoProvider = associateInfoService;
            _requestContext = requestContext;
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Index()
        {
            HomeModel hm = new HomeModel();

            List<Dataset> dsList = _datasetContext.Datasets.ToList();
            List<Category> cList = _datasetContext.Categories.ToList();

            hm.DatasetCount = dsList.Count(w => w.DatasetType != "RPT");
            hm.Categories = cList.Where(w => w.ObjectType != "RPT").ToList();
            hm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            hm.CanUpload = SharedContext.CurrentUser.CanUpload;

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Dataset Home Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(hm);
        }


        #region Dataset Modification

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Create()
        {
            CreateDatasetModel cdm = new CreateDatasetModel();

            cdm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            cdm.CanUpload = SharedContext.CurrentUser.CanUpload;

            cdm.DatasetFileConfigs = new List<DatasetFileConfigsModel>();
            cdm.DatasetFileConfigs.Add(new DatasetFileConfigsModel() { ConfigFileName = "Default", ConfigFileDesc = "Default Config for Dataset.  Uploaded files that do not match any configs will default to this config" });

            cdm.SearchCriteria = "\\.";           

            cdm = (CreateDatasetModel) Utility.setupLists(_datasetContext, cdm);
            cdm.IsRegexSearch = true;

            cdm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Dataset Creation Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(cdm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Create(CreateDatasetModel cdm)
        {

            if (_datasetContext.isDatasetNameDuplicate(cdm.DatasetName, _datasetContext.GetCategoryById(cdm.CategoryIDs).Name))
            {
                AddCoreValidationExceptionsToModel(new ValidationException("DatasetName", "Dataset name already exists within category"));
            }
            try
            {
                if (ModelState.IsValid)
                {
                    Dataset ds = CreateDatasetFromModel(cdm);
                    ds = _datasetContext.Merge<Dataset>(ds);

                    List<DataElement> deList = new List<DataElement>();
                    DataElement de = CreateNewDataElement(cdm);

                    //Create Generic Data File Config for Dataset            
                    DatasetFileConfig dfc = new DatasetFileConfig()
                    {
                        ConfigId = 0,
                        Name = cdm.ConfigFileName,
                        Description = cdm.ConfigFileDesc,
                        //SearchCriteria = cdm.SearchCriteria, 
                        //DropPath = cdm.DropPath + "default\\",
                        //IsRegexSearch = cdm.IsRegexSearch,
                        //OverwriteDatafile = true,
                        FileTypeId = (int)FileType.DataFile,
                        ParentDataset = ds,
                        DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(cdm.DatasetScopeTypeID),
                        FileExtension = _datasetContext.GetById<FileExtension>(cdm.FileExtensionID),                        
                    };

                    de.DatasetFileConfig = dfc;
                    deList.Add(de);
                    dfc.Schema = deList;

                    List<RetrieverJob> jobList =  new List<RetrieverJob>();

                    //Store this one so that the local drop location can be created.
                    RetrieverJob rj = Utility.InstantiateJobsForCreation(dfc, _datasetContext.DataSources.First(x => x.Name.Contains("Default Drop Location")));
                    jobList.Add(rj);

                    jobList.Add(Utility.InstantiateJobsForCreation(dfc, _datasetContext.DataSources.First(x => x.Name.Contains("Default S3 Drop Location"))));

                    dfc.RetrieverJobs = jobList;

                    _datasetContext.Merge<DatasetFileConfig>(dfc);
                    _datasetContext.SaveChanges();

                    Event e = new Event();
                    e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Created Dataset").FirstOrDefault();
                    e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                    e.Dataset = (_datasetContext.Datasets.ToList()).FirstOrDefault(x => x.DatasetName == cdm.DatasetName).DatasetId;
                    e.Reason = cdm.DatasetName + " was created.";
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                    //create drop locations
                    try
                    {
                        //Config Drop location
                        if (!Directory.Exists(rj.GetUri().LocalPath))
                        {
                            Directory.CreateDirectory(rj.GetUri().LocalPath);
                        }

                    }
                    catch (Exception ex)
                    {
                        StringBuilder errmsg = new StringBuilder();
                        errmsg.AppendLine("Failed to Create Drop Location:");
                        errmsg.AppendLine($"DatasetId: {ds.DatasetId}");
                        errmsg.AppendLine($"DatasetName: {ds.DatasetName}");
                        errmsg.AppendLine($"DropLocation: {rj.GetUri().LocalPath}");

                        Logger.Error(errmsg.ToString(), ex);
                    }

                    return RedirectToAction("Detail", new { id = (_datasetContext.Datasets.ToList()).FirstOrDefault(x => x.DatasetName == cdm.DatasetName).DatasetId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _datasetContext.Clear();
                cdm = (CreateDatasetModel) Utility.setupLists(_datasetContext, cdm);
                cdm.ExtensionList = Utility.GetFileExtensionListItems(_datasetContext);
            }

            List<SearchableTag> tagsToReturn = new List<SearchableTag>();
            int[] json = new JavaScriptSerializer().Deserialize<int[]>(cdm.TagString);
            for (int i = 0; i < json.Length; i++)
            {
                tagsToReturn.Add(_datasetContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault().GetSearchableTag());
            }
            cdm.TagString = new JavaScriptSerializer().Serialize(tagsToReturn);
            ModelState.Remove("TagString");
            return View(cdm);
        }

        private DataElement CreateNewDataElement(CreateDatasetModel cdm)
        {
            DataElement de = new DataElement()
            {
                DataElementCreate_DTM = DateTime.Now,
                DataElementChange_DTM = DateTime.Now,
                DataElement_CDE = "F",
                DataElement_DSC = DataElementCode.DataFile,
                DataElement_NME = cdm.ConfigFileName,
                LastUpdt_DTM = DateTime.Now,
                SchemaIsPrimary = true,
                SchemaDescription = cdm.ConfigFileDesc,
                SchemaName = cdm.ConfigFileName,
                SchemaRevision = 1,
                SchemaIsForceMatch = false,
                FileFormat = _datasetContext.GetById<FileExtension>(cdm.FileExtensionID).Name.ToUpper(),
                Delimiter = cdm.Delimiter,
                StorageCode = _datasetContext.GetNextStorageCDE().ToString(),
                HiveDatabase = "Default",
                HiveTable = cdm.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + cdm.ConfigFileName.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString()
            };

            return de;
        }

        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        private Dataset CreateDatasetFromModel(CreateDatasetModel cdm)
        {
            DateTime CreateTime = DateTime.Now;
            Category cat = _datasetContext.GetById<Category>(cdm.CategoryIDs);
            IApplicationUser user = _userService.GetCurrentUser();

            Dataset ds = new Dataset()
            {
                DatasetId = 0,
                Category = cat.Name,
                DatasetCategory = _datasetContext.GetCategoryById(cdm.CategoryIDs),
                DatasetName = cdm.DatasetName,
                DatasetDesc = cdm.DatasetDesc,
                DatasetInformation = cdm.DatasetInformation,
                CreationUserName = user.DisplayName,
                SentryOwnerName = cdm.SentryOwnerName,
                UploadUserName = user.AssociateId,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), cdm.OriginationID),
                DatasetDtm = CreateTime,
                ChangedDtm = CreateTime,
                IsSensitive = false,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null,
                Tags = new List<MetadataTag>()
            };

            int[] json = new JavaScriptSerializer().Deserialize<int[]>(cdm.TagString);

            ds.Tags = new List<MetadataTag>();

            for (int i = 0; i < json.Length; i++)
            {
                ds.Tags.Add(_datasetContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault());
            }

            return ds;
        }

        // GET: DatasetFileVersion/Edit/5
        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Edit(int id)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(id);

            EditDatasetModel item = new EditDatasetModel(ds, _associateInfoProvider);

            item = (EditDatasetModel) Utility.setupLists(_datasetContext,item);

            var json = new JavaScriptSerializer().Serialize(ds.Tags.Select(x => x.GetSearchableTag()));
            item.TagString = json;

            item.OwnerID = ds.SentryOwnerName;

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Dataset Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(item);
        }

        // POST: Dataset/Edit/5
        [HttpPost()]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Edit(int id, EditDatasetModel edm)
        {
            try
            {
                Dataset item = _datasetContext.GetById<Dataset>(id);
                if (ModelState.IsValid)
                {
                    item = UpdateDatasetFromModel(item, edm);
                    _datasetContext.SaveChanges();
                    return RedirectToAction("Detail", new { id = id });

                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _datasetContext.Clear();

                edm = (EditDatasetModel) Utility.setupLists(_datasetContext, edm);
            }

            List<SearchableTag> tagsToReturn = new List<SearchableTag>();
            int[] json = new JavaScriptSerializer().Deserialize<int[]>(edm.TagString);
            for (int i = 0; i < json.Length; i++)
            {
                tagsToReturn.Add(_datasetContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault().GetSearchableTag());
            }
            edm.TagString = new JavaScriptSerializer().Serialize(tagsToReturn);
            ModelState.Remove("TagString");

            return View(edm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        private Dataset UpdateDatasetFromModel(Dataset ds, EditDatasetModel eds)
        {
            ds.DatasetInformation = eds.DatasetInformation;
            ds.OriginationCode = eds.OriginationCode;
            ds.ChangedDtm = DateTime.Now;

            if (null != eds.Category && eds.Category.Length > 0)
            {
                ds.Category = eds.Category;
            }

            if (null != eds.CreationUserName && eds.CreationUserName.Length > 0)
            {
                ds.CreationUserName = eds.CreationUserName;
            }

            if (null != eds.DatasetDesc && eds.DatasetDesc.Length > 0)
            {
                ds.DatasetDesc = eds.DatasetDesc;
            }

            if (eds.DatasetDtm > DateTime.MinValue)
            {
                ds.DatasetDtm = eds.DatasetDtm;
            }

            if (null != eds.DatasetName && eds.DatasetName.Length > 0)
            {
                ds.DatasetName = eds.DatasetName;
            }

            if (null != eds.SentryOwnerName && eds.SentryOwnerName.Length > 0)
            {
                int n;
                if(int.TryParse(eds.SentryOwnerName, out n))
                {
                    ds.SentryOwnerName = eds.SentryOwnerName;
                }
                else
                {
                    var associate = _associateInfoProvider.GetAssociateInfoByName(eds.SentryOwnerName);
                    if (associate.FullName == eds.SentryOwnerName)
                    {
                        ds.SentryOwnerName = associate.Id;
                    }
                }

            }

            int[] json = new JavaScriptSerializer().Deserialize<int[]>(eds.TagString);

            ds.Tags = new List<MetadataTag>();

            for (int i = 0; i < json.Length; i++)
            {
                ds.Tags.Add(_datasetContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault());
            }

            return ds;
        }

        [HttpPost]
        [Route("{objectType}/Delete/{id}/")]
        public JsonResult Delete(string objectType, int id)
        {
            switch (objectType.ToLower())
            {
                case "businessintelligence":
                    if (!SharedContext.CurrentUser.CanManageReports)
                    {
                        throw new NotAuthorizedException("User is authenticated but does not have permission");
                    }

                    try
                    {
                        _datasetContext.RemoveById<Dataset>(id);
                        _datasetContext.SaveChanges();
                        return Json(new { Success = true, Message = "Object was successfully deleted" });                        
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to delete dataset - DatasetId:{id} RequestorId:{SharedContext.CurrentUser.AssociateId} RequestorName:{SharedContext.CurrentUser.DisplayName}", ex);
                        return Json(new { Success = false, Message = "We failed to delete object.  Please try again later." });
                    }
                default:
                    return Json(new { Success = false, Message = "Delete not allowed for this type of object."});
            }
        }
        #endregion

        #region Dataset FILE Modification

        [HttpPost()]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult EditDatasetFile(int id, DatasetFileGridModel i)
        {
            try
            {
                DatasetFile item = _datasetContext.GetById<DatasetFile>(id);

                Event e = new Event();
                e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Edited Data File").FirstOrDefault();
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                e.Dataset = item.Dataset.DatasetId;
                e.DataFile = item.DatasetFileId;
                e.DataConfig = item.DatasetFileConfig.ConfigId;
                e.Reason = "Edited Dataset File";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


                if (ModelState.IsValid)
                {
                    UpdateDatasetfileFromModel(item, i);
                    _datasetContext.SaveChanges();
                    return RedirectToAction("Detail", new { id = item.Dataset.DatasetId });

                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _datasetContext.Clear();
            }

            return View();
        }


        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        private void UpdateDatasetfileFromModel(DatasetFile df, DatasetFileGridModel dfgm)
        {
            DateTime now = DateTime.Now;

            df.Information = dfgm.Information;

            df.ModifiedDTM = now;
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public PartialViewResult EditDatasetFile(int id)
        {
            DatasetFile df = _datasetContext.GetById<DatasetFile>(id);
            DatasetFileGridModel item = new DatasetFileGridModel(df, _associateInfoProvider);

            return PartialView("EditDataFile", item);

        }

        #endregion

        #region Detail Page

        [HttpGet]
        [Route("{objectType}/Detail/{id}/")]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Detail(string objectType, int id)
        {
            Dataset ds = _datasetContext.GetById(id);

            BaseDatasetModel bdm = new BaseDatasetModel(ds, _associateInfoProvider, _datasetContext);

            bdm.IsFavorite = ds.Favorities.Where(w => w.UserId == SharedContext.CurrentUser.AssociateId).Any();
            
            Event e = new Event();

            //Object specific settings
            switch (objectType.ToLower())
            {                
                case "businessintelligence":
                    if (!SharedContext.CurrentUser.CanViewReports)
                    {
                        throw new NotAuthorizedException("User is authenticated but does not have permission");
                    }
                    //Model settings specific to Business Intelligence
                    bdm.CanManageReport = SharedContext.CurrentUser.CanManageReports;
                    bdm.ObjectType = ds.DatasetType;

                    //Event reason tailored to Business Intelligence
                    e.Reason = "Viewed Business Intelligence Detail Page";

                    ViewBag.Title = "View Report Details";
                    break;

                case "dataset":
                default:
                    if (!SharedContext.CurrentUser.CanViewDataset)
                    {
                        throw new NotAuthorizedException("User is authenticated but does not have permission");
                    }
                    bdm.CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
                    bdm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
                    bdm.CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs;
                    bdm.CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
                    bdm.CanUpload = SharedContext.CurrentUser.CanUpload;
                    bdm.CanQueryTool = SharedContext.CurrentUser.CanQueryTool || SharedContext.CurrentUser.CanQueryToolPowerUser;
                    bdm.Downloads = _datasetContext.Events.Where(x => x.EventType.Description == "Downloaded Data File" && x.Dataset == ds.DatasetId).Count();
                    if (ds.DatasetFiles.Any())
                    {
                        bdm.ChangedDtm = ds.DatasetFiles.Max(x => x.ModifiedDTM);
                    }
                    else
                    {
                        bdm.ChangedDtm = ds.ChangedDtm;
                    }

                    //Event reason tailored to Datasets
                    e.Reason = "Viewed Dataset Detail Page";

                    ViewBag.Title = "View Dataset Details";
                    break;
            }
            
            bdm.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(_userService.GetCurrentUser().AssociateId, id);
            bdm.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, id).Count;

            bdm.Views = _datasetContext.Events.Where(x => x.EventType.Description == "Viewed" && x.Dataset == ds.DatasetId).Count();           

            
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Dataset = ds.DatasetId;            
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
            
            return View(bdm);
        }

        [Route("Dataset/Detail/{id}/Configuration")]
        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult DatasetConfiguration(int id)
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

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Dataset Configuration Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View("Configuration", bdm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Subscribe(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            SubscriptionModel sm = new SubscriptionModel();

            sm.AllEventTypes = _datasetContext.EventTypes.Where(w => w.Display).Select((c) => new SelectListItem { Text = c.Description, Value = c.Type_ID.ToString() });
            sm.AllIntervals = _datasetContext.GetAllIntervals().Select((c) => new SelectListItem { Text = c.Description, Value = c.Interval_ID.ToString() });

            sm.CurrentSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, id);

            sm.datasetID = ds.DatasetId;

            sm.SentryOwnerName = _userService.GetCurrentUser().AssociateId;


            foreach (Core.EventType et in _datasetContext.EventTypes.Where(w => w.Display))
            {
                if(!sm.CurrentSubscriptions.Any(x => x.EventType.Type_ID == et.Type_ID))
                {
                    DatasetSubscription subscription = new DatasetSubscription();
                    subscription.Dataset = ds;
                    subscription.SentryOwnerName = _userService.GetCurrentUser().AssociateId;
                    subscription.EventType = et;
                    subscription.Interval = _datasetContext.GetInterval("Never");
                    subscription.ID = 0;

                    sm.CurrentSubscriptions.Add(subscription);
                }
            }

            return PartialView("_Subscribe", sm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Subscribe(SubscriptionModel sm)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            var a = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, sm.datasetID);


            foreach (DatasetSubscription sub in sm.CurrentSubscriptions)
            {
                bool found = false;
                foreach (DatasetSubscription ds in a)
                {
                    if (sub.EventType.Type_ID == ds.EventType.Type_ID)
                    {
                        ds.Interval = sub.Interval;
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    _datasetContext.Merge<DatasetSubscription>(
                        new DatasetSubscription(
                            _datasetContext.GetById<Dataset>(sm.datasetID),
                            _datasetContext.EventTypes.Where(w => w.Type_ID == sub.EventType.Type_ID).FirstOrDefault(),
                            _datasetContext.GetInterval(sub.Interval.Interval_ID),
                            user.AssociateId));
                }

            }

            _datasetContext.SaveChanges();

            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult GetDatasetFileInfoForGrid(int Id, Boolean bundle, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            //IEnumerable < DatasetFileGridModel > files = _datasetContext.GetAllDatasetFiles().ToList().

            List<DatasetFileGridModel> files = new List<DatasetFileGridModel>();
            Boolean CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
            Boolean CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
            Boolean CanEdit = SharedContext.CurrentUser.CanEditDataset;

            //Query the Dataset for the following information:
            foreach (DatasetFile df in _datasetContext.GetDatasetFilesForDatasetFileConfig(Id, x => !x.IsBundled).ToList())
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(df, _associateInfoProvider);
                dfgm.CanDwnldNonSensitive = CanDwnldNonSensitive;
                dfgm.CanDwnldSenstive = CanDwnldSenstive;
                dfgm.CanEdit = CanEdit;
                dfgm.CanPreview = true;
                files.Add(dfgm);
            }

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);
            int a = dtqa.GetDataTablesResponse().data.Count();

            Debug.WriteLine(a);

            if (bundle)
            {
                return Json(dtqa.GetDataTablesResponse(true), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
            }
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult GetBundledFileInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            //IEnumerable < DatasetFileGridModel > files = _datasetContext.GetAllDatasetFiles().ToList().
            List<DatasetFileGridModel> files = new List<DatasetFileGridModel>();
            Boolean CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
            Boolean CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
            Boolean CanEdit = SharedContext.CurrentUser.CanEditDataset;

            List<DatasetFile> bundledList = _datasetContext.GetDatasetFilesForDatasetFileConfig(Id, w => w.IsBundled).ToList();

            foreach (DatasetFile df in bundledList)
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(df, _associateInfoProvider);
                dfgm.CanDwnldNonSensitive = CanDwnldNonSensitive;
                dfgm.CanDwnldSenstive = CanDwnldSenstive;
                dfgm.CanEdit = CanEdit;
                dfgm.CanPreview = true;
                files.Add(dfgm);
            }

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);

            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);

        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult GetVersionsOfDatasetFileForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            DatasetFile df = _datasetContext.GetDatasetFile(Id);

            List<DatasetFileGridModel> files = new List<DatasetFileGridModel>();
            Boolean CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
            Boolean CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
            Boolean CanEdit = SharedContext.CurrentUser.CanEditDataset;

            foreach (DatasetFile dfversion in _datasetContext.GetDatasetFilesVersions(df.Dataset.DatasetId, df.DatasetFileConfig.ConfigId, df.FileName).ToList())
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(dfversion, _associateInfoProvider);
                dfgm.CanDwnldNonSensitive = CanDwnldNonSensitive;
                dfgm.CanDwnldSenstive = CanDwnldSenstive;
                dfgm.CanEdit = CanEdit;
                dfgm.CanPreview = false;
                files.Add(dfgm);
            }

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult GetAllDatasetFileInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            IEnumerable<DatasetFileGridModel> files = _datasetContext.GetAllDatasetFiles().
                Select((f) => new DatasetFileGridModel(f, _associateInfoProvider)
                );

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.UseApp)]
        public JsonResult GetDatasetFileConfigInfo(int Id)
        {
            IEnumerable<DatasetFileConfigsModel> files = null;
            if (Id > 0)
            {
                var a = _datasetContext.GetById<Dataset>(Id).DatasetFileConfigs.ToList();

                var b = a.Select((f) => new DatasetFileConfigsModel(f, false, true)).ToList();

                files = b;
            }
            else
            {
                files = _datasetContext.getAllDatasetFileConfigs().Select((f) => new DatasetFileConfigsModel(f, false, true));
            }

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public JsonResult GetDatasetFileConfigInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            IEnumerable<DatasetFileConfigsModel> files = null;
            if (Id > 0)
            {
                files = _datasetContext.GetById(Id).DatasetFileConfigs.Select((f) => new DatasetFileConfigsModel(f, true, false));
            }
            else
            {
                files = _datasetContext.getAllDatasetFileConfigs().Select((f) => new DatasetFileConfigsModel(f, true, false));
            }

            DataTablesQueryableAdapter<DatasetFileConfigsModel> dtqa = new DataTablesQueryableAdapter<DatasetFileConfigsModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Helpers

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DwnldNonSensitive)]
        public JsonResult GetDatasetFileDownloadURL(int id)
        {
            DatasetFile df = _datasetContext.GetDatasetFile(id);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Downloaded Data File").FirstOrDefault();
            
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.DataFile = df.DatasetFileId;
            e.Dataset = df.Dataset.DatasetId;
            e.DataConfig = df.DatasetFileConfig.ConfigId;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
           
            try
            {                
                //Testing if object exists in S3, response is not used.
                _s3Service.GetObjectMetadata(df.FileLocation);              

                JsonResult jr = new JsonResult();
                jr.Data = _s3Service.GetDatasetDownloadURL(df.FileLocation, df.VersionId);
                jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

                e.Reason = "Successfully Downloaded Data File";
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return jr;
            }
            catch(Exception ex)
            {
                e.Reason = "Failed to Download Data File";
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                var datasetId = df.Dataset != null ? df.Dataset.DatasetId.ToString() : "HUGE PROBLEM.  NO DATASET FOUND.";

                Logger.Fatal($"S3 Data File Not Found - DatasetID:{datasetId} DatasetFile_ID:{id}", ex);
                return Json(new { message = "Encountered Error Retrieving File.<br />If this problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>" }, JsonRequestBehavior.AllowGet);
            }   
            

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

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult PushToSAS(int id, string fileOverride, string delimiter, int guessingrows)
        {
            Event _event = new Event();
            _event.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Pushed Data File to SAS").FirstOrDefault();
            _event.TimeCreated = DateTime.Now;
            _event.TimeNotified = DateTime.Now;
            _event.IsProcessed = false;
            _event.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;

            try
            {
                DatasetFile ds = _datasetContext.GetById<DatasetFile>(id);

                _event.DataFile = ds.DatasetFileId;
                _event.Dataset = ds.Dataset.DatasetId;
                _event.DataConfig = ds.DatasetFileConfig.ConfigId;

                string filename = null;
                string filename_orig = null;

                Sentry.Common.Logging.Logger.Debug("DatasetId: " + id);
                Sentry.Common.Logging.Logger.Debug("File Name Override Value: " + fileOverride);

                //Test for an override name; if empty or null, use current value on dataset model
                if (!String.IsNullOrWhiteSpace(fileOverride))
                {
                    //Test if override name includes an extension; if exists, replace with current value in dataset model
                    if (Path.HasExtension(fileOverride))
                    {
                        Sentry.Common.Logging.Logger.Debug("Has File Extension: " + System.IO.Path.GetExtension(fileOverride));
                        Sentry.Common.Logging.Logger.Debug("Dataset Model Extension: " + System.IO.Path.GetExtension(ds.FileName));
                        filename = fileOverride.Replace(System.IO.Path.GetExtension(fileOverride), System.IO.Path.GetExtension(ds.FileName));
                    }
                    else
                    {
                        Sentry.Common.Logging.Logger.Debug("Has No File Extension");
                        Sentry.Common.Logging.Logger.Debug("Dataset Model Extension: " + System.IO.Path.GetExtension(ds.FileName));
                        filename = (fileOverride + System.IO.Path.GetExtension(ds.FileName));
                    }
                }
                else
                {
                    Sentry.Common.Logging.Logger.Debug(" No Override Value");
                    Sentry.Common.Logging.Logger.Debug("Dataset Model S3Key: " + System.IO.Path.GetFileName(ds.FileLocation));
                    filename = System.IO.Path.GetFileName(ds.FileLocation);
                }

                filename_orig = filename;

                //Gerenate SAS friendly file name.
                filename = _sasService.GenerateSASFileName(filename);

                //Sentry.Common.Logging.Logger.Debug($"File Name Translation: Original({filename_orig} SASFriendly({filename})");

                string BaseTargetPath = Configuration.Config.GetHostSetting("PushToSASTargetPath");

                //creates category directory if does not exist, otherwise does nothing.
                System.IO.Directory.CreateDirectory(BaseTargetPath + ds.Dataset.Category);

                try
                {

                    _s3Service.TransferUtilityDownload(BaseTargetPath, ds.Dataset.Category, filename, ds.FileLocation, ds.VersionId);

                }
                catch (Exception e)
                {
                    Sentry.Common.Logging.Logger.Error("S3 Download Error", e);
                    return PartialView("_Success", new SuccessModel("Push to SAS Error", e.Message, false));
                }


                //Converting file to .sas7bdat format
                try
                {
                    // Pass preview object key for data file to ConvertToSASFormat  
                    _sasService.ConvertToSASFormat(ds.DatasetFileId, filename, delimiter, guessingrows);

                }
                catch (WebException we)
                {
                    Sentry.Common.Logging.Logger.Error("Web Error Calling SAS Stored Process", we);
                    return PartialView("_Success", new SuccessModel("Push to SAS Error", we.Message, false));
                }
                catch (Exception e)
                {
                    Sentry.Common.Logging.Logger.Error("Error calling SAS Stored Process", e);
                    return PartialView("_Success", new SuccessModel("Push to SAS Error", e.Message, false));
                }

                _event.Reason = "Successfully Pushed to SAS";
                _event.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(_event), TaskCreationOptions.LongRunning);

                return PartialView("_Success", new SuccessModel("Successfully Pushed File to SAS", $"Dataset file {filename_orig} has been converted to {filename.Replace(Path.GetExtension(filename), ".sas7bdat")}. The file can be found at {BaseTargetPath.Replace("\\sentry.com\appfs_nonprod", "S: ")}.", true));
            }
            catch (Exception e)
            {
                _event.Reason = "Error calling SAS Stored Process";
                _event.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(_event), TaskCreationOptions.LongRunning);

                Logger.Error("Error calling SAS Stored Process", e);
                return PartialView("_Success", new SuccessModel("Push to SAS Error", e.Message, false));
            }
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public PartialViewResult PushToFileNameOverride(int id)
        {
            PushToDatasetModel model = new PushToDatasetModel();
            
            DatasetFile datafile = _datasetContext.GetById<DatasetFile>(id);
            model.DatasetFileId = datafile.DatasetFileId;
            model.DatasetFileName = datafile.FileName;

            return PartialView("_PushToFilenameOverride_new", model);

        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult PreviewDatafile(int id)
        {
            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Previewed Data File").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
           
            try
            {
                DatasetFile df = _datasetContext.GetDatasetFile(id);
                e.DataFile = df.DatasetFileId;
                e.Dataset = df.Dataset.DatasetId;
                e.DataConfig = df.DatasetFileConfig.ConfigId;
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();

                string previewKey = _datasetContext.GetPreviewKey(id);

                e.Reason = "Successfully Viewed Preview";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return PartialView("_PreviewData", PreviewFile(previewKey));
            }
            catch (Exception ex)
            {
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                e.Reason = "Error Retrieving Preview";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return PartialView("_Success", new SuccessModel("Error Retrieving Preview", ex.Message, false));
            }
            
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public PartialViewResult PreviewLatestDatafile(int id)
        {
            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Previewed Data File").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.DataFile = id;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;

            try
            {
                DatasetFile df = _datasetContext.GetDatasetFile(id);
                e.DataFile = df.DatasetFileId;
                e.Dataset = df.Dataset.DatasetId;
                e.DataConfig = df.DatasetFileConfig.ConfigId;
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();

                int latestDatafile = GetLatestDatasetFileIdForDataset(id);
                string previewKey = _datasetContext.GetPreviewKey(latestDatafile);

                e.Reason = "Successfully Viewed Preview";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return PartialView("_PreviewData", PreviewFile(previewKey));
            }
            catch (Exception ex)
            {
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                e.Reason = "Error Retrieving Preview";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return PartialView("_Success", new SuccessModel("Error Retrieving Preview", ex.Message, false));
            }
           
        }

        private PreviewDataModel PreviewFile(string previewKey)
        {
            PreviewDataModel model = new PreviewDataModel();

            using (Stream stream = _s3Service.GetObject(previewKey))
            {
                long length = stream.Length;
                byte[] bytes = new byte[length];
                int i = stream.Read(bytes, 0, (int)length);

                if (i == 0)
                {
                    throw new AmazonS3Exception("Error Retrieving Preview");
                }
                else
                {
                    model.PreviewData = Encoding.UTF8.GetString(bytes);
                }

                stream.Close();
                stream.Dispose();
            }

            return model;
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public PartialViewResult GetDatasetFileVersions(int id)
        {
            DatasetFileVersionsModel model = new DatasetFileVersionsModel();

            model.DatasetFileId = id;

            return PartialView("_DatasetFileVersions", model);
            
        }

       
        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public async Task<ActionResult> BundleFiles(string listOfIds, string newName, int datasetID)
        {
            string[] ids = listOfIds.Split(',');

            List<DatasetFile> files = (from file in _datasetContext.GetAllDatasetFiles()
                                       from id in ids
                                       where file.DatasetFileId.ToString() == id
                                       select file).ToList();

            var extension = Path.GetExtension(files[0].FileName);


            //Do all the files included in the list have the same exact extension.
            Boolean sameExtension = files.All(x => Path.GetExtension(x.FileName) == extension) ? true : false;
            Boolean allDataFiles = files.All(x => x.DatasetFileConfig.FileTypeId == (int)FileType.DataFile) ? true : false;

            //Get the users permissions
            Boolean errorsFound = false;
            string errorString = "";
            Boolean CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
            Boolean CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;

            Boolean bundlingSensitive = files.Any(x => x.Dataset.IsSensitive);

            if(newName == "" || newName == null)
            {
                errorsFound = true;
                errorString += "<p>Please supply a new name to give to your bundled file.</p>";
            }

            if(!allDataFiles)
            {
                errorsFound = true;
                errorString += "<p>You cannot bundle files that are labeled as supplementary files, help documents, or usage manuals.</p>";
            }

            if(files.Count == 1)
            {
                errorsFound = true;
                errorString += "<p>You cannot bundle just one file.</p>";
            }
            else if (files.Count == 0)
            {
                errorsFound = true;
                errorString += "<p>You selected no files.</p>";
            }

            if ((bundlingSensitive && !CanDwnldSenstive) || (!bundlingSensitive && !CanDwnldNonSensitive))
            {
                errorsFound = true;
                errorString += "<p>You do not have permission to download or bundle these files.</p>";
            }

            if(!sameExtension)
            {
                errorsFound = true;
                errorString += "<p>The files did not have the same file extension. Bundling requires that all files have the same extension.  Please filter by putting the file extension in either the Name Column or the Search Box provided at the top right of the table.</p>";
            }

            try
            {
                if (!errorsFound)
                {
                    //Pass the list of files off to the File Bundler in S3.
                    string userEmail = SharedContext.CurrentUser.EmailAddress;
                    DatafileBundleProvider myBundleRequest = new DatafileBundleProvider();

                    Dataset parentDataset = _datasetContext.GetById<Dataset>(files.FirstOrDefault().Dataset.DatasetId);

                    //Passing UserID and Timestamp to hash method to ensure unqiue GUID for request
                    BundleRequest _request = new BundleRequest(Utilities.GenerateHash($"{SharedContext.CurrentUser.AssociateId}_{DateTime.Now.ToString()}"));

                    //string requestLocation = @"bundlework/intake/" + _request.RequestGuid;

                    _request.DatasetID = files.FirstOrDefault().Dataset.DatasetId;
                    _request.Bucket = Configuration.Config.GetHostSetting("AWSRootBucket");
                    _request.DatasetFileConfigId = files.FirstOrDefault().DatasetFileConfig.ConfigId;
                    _request.TargetFileName = newName;
                    _request.Email = userEmail;
                    _request.TargetFileLocation = Configuration.Config.GetSetting("S3BundlePrefix") + parentDataset.S3Key;
                    _request.DatasetDropLocation = files.FirstOrDefault().DatasetFileConfig.RetrieverJobs.FirstOrDefault(x => x.DataSource.Is<DfsBasic>()).GetUri().LocalPath;
                    _request.RequestInitiatorId = SharedContext.CurrentUser.AssociateId;

                    foreach (DatasetFile df in files)
                    {
                        _request.SourceKeys.Add(Tuple.Create(df.FileLocation, df.VersionId));
                    }

                    _request.FileExtension = Path.GetExtension(_request.SourceKeys.FirstOrDefault().Item1);

                    string jsonRequest = JsonConvert.SerializeObject(_request, Formatting.Indented);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        StreamWriter writer = new StreamWriter(ms);

                        writer.WriteLine(jsonRequest);
                        writer.Flush();

                        //You have to rewind the MemoryStream before copying
                        ms.Seek(0, SeekOrigin.Begin);
                        Logger.Info($"Sending Bundle Request to:{Path.Combine($"{Configuration.Config.GetHostSetting("DatasetBundleBaseLocation")}", "request", $"{_request.RequestGuid}.json")}");

                        using (FileStream fs = new FileStream(Path.Combine($"{Configuration.Config.GetHostSetting("DatasetBundleBaseLocation")}", "request", $"{_request.RequestGuid}.json"), FileMode.CreateNew))
                        {
                            ms.CopyTo(fs);
                            fs.Flush();
                        }

                    }

                    //Create Bundle Started Event
                    Event e = new Event();
                    e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault();
                    e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Started").FirstOrDefault();
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.UserWhoStartedEvent = _request.RequestInitiatorId;
                    e.Dataset = _request.DatasetID;
                    e.DataConfig = _request.DatasetFileConfigId;
                    e.Reason = $"Submitted bundle request for dataset [<b>{_datasetContext.GetById(_request.DatasetID).DatasetName}</b>] targeting file name [<b>{_request.TargetFileName}</b>]";
                    e.Parent_Event = _request.RequestGuid;
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
                    
                    return Json(new { Success = true, Message = "Successfully sent request to Dataset Bundler.  You will recieve notification when completed." });
                }
                else
                {
                    //Return an error to the user in the Client UI.
                    return Json(new { Success = false, Message = errorString });
                }
            }
            catch(Exception ex)
            {
                Logger.Error("Error Processing Bundle Request", ex);
                return Json(new { Success = false, Message = "An error occurred, please try later! : " + ex.Message });
            }
        }

        [HttpGet()]
        public int GetLatestDatasetFileIdForDataset(int id)
        {
            int dfid = _datasetContext.GetLatestDatasetFileIdForDataset(id);
            return dfid;
        }

        [HttpPost]
        public void CreateFilePath(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.Upload)]
        public ActionResult UploadDatafile(int id, int configId)
        {//JsonResult
            if (Request.Files.Count > 0 && id != 0)
            {
                DatasetFileConfig dfc = _datasetContext.getDatasetFileConfigs(configId);

                IApplicationUser user = _userService.GetCurrentUser();

                LoaderRequest loadReq = null;

                try
                {
                    if (user.CanUpload)
                    {
                        HttpFileCollectionBase files = Request.Files;

                        HttpPostedFileBase file = files[0];

                        string dsfi;

                        //Adding ProcessedFilePrefix so GoldenEye Watch.cs does not pick up the file since we will create a Dataset Loader request
                        dsfi = Sentry.Configuration.Config.GetHostSetting("ProcessedFilePrefix") + System.IO.Path.GetFileName(file.FileName);

                        if (dfc != null)
                        {
                            //Create Dataset Loader request
                            //Find job DFSBasic generic job associated with this config and add ID to request.
                            RetrieverJob dfsBasicJob = null;
                            List<RetrieverJob> jobList = _requestContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == configId).ToList();
                            bool jobFound = false;
                            foreach (RetrieverJob job in jobList)
                            {
                                if (job.DataSource.Is<DfsBasic>())
                                {
                                    dfsBasicJob = job;
                                    jobFound = true;
                                    break;
                                }
                            }

                            if (!jobFound)
                            {
                                throw new NotImplementedException("Failed to find generic DFS Basic job");
                            }

                            string fileDropLocation = Path.Combine(dfsBasicJob.GetUri().LocalPath, dsfi);

                            using (Stream sfile = file.InputStream)
                            {
                                Logger.Debug($"Upload input stream|length:{sfile.Length.ToString()}|DropPath:{fileDropLocation}|TgtFileName:{dsfi}");
                                using (Stream fileStream = new FileStream(fileDropLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                                {
                                    sfile.CopyTo(fileStream);
                                }
                            }

                            var hashInput = $"{user.AssociateId.ToString()}_{DateTime.Now.ToString("MM-dd-yyyyHH:mm:ss.fffffff")}_{dsfi}";

                            loadReq = new LoaderRequest(Utilities.GenerateHash(hashInput));
                            loadReq.File = fileDropLocation;
                            loadReq.IsBundled = false;
                            loadReq.DatasetID = dfc.ParentDataset.DatasetId;
                            loadReq.DatasetFileConfigId = dfc.ConfigId;
                            loadReq.RetrieverJobId = dfsBasicJob.Id;
                            loadReq.RequestInitiatorId = user.AssociateId;

                            Logger.Debug($"Submitting Loader Request - File:{dsfi} Guid:{loadReq.RequestGuid} HashInput:{hashInput}");

                            string jsonReq = JsonConvert.SerializeObject(loadReq, Formatting.Indented);

                            //Send request to DFS location loader service is watching for requests
                            using (MemoryStream ms = new MemoryStream())
                            {
                                StreamWriter writer = new StreamWriter(ms);

                                writer.WriteLine(jsonReq);
                                writer.Flush();

                                //You have to rewind the MemoryStream before copying
                                ms.Seek(0, SeekOrigin.Begin);

                                using (FileStream fs = new FileStream($"{Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath")}{loadReq.RequestGuid}.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    ms.CopyTo(fs);
                                    fs.Flush();
                                }
                            }

                            //Create Bundle Success Event
                            Event e = new Event();
                            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault();
                            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Started").FirstOrDefault();
                            e.TimeCreated = DateTime.Now;
                            e.TimeNotified = DateTime.Now;
                            e.IsProcessed = false;
                            e.UserWhoStartedEvent = loadReq.RequestInitiatorId;
                            e.Dataset = loadReq.DatasetID;
                            e.DataConfig = loadReq.DatasetFileConfigId;
                            e.Reason = $"Successfully submitted requset to load file [<b>{System.IO.Path.GetFileName(file.FileName)}</b>] to dataset [<b>{dfc.ParentDataset.DatasetName}</b>]";
                            e.Parent_Event = loadReq.RequestGuid;
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                            return Json("File Successfully Sent to Dataset Loader.");
                        }
                        else
                        {
                            Logger.Debug("File did not match a configuration file");
                            return Json("File did not match a configuration file.");
                        }
                    }
                    else
                    {
                        Logger.Debug("User Unable to upload files to dataset");
                        return Json("You cannot upload files to this dataset.");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error occurred", e);
                    return Json("Error occurred: " + e.Message);
                }                
            }
            else
            {
                if (id == 0)
                {
                    Logger.Debug("No Dataset Selected");
                    return Json("No Dataset Selected");
                }
                else
                {
                    Logger.Debug("No files selected");
                    return Json("No files selected");
                }                
            }
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.Upload)]
        public ActionResult GetDatasetUploadPartialView(int datasetId)
        {
            CreateDataFileModel cd = new CreateDataFileModel();
            //If a value was passed, load appropriate information
            if (datasetId != 0)
            {
                cd = new CreateDataFileModel(_datasetContext.GetById(datasetId), _associateInfoProvider);
            }
            
            ViewBag.Categories = Utility.GetCategoryList(_datasetContext);

            return PartialView("_UploadDataFile", cd);
        }

        #endregion

        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        [HttpPost]
        public ActionResult RunRetrieverJob(int id)
        {
            try
            {
                BackgroundJob.Enqueue<RetrieverJobService>(RetrieverJobService => RetrieverJobService.RunRetrieverJob(id, JobCancellationToken.Null, null));
            }
            catch(Exception ex)
            {
                Logger.Error($"Error Enqueing Retriever Job ({id}).", ex);
                return Json(new { Success = false, Message = "Failed to queue job, please try later! Contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a> if problem persists." });
            }

            return Json(new { Success = true, Message = "Job successfully queued." });
        }

        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        [HttpPost]
        public ActionResult DisableRetrieverJob(int id)
        {
            try
            {
                RetrieverJobService jobservice = new RetrieverJobService();

                jobservice.DisableJob(id);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error disabling retriever job ({id}).", ex);
                return Json(new { Success = false, Message = "Failed disabling job.  If problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>." });
            }

            return Json(new { Success = true, Message = "Job has been marked as disabled and will be removed from the job scheduler." });
        }

        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        [HttpPost]
        public ActionResult EnableRetrieverJob(int id)
        {
            try
            {
                RetrieverJobService jobservice = new RetrieverJobService();

                jobservice.EnableJob(id);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error enabling retriever job ({id}).", ex);
                return Json(new { Success = false, Message = "Failed enabling job.  If problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>." });
            }

            return Json(new { Success = true, Message = "Job has been marked as enabled and will be added to the job scheduler." });
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult LoadDatasetList(int id)
        {
            IEnumerable<Dataset> dfList = Utility.GetDatasetByCategoryId(_datasetContext, id);

            SelectListGroup group = new SelectListGroup(){ Name = _datasetContext.GetCategoryById(id).Name };

            IEnumerable<SelectListItem> sList = dfList.Select(m => new SelectListItem()
            {
                Text = m.DatasetName,
                Value = m.DatasetId.ToString(),
                Group = group
            });

            return Json(sList, JsonRequestBehavior.AllowGet);
        }       

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DwnldNonSensitive)]
        public JsonResult GetUserGuide(string key)
        {
            try
            {

                JsonResult jr = new JsonResult();
                jr.Data = _s3Service.GetUserGuideDownloadURL(key, "application\\pdf");
                jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jr;
            }
            catch
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(new { message = "Encountered Error Retrieving File.<br />If this problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult GetAllDatasets()
        {
            var list = _datasetContext.Categories;

            List<SelectListItem> sList = new List<SelectListItem>();

            foreach (var cat in list)
            {
                IEnumerable<Dataset> dfList = Utility.GetDatasetByCategoryId(_datasetContext, cat.Id);

                SelectListGroup group = new SelectListGroup() { Name = cat.Name };

                sList.AddRange(dfList.Select(m => new SelectListItem()
                {
                    Text = m.DatasetName,
                    Value = m.DatasetId.ToString(),
                    Group = group
                }));
            }

            return Json(sList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetSourceDescription(string DiscrimatorValue)
        {
            var obj = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == DiscrimatorValue).Select(x => x.Description);

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        
        [AuthorizeByPermission(PermissionNames.QueryToolPowerUser)]
        public ActionResult QueryTool()
        {
            ViewBag.PowerUser = SharedContext.CurrentUser.CanQueryToolPowerUser;
            ViewBag.LivyURL = Sentry.Configuration.Config.GetHostSetting("ApacheLivy");

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Query Tool Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View("QueryTool");
        }

        public JsonResult SetFavorite(int datasetId)
        {
            try
            {
                Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

                if (!ds.Favorities.Any(w => w.UserId == SharedContext.CurrentUser.AssociateId))
                {
                    Favorite f = new Favorite()
                    {
                        DatasetId = ds.DatasetId,
                        UserId = SharedContext.CurrentUser.AssociateId,
                        Created = DateTime.Now
                    };

                    _datasetContext.Merge(f);
                    _datasetContext.SaveChanges();

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return Json(new { message = "Successfully added favorite." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _datasetContext.Remove(ds.Favorities.First(w => w.UserId == SharedContext.CurrentUser.AssociateId));
                    _datasetContext.SaveChanges();

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return Json(new { message = "Successfully removed favorite." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                _datasetContext.Clear();
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Failed to modify favorite." }, JsonRequestBehavior.AllowGet);
            }
            
        }
    }
}