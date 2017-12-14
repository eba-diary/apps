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

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class DatasetController : BaseController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        private UserService _userService;
        private S3ServiceProvider _s3Service;
        private ISASService _sasService;
        private IAppCache _cache;

        private static Amazon.S3.IAmazonS3 _s3client = null;


        // JCG TODO: Revisit, Could this be push down into the Infrastructure\Core layer? 
        private Amazon.S3.IAmazonS3 S3Client
        {
            get
            {
                throw new NotImplementedException();
                if (null == _s3client)
                {
                    // instantiate a new shared client
                    // TODO: move this all to the config(s)...
                    AWSConfigsS3.UseSignatureVersion4 = true;
                    AmazonS3Config s3config = new AmazonS3Config();
                    s3config.RegionEndpoint = RegionEndpoint.GetBySystemName(Configuration.Config.GetSetting("AWSRegion"));
                    //s3config.UseHttp = true;
                    s3config.ProxyHost = Configuration.Config.GetHostSetting("SentryS3ProxyHost");
                    s3config.ProxyPort = int.Parse(Configuration.Config.GetSetting("SentryS3ProxyPort"));
                    s3config.ProxyCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    string awsAccessKey = Configuration.Config.GetSetting("AWSAccessKey");
                    string awsSecretKey = Configuration.Config.GetSetting("AWSSecretKey");
                    _s3client = new AmazonS3Client(awsAccessKey, awsSecretKey, s3config);
                }
                return _s3client;
            }
        }

        private List<BaseDatasetModel> _dsModelList = null;

        // JCG TODO: Add unit test for dsModelList getter
        private List<BaseDatasetModel> dsModelList
        {
            get
            {
                if (_dsModelList == null)
                {
                    List<BaseDatasetModel> dsmList = new List<BaseDatasetModel>();
                    IEnumerable<Dataset> dsList = _datasetContext.Datasets.AsEnumerable();

                    foreach (Dataset ds in dsList)
                    {
                        BaseDatasetModel dsModel = new BaseDatasetModel(ds, _associateInfoProvider);
                        dsModel.CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
                        dsModel.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
                        dsModel.CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs;
                        dsModel.CanDwnldNonSensitive = SharedContext.CurrentUser.CanDwnldNonSensitive;
                        dsModel.CanUpload = SharedContext.CurrentUser.CanUpload;
                        dsmList.Add(dsModel);
                    }
                    _dsModelList = dsmList;
                }
                return _dsModelList;
            }
        }

        public DatasetController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, ISASService sasService, IAssociateInfoProvider associateInfoService)
        {
            _cache = new CachingService();
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            _associateInfoProvider = associateInfoService;
        }

        // GET: Dataset
        public ActionResult Index()
        {
            List<BaseDatasetModel> dsList = GetDatasetModelList();
            return View(dsList);
        }

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult HomeDataset()
        {
            List<Category> categories = _datasetContext.Categories.ToList();
            ViewData["dsCount"] = _datasetContext.GetDatasetCount();
            ViewData["CanEditDataset"] = SharedContext.CurrentUser.CanEditDataset;
            ViewData["CanUpload"] = SharedContext.CurrentUser.CanUpload;

            foreach (Category c in categories) //parallel?
            {
                ViewData[c.Name + "Count"] = _datasetContext.GetCategoryDatasetCount(c);
            }

            return PartialView("_HomeDataset", categories.OrderBy(o => o.Name).ToList());
        }

        #region Search
        // GET: Dataset/List/searchParms
        [Route("Dataset/List/Index")]
        [Route("Dataset/List/SearchPhrase")]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult List(string category, string searchPhrase, string ids)
        {
            //Helpers.Search searchHelper = new Helpers.Search(_cache);

            //return View(searchHelper.List(this, null, searchPhrase, category, ids));

            return View("ClientSideList");
        }

        [Route("Dataset/DatasetList")]
        public JsonResult DatasetList()
        {

            List<SearchModel> models = new List<SearchModel>();
            foreach(BaseDatasetModel bdm in GetDatasetModelList())
            {

                SearchModel sm = new SearchModel();

                sm.DatasetId = bdm.DatasetId;
                sm.Category = bdm.Category;
                sm.DatasetName = bdm.DatasetName;
                sm.DatasetDesc = bdm.DatasetDesc;
                sm.DatasetInformation = bdm.DatasetInformation;
                sm.SentryOwnerName = bdm.SentryOwner.FullName;

                sm.Frequencies = bdm.CreationFreqDesc;
                sm.DistinctFileExtensions = bdm.DistinctFileExtensions();

                sm.IsSensitive = bdm.IsSensitive;
                sm.ChangedDtm = bdm.ChangedDtm.ToShortDateString();

                sm.BannerColor = "categoryBanner-" + bdm.DatasetCategory.Color;
                sm.BorderColor = "borderSide_" + bdm.DatasetCategory.Color;
                sm.Color = bdm.DatasetCategory.Color;



                models.Add(sm);
            }

            return Json(models, JsonRequestBehavior.AllowGet);
        }

        [Route("Dataset/List")]
        public ActionResult List(ListDatasetModel ldm)
        {
            //Helpers.Search searchHelper = new Helpers.Search(_cache);

           // Debug.WriteLine(Request.Url.AbsoluteUri);

            return View("ClientSideList");
        }
        #endregion

        #region Dataset Modification

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Create()
        {
            CreateDatasetModel udm = new CreateDatasetModel();
            udm.AllCategories = GetCategoryList();
            udm.AllFrequencies = GetDatasetFrequencyListItems();  //load all values for dropdown
            udm.AllOriginationCodes = GetDatasetOriginationListItems(); //load all values for dropdown
            udm.AllDatasetScopeTypes = GetDatasetScopeTypesListItems();
            udm.FreqencyID = 6; // preselected NonSchedule            
            return View(udm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Create(CreateDatasetModel cdm)
        {

            if (_datasetContext.isDatasetNameDuplicate(cdm.DatasetName, _datasetContext.GetCategoryById(cdm.CategoryIDs).Name))
            {
                AddCoreValidationExceptionsToModel(new ValidationException("Dataset name already exists within category"));
            }
            try
            {
                if (ModelState.IsValid)
                {
                    Dataset ds = CreateDatasetFromModel(cdm);

                    //IApplicationUser user = _userService.GetCurrentUser();
                    //DateTime CreateTime = DateTime.Now;

                    ds = _datasetContext.Merge<Dataset>(ds);

                    List<DatasetFileConfig> dfcList = new List<DatasetFileConfig>();
                    //Create Generic Data File Config for Dataset                    
                    DatasetFileConfig dfc = new DatasetFileConfig(
                        0,
                        "Default",
                        "Default Config for Dataset.  Uploaded files that do not match any configs will default to this config",
                        "\\.",
                        "DFS",
                        ds.DropLocation + "default\\",
                        true,
                        true,
                        (int)FileType.DataFile,
                        true,
                        ds,
                        Enum.GetName(typeof(DatasetFrequency), cdm.FreqencyID),
                        cdm.DatasetScopeTypeID
                        );

                    dfc.DatasetScopeType = _datasetContext.GetAllDatasetScopeTypes().Where(x => x.ScopeTypeId == cdm.DatasetScopeTypeID).First();

                    _datasetContext.Merge(dfc);
                    _datasetContext.SaveChanges();

                    //create drop locations
                    try
                    {
                        //Config Drop location
                        if (!System.IO.Directory.Exists(dfc.DropPath))
                        {
                            System.IO.Directory.CreateDirectory(dfc.DropPath);
                        }

                        //Bundle Drop location
                        if (!Directory.Exists(ds.DropLocation + "bundle\\"))
                        {
                            Directory.CreateDirectory(ds.DropLocation + "bundle\\");
                        }
                    }
                    catch (Exception e)
                    {

                        StringBuilder errmsg = new StringBuilder();
                        errmsg.AppendLine("Failed to Create Drop Location:");
                        errmsg.AppendLine($"DatasetId: {ds.DatasetId}");
                        errmsg.AppendLine($"DatasetName: {ds.DatasetName}");
                        errmsg.AppendLine($"DropLocation: {dfc.DropPath}");
                        errmsg.AppendLine($"BundleDropLocation: {ds.DropLocation + "bundle\\"}");

                        Logger.Error(errmsg.ToString(), e);
                    }

                    int maxId = _datasetContext.GetMaxId();
                    return RedirectToAction("Detail", new { id = maxId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _datasetContext.Clear();
                cdm.AllCategories = GetCategoryList();  //Reload dropdown value list
                //cdm.AllFrequencies = GetDatasetFrequencyListItems();  //Reload dropdown value list
                cdm.AllOriginationCodes = GetDatasetOriginationListItems(); //Reload dropdown value list
                //cdm.AllDatasetScopeTypes = GetDatasetScopeTypesListItems();
                cdm.AllFrequencies = GetDatasetFrequencyListItems();  //load all values for dropdown
                cdm.AllDatasetScopeTypes = GetDatasetScopeTypesListItems();
                cdm.OriginationID = 1; // preselected Internal
            }

            return View(cdm);
        }

        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        private Dataset CreateDatasetFromModel(CreateDatasetModel cdm)
        {
            DateTime CreateTime = DateTime.Now;
            string cat = _datasetContext.GetCategoryById(cdm.CategoryIDs).Name;
            string freqName = Enum.GetName(typeof(DatasetFrequency), cdm.FreqencyID);
            IApplicationUser user = _userService.GetCurrentUser();
            Dataset ds = new Dataset(
                0,
                cat,
                cdm.DatasetName,
                cdm.DatasetDesc,
                "This is a test Information line.",
                cdm.CreationUserName,
                cdm.SentryOwnerName,
                user.AssociateId,
                Enum.GetName(typeof(DatasetOriginationCode), cdm.OriginationID),
                CreateTime,
                CreateTime,
                //freqName,
                Utilities.GenerateDatasetStorageLocation(cat, cdm.DatasetName),
                false,
                true,
                null,
                _datasetContext.GetCategoryById(cdm.CategoryIDs),
                null,
                //_datasetContext.GetDatasetScopeById(cdm.DatasetScopeTypeID),
                //cdm.DatafilesFilesToKeep,
                null,
                Utilities.GenerateDatasetDropLocation(cat, cdm.DatasetName));

            return ds;
        }

        // GET: DatasetFileVersion/Edit/5
        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Edit(int id)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(id);
            EditDatasetModel item = new EditDatasetModel(ds, _associateInfoProvider);
            //item.AllFrequencies = GetDatasetFrequencyListItems();  // Load dropdown value list
            //item.FreqencyID = (int)(Enum.Parse(typeof(DatasetFrequency), ds.CreationFreqDesc));  //Preselect current value
            item.AllOriginationCodes = GetDatasetOriginationListItems();
            item.OriginationID = (int)(Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode));  //Preselect current value
            //item.AllDatasetScopeTypes = GetDatasetScopeTypesListItems();
            //item.DatasetScopeTypeID = ds.DatasetScopeType.ScopeTypeId;
            return View(item);
        }

        // POST: Dataset/Edit/5
        [HttpPost()]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult Edit(int id, EditDatasetModel i)
        {
            try
            {
                Dataset item = _datasetContext.GetById<Dataset>(id);
                if (ModelState.IsValid)
                {
                    UpdateDatasetFromModel(item, i);
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
                i.AllFrequencies = GetDatasetFrequencyListItems();  //Reload dropdown value list
                i.AllOriginationCodes = GetDatasetOriginationListItems(); //Reload dropdown value list
                i.AllDatasetScopeTypes = GetDatasetScopeTypesListItems(); //Reload dropdown value list
            }

            return View(i);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        private void UpdateDatasetFromModel(Dataset ds, EditDatasetModel eds)
        {
            DateTime now = DateTime.Now;

            string frequency = ((DatasetFrequency)eds.FreqencyID).ToString();
            string originationcode = ((DatasetOriginationCode)eds.OriginationID).ToString();

            if (eds.DatasetInformation != null && eds.DatasetInformation.Length > 0)
            {
                ds.DatasetInformation = eds.DatasetInformation;
            }

            ds.ChangedDtm = now;
            if (null != eds.Category && eds.Category.Length > 0) ds.Category = eds.Category;
            //ds.CreationFreqDesc = frequency;
            if (null != eds.CreationUserName && eds.CreationUserName.Length > 0) ds.CreationUserName = eds.CreationUserName;
            if (null != eds.DatasetDesc && eds.DatasetDesc.Length > 0) ds.DatasetDesc = eds.DatasetDesc;
            if (eds.DatasetDtm > DateTime.MinValue) ds.DatasetDtm = eds.DatasetDtm;
            if (null != eds.DatasetName && eds.DatasetName.Length > 0) ds.DatasetName = eds.DatasetName;
            ds.OriginationCode = originationcode;
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


            //if (eds.DatasetScopeTypeID != ds.DatasetScopeType.ScopeTypeId) { ds.DatasetScopeType = _datasetContext.GetDatasetScopeById(eds.DatasetScopeTypeID); }                        
            ds.S3Key = Utilities.GenerateDatasetStorageLocation(ds.Category, ds.DatasetName);
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


            if (dfgm.Information != null && dfgm.Information.Length > 0)
            {
                df.Information = dfgm.Information;
            }

            df.IsUsable = dfgm.IsUsable;

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
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Detail(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            // IList<String> catList = _datasetContext.GetCategoryList();
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
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Subscribe(int id)
        {
            SubscriptionModel sm = new SubscriptionModel();

            sm.AllEventTypes = _datasetContext.GetAllEventTypes().Select((c) => new SelectListItem { Text = c.Description, Value = c.Type_ID.ToString() });
            sm.AllIntervals = _datasetContext.GetAllIntervals().Select((c) => new SelectListItem { Text = c.Description, Value = c.Interval_ID.ToString() });

            sm.CurrentSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, id);

            sm.datasetID = id;

            sm.SentryOwnerName = _userService.GetCurrentUser().AssociateId;


            foreach (Core.EventType et in _datasetContext.GetAllEventTypes())
            {
                if(!sm.CurrentSubscriptions.Any(x => x.EventType.Type_ID == et.Type_ID))
                {
                    DatasetSubscription subscription = new DatasetSubscription();

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
                            _datasetContext.GetEventType(sub.EventType.Type_ID),
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
            foreach (DatasetFile df in _datasetContext.GetDatasetFilesForDataset(Id, x => !x.IsBundled).ToList())
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(df, _associateInfoProvider);
                dfgm.CanDwnldNonSensitive = CanDwnldNonSensitive;
                dfgm.CanDwnldSenstive = CanDwnldSenstive;
                dfgm.CanEdit = CanEdit;
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

            foreach (DatasetFile df in _datasetContext.GetDatasetFilesForDataset(Id, x => x.IsBundled).ToList())
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(df, _associateInfoProvider);
                dfgm.CanDwnldNonSensitive = CanDwnldNonSensitive;
                dfgm.CanDwnldSenstive = CanDwnldSenstive;
                dfgm.CanEdit = CanEdit;
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
                files.Add(dfgm);
            }

            //IEnumerable<DatasetFileGridModel> files = _datasetContext.GetDatasetFilesVersions(df.Dataset.DatasetId, df.DatasetFileConfig.DataFileConfigId, df.FileName).ToList().
            //    Select((f) => new DatasetFileGridModel(f));

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
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public JsonResult GetDatasetFileConfigInfo(int Id)
        {
            IEnumerable<DatasetFileConfigsModel> files = null;
            if (Id > 0)
            {
                files = _datasetContext.GetById(Id).DatasetFileConfigs.Select((f) => new DatasetFileConfigsModel(f));
            }
            else
            {
                files = _datasetContext.getAllDatasetFileConfigs().Select((f) => new DatasetFileConfigsModel(f));
            }

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public JsonResult GetDatasetFileConfigInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            IEnumerable<DatasetFileConfigsModel> files = null;
            if (Id > 0)
            {
                files = _datasetContext.GetById(Id).DatasetFileConfigs.Select((f) => new DatasetFileConfigsModel(f));
            }
            else
            {
                files = _datasetContext.getAllDatasetFileConfigs().Select((f) => new DatasetFileConfigsModel(f));
            }
            //= _datasetContext.Datasets.Where(w => w.DatasetId == Id).FirstOrDefault().DatasetFileConfigs.ToList().
            //        Select((f) => new DatasetFileConfigsModel(f));

            DataTablesQueryableAdapter<DatasetFileConfigsModel> dtqa = new DataTablesQueryableAdapter<DatasetFileConfigsModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Create Data File Configuration
        [HttpGet]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult CreateDataFileConfig(int id)
        {
            Dataset parent = _datasetContext.GetById<Dataset>(id);

            DatasetFileConfigsModel dfcm = new DatasetFileConfigsModel();
            dfcm.DatasetId = id;
            dfcm.ParentDatasetName = parent.DatasetName;


            dfcm.AllFrequencies = GetDatasetFrequencyListItems();  //load all values for dropdown
                                                                    //dfcm.AllOriginationCodes = GetDatasetOriginationListItems(); //load all values for dropdown
            dfcm.AllDatasetScopeTypes = GetDatasetScopeTypesListItems();
            dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                    => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            dfcm.DropPath = parent.DropLocation;
            dfcm.DropLocationType = "DFS";

            dfcm.SearchCriteria = "\\.";
            dfcm.CreationFreq = DatasetFrequency.NonSchedule.ToString();
            

            return View(dfcm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.DatasetEdit)]
        public ActionResult CreateDataFileConfig(DatasetFileConfigsModel dfcm)
        {

            Dataset parent = _datasetContext.GetById<Dataset>(dfcm.DatasetId);

            if (parent.DatasetFileConfigs.Any(x => x.Name == dfcm.ConfigFileName))
            {
                AddCoreValidationExceptionsToModel(new ValidationException("Dataset config with that name already exists within dataset"));
            }


            try
            {
                if (ModelState.IsValid)
                {
                    List<DatasetFileConfig> dfcList = parent.DatasetFileConfigs.ToList();

                    //Create Generic Data File Config for Dataset                    
                    DatasetFileConfig dfc = new DatasetFileConfig(
                        0,
                        dfcm.ConfigFileName,
                        dfcm.ConfigFileDesc,
                        dfcm.SearchCriteria,
                        dfcm.DropLocationType,
                        dfcm.DropPath,
                        dfcm.IsRegexSearch,
                        dfcm.OverwriteDatasetFile,
                        dfcm.FileTypeId,
                        true,
                        parent,
                        dfcm.CreationFreq.ToString(),
                        dfcm.DatasetScopeTypeID
                        );

                    dfc.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dfcm.DatasetScopeTypeID);

                    dfcList.Add(dfc);
                    parent.DatasetFileConfigs = dfcList;

                    _datasetContext.Merge<Dataset>(parent);
                    // _datasetContext.Merge(dfc);
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

                    return RedirectToAction("Detail", new { id = parent.DatasetId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _datasetContext.Clear();
                dfcm.AllFrequencies = GetDatasetFrequencyListItems();  //load all values for dropdown
                                                                       //dfcm.AllOriginationCodes = GetDatasetOriginationListItems(); //load all values for dropdown
                dfcm.AllDatasetScopeTypes = GetDatasetScopeTypesListItems();
                dfcm.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                        => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            }

            return View(dfcm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult EditDataFileConfig()
        {
            DatasetFileConfigsModel edfc = new DatasetFileConfigsModel();

            return View(edfc);
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult GetEditConfigPartialView(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.getDatasetFileConfigs(configId);
            EditDatasetFileConfigModel edfc = new EditDatasetFileConfigModel(dfc);

            edfc.AllFrequencies = GetDatasetFrequencyListItems(edfc.CreationFreq);  //load all values for dropdown
            //dfcm.AllOriginationCodes = GetDatasetOriginationListItems(); //load all values for dropdown
            edfc.AllDatasetScopeTypes = GetDatasetScopeTypesListItems(edfc.DatasetScopeTypeID);
            edfc.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            ViewBag.ModifyType = "Edit";

            return PartialView("_EditConfigFile", edfc);
        }

        [HttpPost()]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult ModifyConfigFile(EditDatasetFileConfigModel edfc)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(edfc.ConfigId);

            try
            {
                if (ModelState.IsValid)
                {
                    dfc = UpdateDatasetFileConfigFromModel(dfc, edfc);
                    _datasetContext.SaveChanges();
                    return AjaxSuccessJson();
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _datasetContext.Clear();
            }

            //Return partial view when there are errors
            return GetEditConfigPartialView(edfc.ConfigId);
        }

        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        private DatasetFileConfig UpdateDatasetFileConfigFromModel(DatasetFileConfig dfc, EditDatasetFileConfigModel edfc)
        {
            dfc.SearchCriteria = edfc.SearchCriteria;
            dfc.TargetFileName = edfc.TargetFileName;
            dfc.IsRegexSearch = edfc.IsRegexSearch;
            dfc.OverwriteDatafile = edfc.OverwriteDatasetFile;
            dfc.CreationFreqDesc = edfc.CreationFreq;
            dfc.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(edfc.DatasetScopeTypeID);
            dfc.FileTypeId = edfc.FileTypeId;

            return dfc;
        }

        #endregion

        #region Helpers

        public List<BaseDatasetModel> GetDatasetModelList()
        {
            return dsModelList;
        }

        public BaseDatasetModel GetDatasetModel(int id)
        {
            BaseDatasetModel ds = dsModelList.FirstOrDefault(m => m.DatasetId == id);
            return ds;
        }

        // JCG TODO: Add unit tests for [GET]GetDownloadURL
        [AuthorizeByPermission(PermissionNames.DwnldSensitve, PermissionNames.DwnldNonSensitive)]
        [HttpGet()]
        public JsonResult GetDownloadURL(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            JsonResult jr = new JsonResult();
            jr.Data = _s3Service.GetDatasetDownloadURL(ds.S3Key);
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jr;
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DwnldNonSensitive)]
        public JsonResult GetDatasetFileDownloadURL(int id)
        {
            DatasetFile df = _datasetContext.GetDatasetFile(id);
            JsonResult jr = new JsonResult();
            jr.Data = _s3Service.GetDatasetDownloadURL(df.FileLocation, df.VersionId);
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            return jr;
        }

        /// <summary>
        /// Callback handler for S3 uploads... Amazon calls this to communicate progress; from here we communicate
        /// that progress back to the client for their progress bar...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void uploadRequest_UploadPartProgressEvent(object sender, TransferProgressEventArgs e)
        {
            throw new NotImplementedException();
            Sentry.data.Web.Helpers.ProgressUpdater.SendProgress(e.FilePath, e.PercentDone);

            if (e.PercentDone % 10 == 0)
            {
                Sentry.Common.Logging.Logger.Debug("DatasetUpload-S3Event: " + e.FilePath + ": " + e.PercentDone);
            }

        }


        /// <summary>
        /// Callback handler for S3 uploads... Amazon calls this to communicate progress; from here we communicate
        /// that progress back to the client for their progress bar...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void PushToSAS_ProgressEvent(object sender, TransferProgressEventArgs e)
        {
            throw new NotImplementedException();
            int percent = 0;

            object x = e.GetType();

            if (e.Type == "Downloading")
            {
                percent = Convert.ToInt32(Convert.ToDouble(e.PercentDone) * 0.9);

                if (percent % 10 == 0)
                {
                    Sentry.Common.Logging.Logger.Debug("PushToSAS_ProgressEvent: " + e.Type + ":" + e.FilePath + ": " + percent);
                }
            }

            if (e.Type == "Converting")
            {
                percent = Convert.ToInt32(Convert.ToDouble(e.PercentDone) * 0.1) + 90;

                if (percent % 10 == 0)
                {
                    Sentry.Common.Logging.Logger.Debug("PushToSAS_ProgressEvent: " + e.Type + ":" + e.FilePath + ": " + percent);
                }
            }

            Sentry.data.Web.Helpers.ProgressUpdater.SendProgress(e.FilePath, percent);

            //if (percent % 10 == 0)
            //{
            //    Sentry.Common.Logging.Logger.Debug("PushToSAS_ProgressEvent: " + e.Type + ":" + e.FilePath + ": " + percent);
            //}

        }


        private IEnumerable<SelectListItem> GetCategoryList()
        {
            IEnumerable<SelectListItem> var = _datasetContext.Categories.OrderByHierarchy().Select((c) => new SelectListItem { Text = c.FullName, Value = c.Id.ToString() });

            return var;
        }

        private IEnumerable<Dataset> GetDatasetByCategoryId(int id)
        {
            IEnumerable<Dataset> dsQ = _datasetContext.GetDatasetByCategoryID(id);
            return dsQ;
        }


        private IEnumerable<SelectListItem> GetDatasetFrequencyListItems(string freq = null)
        {
            List<SelectListItem> items;

            if (freq == null)
            {
                items = Enum.GetValues(typeof(DatasetFrequency)).Cast<DatasetFrequency>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            }
            else
            {
                items = Enum.GetValues(typeof(DatasetFrequency)).Cast<DatasetFrequency>().Select(v => 
                    new SelectListItem {
                        Selected = (v.ToString() == freq),
                        Text = v.ToString(),
                        Value = v.ToString() }).ToList();
            }
            return items;
        }

        private IEnumerable<SelectListItem> GetDatasetOriginationListItems()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(DatasetOriginationCode)).Cast<DatasetOriginationCode>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
        }

        private IEnumerable<SelectListItem> GetDatasetScopeTypesListItems(int id = -1)
        {
            IEnumerable<SelectListItem> dScopeTypes;
            if (id == -1)
            {
                dScopeTypes = _datasetContext.GetAllDatasetScopeTypes()
                    .Select((c) => new SelectListItem { Text = c.Name, Value = c.ScopeTypeId.ToString() });
            }
            else
            {
                dScopeTypes = _datasetContext.GetAllDatasetScopeTypes()
                    .Select((c) => new SelectListItem {
                        Selected = (c.ScopeTypeId == id),
                        Text = c.Name,
                        Value = c.ScopeTypeId.ToString() });
            }

            return dScopeTypes;
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
        public ActionResult PushToSAS(int id, string fileOverride)
        {
            DatasetFile ds = _datasetContext.GetById<DatasetFile>(id);
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


            _s3Service.OnTransferProgressEvent += new EventHandler<TransferProgressEventArgs>(PushToSAS_ProgressEvent);
            _sasService.OnPushToProgressEvent += new EventHandler<TransferProgressEventArgs>(PushToSAS_ProgressEvent);

            try
            {

                _s3Service.TransferUtilityDownload(BaseTargetPath, ds.Dataset.Category, filename, ds.FileLocation);

            }
            catch (Exception e)
            {
                Sentry.Common.Logging.Logger.Error("S3 Download Error", e);
                return PartialView("_Success", new SuccessModel("Push to SAS Error", e.Message, false));
            }
            finally
            {
                _s3Service.OnTransferProgressEvent -= new EventHandler<TransferProgressEventArgs>(PushToSAS_ProgressEvent);
            }


            //Converting file to .sas7bdat format
            try
            {

                _sasService.ConvertToSASFormat(filename, ds.Dataset.Category);

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
            finally
            {
                _sasService.OnPushToProgressEvent -= new EventHandler<TransferProgressEventArgs>(PushToSAS_ProgressEvent);
            }


            return PartialView("_Success", new SuccessModel("Successfully Pushed File to SAS", $"Dataset file {filename_orig} has been converted to {filename.Replace(Path.GetExtension(filename), ".sas7bdat")}. The file can be found at {BaseTargetPath.Replace("\\sentry.com\appfs_nonprod", "S: ")}.", true));

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
        public PartialViewResult PreviewDatafile(int id)
        {
            PreviewDataModel model = new PreviewDataModel();

            //Dataset dataset = _datasetContext.GetById<Dataset>(id);
            string previewKey = _datasetContext.GetPreviewKey(id);
            model.PreviewData = _s3Service.GetObjectPreview(previewKey);

            return PartialView("_PreviewData", model);
        }

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public PartialViewResult PreviewLatestDatafile(int id)
        {
            PreviewDataModel model = new PreviewDataModel();

            int latestDatafile = GetLatestDatasetFileIdForDataset(id);
            string previewKey = _datasetContext.GetPreviewKey(latestDatafile);
            model.PreviewData = _s3Service.GetObjectPreview(previewKey);

            return PartialView("_PreviewData", model);
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

            Boolean bundlingSensitive = files.Any(x => x.IsSensitive);
            Boolean bundlingUnUsable = files.Any(x => !x.IsUsable);

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

            if(bundlingUnUsable)
            {
                errorsFound = true;
                errorString += "<p>The files were not all labeled usable. Bundling requires that all files be labeled usable.  Please filter using the Usable Column.</p>";
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
                    _request.DatasetDropLocation = parentDataset.DropLocation + "bundle\\";
                    _request.RequestInitiatorId = SharedContext.CurrentUser.AssociateId;

                    foreach (DatasetFile df in files)
                    {
                        _request.SourceKeys.Add(Tuple.Create(df.FileLocation, df.VersionId));
                    }

                    _request.FileExtension = Path.GetExtension(_request.SourceKeys.FirstOrDefault().Item1);

                    string jsonRequest = JsonConvert.SerializeObject(_request,Formatting.Indented);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        StreamWriter writer = new StreamWriter(ms);

                        writer.WriteLine(jsonRequest);
                        writer.Flush();

                        //You have to rewind the MemoryStream before copying
                        ms.Seek(0, SeekOrigin.Begin);

                        using (FileStream fs = new FileStream($"C:\\tmp\\DatasetBundler\\{_request.RequestGuid}.json", FileMode.OpenOrCreate))
                        {
                            ms.CopyTo(fs);
                            fs.Flush();
                        }
                    }

                    //Create Bundle Started Event
                    Event e = new Event();
                    e.EventType = _datasetContext.GetEventType(3);
                    e.Status = _datasetContext.GetStatus(1);
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.UserWhoStartedEvent = _request.RequestInitiatorId;
                    e.Dataset = _request.DatasetID;
                    e.DataConfig = _request.DatasetFileConfigId;
                    e.Reason = $"{_request.RequestGuid} : Bundle Request Sumbitted";
                    e.Parent_Event = _request.RequestGuid;
                    await Utilities.CreateEventAsync(e);
                    
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
        {
            if (Request.Files.Count > 0 && id != 0)
            {
                List<DatasetFileConfig> fcList = Utilities.LoadDatasetFileConfigsByDatasetID(id, _datasetContext);
                IApplicationUser user = _userService.GetCurrentUser();

                try
                {
                    if (user.CanUpload)
                    {
                        HttpFileCollectionBase files = Request.Files;

                        HttpPostedFileBase file = files[0];

                        string dsfi;
                        dsfi = System.IO.Path.GetFileName(file.FileName);

                        //Get Matching DataFileConfigs for file path
                        List<DatasetFileConfig> fcMatches = Utilities.GetMatchingDatasetFileConfigs(fcList, file.FileName);

                        DatasetFileConfig first = fcMatches.First(x => x.ConfigId == configId);

                        if (first != null)
                        {
                            string fileDropLocation = first.DropPath + "\\" + dsfi;


                            using (Stream sfile = file.InputStream)
                            using (Stream fileStream = new FileStream(fileDropLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                            {
                                sfile.CopyTo(fileStream);
                            }

                            return Json("File Successfully Sent to Dataset Loader with a Path of : " + fileDropLocation);
                        }
                        else
                        {
                            return Json("File did not match a configuration file.");
                        }
                    }
                    else
                    {
                        return Json("You cannot upload files to this dataset.");
                    }
                }
                catch (Exception e)
                {
                    return Json("Error occurred: " + e.Message);
                }                
            }
            else
            {
                if (id == 0)
                {
                    return Json("No Dataset Selected");
                }
                else
                {
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
            
            ViewBag.Categories = GetCategoryList();

            return PartialView("_UploadDataFile", cd);
        }

#endregion

        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public JsonResult LoadDatasetList(int id)
        {
            IEnumerable<Dataset> dfList = GetDatasetByCategoryId(id);

            IEnumerable<SelectListItem> sList = dfList.Select(m => new SelectListItem()
            {
                Text = m.DatasetName,
                Value = m.DatasetId.ToString()
            });

            return Json(sList, JsonRequestBehavior.AllowGet);
        }
    }
}
