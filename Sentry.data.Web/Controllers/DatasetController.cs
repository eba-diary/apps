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

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class DatasetController : BaseController
    {
        private IDatasetContext _datasetContext;
        private UserService _userService;
        private IDatasetService _s3Service;
        private ISASService _sasService;
        private static Amazon.S3.IAmazonS3 _s3client = null;
        //private IApiClient _apiClient;
        //private IWeatherDataProvider _weatherDataProvider;

        // JCG TODO: Revisit, Could this be push down into the Infrastructure\Core layer? 
        private Amazon.S3.IAmazonS3 S3Client
        {
            get
            {
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
                    //return DemoDatasetData;
                    // return _datasetContext.Datasets.Select((c) => new SelectListItem { Text = c.FullName, Value = c.Id.ToString() });

                    List<BaseDatasetModel> dsmList = new List<BaseDatasetModel>();
                    IEnumerable<Dataset> dsList = _datasetContext.Datasets.AsEnumerable();
                    //List<String> catList = dsList.Select(s => s.Category).Distinct().ToList();
                    foreach (Dataset ds in dsList)
                    {
                        BaseDatasetModel dsModel = new BaseDatasetModel(ds);
                        dsmList.Add(dsModel);
                        dsModel.CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
                        dsModel.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
                    }
                    _dsModelList = dsmList;
                }
                return _dsModelList;
            }
        }

        public DatasetController(IDatasetContext dsCtxt, IDatasetService dsSvc, UserService userService, ISASService sasService)
        {
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            //_weatherDataProvider = weatherDataProvider;
        }

        //private static List<BaseDatasetModel> _demoDatasetData;

        //private static List<BaseDatasetModel> DemoDatasetData
        //{
        //    get
        //    {
        //        if (_demoDatasetData == null || _demoDatasetData.Count == 0)
        //        {
        //            _demoDatasetData = new List<BaseDatasetModel>();
        //            Random rnd = new Random(1234);
        //            for (int i = 1; i <= 100; i++)
        //            {
        //                BaseDatasetModel dsm = new BaseDatasetModel();
        //                string ext = "";
        //                switch (rnd.Next(5))
        //                {
        //                    case 0: ext = ".txt"; break;
        //                    case 1: ext = ".csv"; break;
        //                    case 2: ext = ".pdf"; break;
        //                    case 3: ext = ".xlsx"; break;
        //                    default: ext = ".sas7bdat"; break;
        //                }
        //                string cat = "";
        //                switch (rnd.Next(4))
        //                {
        //                    case 0: cat = "claims"; break;
        //                    case 1: cat = "geographic"; break;
        //                    case 2: cat = "government"; break;
        //                    default: cat = "industry"; break;
        //                }
        //                string addlTag1 = "";
        //                switch (rnd.Next(10))
        //                {
        //                    case 0: addlTag1 = "weather"; break;
        //                    case 1: addlTag1 = "crime"; break;
        //                    case 2: addlTag1 = "population"; break;
        //                    case 3: addlTag1 = "ratio"; break;
        //                    case 4: addlTag1 = "energy"; break;
        //                    case 5: addlTag1 = "wealth"; break;
        //                    case 6: addlTag1 = "height"; break;
        //                    case 7: addlTag1 = "weight"; break;
        //                    case 8: addlTag1 = "age"; break;
        //                    default: addlTag1 = "salary"; break;
        //                }
        //                string addlTag2 = "";
        //                switch (rnd.Next(10))
        //                {
        //                    case 0: addlTag2 = "daily"; break;
        //                    case 1: addlTag2 = "weekly"; break;
        //                    case 2: addlTag2 = "monthly"; break;
        //                    case 3: addlTag2 = "yearly"; break;
        //                    case 4: addlTag2 = "trend"; break;
        //                    case 5: addlTag2 = "detail"; break;
        //                    case 6: addlTag2 = "summary"; break;
        //                    case 7: addlTag2 = "description"; break;
        //                    case 8: addlTag2 = "overview"; break;
        //                    default: addlTag2 = "forecast"; break;
        //                }
        //                string owner = "";
        //                switch (rnd.Next(4))
        //                {
        //                    case 0: owner = "John Schneider"; break;
        //                    case 1: owner = "Cory Woytasik"; break;
        //                    case 2: owner = "Evan Volm"; break;
        //                    default: owner = "Aaron Deering"; break;
        //                }
        //                dsm.Name = "testFile_" + cat + "_" + i.ToString("D3") + ext;
        //                dsm.UniqueKey = "testFile_UniqueKey_" + cat + "_" + i.ToString("D3");
        //                string dsc = "";
        //                switch (rnd.Next(5))
        //                {
        //                    case 0:
        //                        dsc = "This is an incredibly detailed, or maybe not, description of the file " + dsm.Name + ". " +
        //                        "Seriously - not even kidding you.. this description goes on for so long that it should wrap around in the " +
        //                        "browser and give us a look at how well our CSS handles that kind of stuff.  Dontchyaknow..."; break;
        //                    case 1: dsc = "OK Description: pretty good content here explaining more than just a little. Bot bad, eh?"; break;
        //                    case 2: dsc = "Less Bad Description: just a little detail here."; break;
        //                    case 3: dsc = "Bad Desc!"; break;
        //                    default: dsc = ""; break;
        //                }
        //                dsm.Description = dsc;
        //                dsm.SentryOwner = owner;
        //                dsm.DatasetDate = DateTime.Now.AddDays(-i).ToString();
        //                dsm.Category = cat;
        //                dsm.ETag = System.Guid.NewGuid().ToString().Replace("-","");
        //                Dictionary<string, string> md = new Dictionary<string, string>();
        //                //md.Add(Sentry.data.Web.BaseDatasetModel.METADATA_KEY_NAME, dsm.DatasetDate);
        //                //md.Add(Sentry.data.Web.BaseDatasetModel.METADATA_KEY_DESC, dsm.DatasetDate);
        //                //md.Add(Sentry.data.Web.BaseDatasetModel.METADATA_KEY_DATASET_DATE, dsm.DatasetDate);
        //                //md.Add(Sentry.data.Web.BaseDatasetModel.METADATA_KEY_SENTRY_OWNER, dsm.DatasetDate);
        //                //dsm.Metadata = md;
        //                dsm.Metadata.Add("addlTag1", addlTag1);
        //                dsm.Metadata.Add("addlTag2", addlTag2);
        //                _demoDatasetData.Add(dsm);
        //            }
        //        }
        //        return _demoDatasetData;
        //    }
        //}

        public List<BaseDatasetModel> GetDatasetModelList()
        {
            //return DemoDatasetData;
            return dsModelList;
        }

        public BaseDatasetModel GetDatasetModel(int id)
        {
            BaseDatasetModel ds = dsModelList.Where(m => m.DatasetId == id).FirstOrDefault();
            return ds;
        }

        // JCG TODO: Add unit tests for [GET]GetDownloadURL
        [HttpGet()]
        public JsonResult GetDownloadURL(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            JsonResult jr = new JsonResult();
            jr.Data = _s3Service.GetDatasetDownloadURL(ds.S3Key);
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jr;
        }
        // GET: Dataset
        public ActionResult Index()
        {
            List<BaseDatasetModel> dsList = GetDatasetModelList();
            return View(dsList);
        }

        [Route("Dataset/List")]
        public ActionResult List(ListDatasetModel ldm)
        {
            ldm.CategoryList = GetDatasetModelList().Select(x => x.Category).Distinct().ToList();
            ldm.SentryOwnerList = GetSentryOwnerList();
            
                                    
            IList<FilterNameModel> checkedFrequencies = ldm.SearchFilters.Where(f => f.FilterType == "Frequency").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
            IList<FilterNameModel> checkedCategories = ldm.SearchFilters.Where(f => f.FilterType == "Category").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
            IList<FilterNameModel> checkedOwners = ldm.SearchFilters.Where(f => f.FilterType == "Sentry Owner").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();


            List<BaseDatasetModel> freqencyDsList = new List<BaseDatasetModel>();

            foreach (var cf in checkedFrequencies)
            {
                string var = cf.value;
                List<BaseDatasetModel> tempList = new List<BaseDatasetModel>();

                tempList = GetDatasetModelList().Where(x => x.CreationFreqDesc == cf.value).ToList();

                freqencyDsList = freqencyDsList.Union(tempList).ToList();                    
            }


            List<BaseDatasetModel> categoryDsList = new List<BaseDatasetModel>();

            foreach (var cc in checkedCategories)
            {
                string var = cc.value;
                List<BaseDatasetModel> tempList = new List<BaseDatasetModel>();

                tempList = GetDatasetModelList().Where(x => x.Category == cc.value).ToList();

                categoryDsList = categoryDsList.Union(tempList).ToList();               
            }


            List<BaseDatasetModel> ownerDsList = new List<BaseDatasetModel>();

            foreach (var co in checkedOwners)
            {
                string var = co.value;
                List<BaseDatasetModel> tempList = new List<BaseDatasetModel>();

                tempList = GetDatasetModelList().Where(x => x.SentryOwnerName == co.value).ToList();

                ownerDsList = ownerDsList.Union(tempList).ToList();                    
            }
            
            IList<BaseDatasetModel> dsList = Utility.IntersectAllIfEmpty(freqencyDsList, categoryDsList, ownerDsList);

            
            if (dsList.Count == 0 && checkedCategories.Count == 0 && checkedFrequencies.Count == 0 && checkedOwners.Count == 0)
            {
                ////Apply searchtext if not null
                if (ldm.SearchText != null && ldm.SearchText.Trim().Length > 0)
                {
                    dsList = FilterDatasetBySearchPhrase(ldm.SearchText, GetDatasetModelList().ToList());
                }
                else
                {
                    dsList = GetDatasetModelList().ToList();
                }
            }
            else
            {
                if (ldm.SearchText != null && ldm.SearchText.Trim().Length > 0)
                {
                    dsList = FilterDatasetBySearchPhrase(ldm.SearchText, dsList.ToList());
                }
                //    dsList = GetDatasetModelList().ToList();
                //}

                //if (dsList.Count == 0 && checkedCategories.Count == 0 && checkedFrequencies.Count == 0 && checkedOwners.Count == 0)
                //{
            }

            ldm.DatasetList = dsList;
            ldm.SearchFilters = GetDatasetFilters(ldm, null);
            
            return View(ldm);
        }

        private IList<FilterModel> GetDatasetFilters(ListDatasetModel ldm, string cat)
        {
            //get current list of datasets
            List<BaseDatasetModel> baseDsList = GetDatasetModelList().ToList();

            //create FilterModel list object to return
            IList<FilterModel> FilterList = new List<FilterModel>();
            IList<FilterNameModel> FilterNames = new List<FilterNameModel>();

            //Generate Category Filers
            FilterModel Filter = new FilterModel();
            Filter.FilterType = "Category";

            IDictionary<int, string> enumList = EnumToDictionary(typeof(DatasetFrequency));
            IList<FilterNameModel> fList = new List<FilterNameModel>();

            int i = 0;
            
            //foreach (string category in baseDsList.Select(x => x.Category).Distinct().ToList())
            foreach (string category in _datasetContext.Categories.Select(x => x.Name).ToList())
                {
                FilterNameModel nf = new FilterNameModel();
                nf.id = i;
                nf.value = category;

                //Match isChecked status to status on input model
                if (ldm.SearchFilters.Count() > 0)
                {
                    if (ldm.SearchFilters.Where(f => f.FilterType == "Category").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).Count() > 0)
                    {
                        if (ldm.SearchFilters.Where(f => f.FilterType == "Category").SelectMany(fi => fi.FilterNameList).Where(fil => fil.value == category && fil.isChecked == true).Count() > 0)
                        {
                            nf.isChecked = true;
                        }
                    }
                }
                else
                {
                    if (category == cat)
                    {
                        nf.isChecked = true;
                    }
                }

                //Count of all datasets equal to this filter
                nf.count = ldm.DatasetList.Where(f => f.Category == nf.value).Count();

                fList.Add(nf);
                i++;
            }
            Filter.FilterNameList = fList;
            FilterList.Add(Filter);


            //Generate SentryOwner Filers
            Filter = new FilterModel();
            Filter.FilterType = "Sentry Owner";

            fList = new List<FilterNameModel>();

            i = 0;

            //foreach (string category in baseDsList.Select(x => x.Category).Distinct().ToList())
            foreach (string category in _datasetContext.Datasets.Select(x => x.SentryOwnerName).Distinct().ToList())
            {
                FilterNameModel nf = new FilterNameModel();
                nf.id = i;
                nf.value = category;

                //Match isChecked status to status on input model
                if (ldm.SearchFilters.Count() > 0)
                {
                    if (ldm.SearchFilters.Where(f => f.FilterType == "Sentry Owner").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).Count() > 0)
                    {
                        if (ldm.SearchFilters.Where(f => f.FilterType == "Sentry Owner").SelectMany(fi => fi.FilterNameList).Where(fil => fil.value == category && fil.isChecked == true).Count() > 0)
                        {
                            nf.isChecked = true;
                        }
                    }
                }

                //Count of all datasets equal to this filter
                nf.count = ldm.DatasetList.Where(f => f.SentryOwnerName == nf.value).Count();

                fList.Add(nf);
                i++;
            }
            Filter.FilterNameList = fList;
            FilterList.Add(Filter);


            //Generate Frequency Filters
            Filter = new FilterModel();
            Filter.FilterType = "Frequency";

            fList = new List<FilterNameModel>();

            foreach (var item in enumList)
            {
                FilterNameModel nf = new FilterNameModel();
                nf.id = item.Key;
                nf.value = item.Value;

                //Match isChecked status to status on input model
                if (ldm.SearchFilters.Count() > 0)
                {
                    if (ldm.SearchFilters.Where(f => f.FilterType == "Frequency").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).Count() > 0)
                    {
                        if (ldm.SearchFilters.Where(f => f.FilterType == "Frequency").SelectMany(fi => fi.FilterNameList).Where(fil => fil.value == item.Value && fil.isChecked == true).Count() > 0)
                        {
                            nf.isChecked = true;
                        }
                    }
                }

                //Count of all datasets equal to this filter
                nf.count = ldm.DatasetList.Where(f => f.CreationFreqDesc == nf.value).Count();

                fList.Add(nf);
            }
            Filter.FilterNameList = fList;
            FilterList.Add(Filter);

            return FilterList;
        }

        private IDictionary<int, string> EnumToDictionary<T>(){

            return EnumToDictionary(GetType());
        }

        private IDictionary<int, string> EnumToDictionary(Type e)
        {
            if (!(e.IsEnum)) {
                throw new InvalidOperationException("Enum list view model must have Enum generic type constraint");
            }

            Dictionary<int, string> kvp = new Dictionary<int, string>();

            string[] namedValues = System.Enum.GetNames(e);

            foreach (var nv in namedValues)
            {
                int castValue = (int)(Enum.Parse(e, nv));
                var enumVal = System.Enum.Parse(e, nv);
                if (!(kvp.ContainsKey(castValue) && castValue > 0)) {
                    kvp.Add(castValue, enumVal.ToString());
                }
            }

            return kvp;
            
        }

        // JCG TODO: Add unit tests for List()
        // GET: Dataset/List/searchParms
        [Route("Dataset/List/Index")]
        [Route("Dataset/List/SearchPhrase")]
        public ActionResult List(string category, string searchPhrase)
        {
            ListDatasetModel rspModel = new ListDatasetModel();

            // get all unique categories (regardless of earch category)
            //rspModel.CategoryList = GetDatasetModelList().Select(x => x.Category).Distinct().ToList();
            //rspModel.SentryOwnerList = GetSentryOwnerList();

            List<BaseDatasetModel> dsList = null;

            if (category != null && category.Length > 0)
            {   // get list filtered on category
                dsList = GetDatasetModelList().Where(x => x.Category == category).ToList();
            }
            else
            {   // get full list
                dsList = GetDatasetModelList().ToList();
            }

            if (searchPhrase != null && searchPhrase.Trim().Length > 0)
            {
                rspModel.DatasetList = FilterDatasetBySearchPhrase(searchPhrase, dsList).OrderByDescending(x => x.SearchHitList.Count()).ToList();
                //rspModel.DatasetList = rspList.OrderByDescending(x => x.SearchHitList.Count()).ToList();
                rspModel.SearchText = searchPhrase;
            }
            else
            {
                rspModel.DatasetList = dsList;
            }

            rspModel.SearchFilters = GetDatasetFilters(rspModel, category);
            //(rspModel.SearchFilters.SelectMany(x => x.FilterNameList).Where(i => i.value == category).Select(c => c.isChecked)) = true;

            return View(rspModel);
        }

        private List<BaseDatasetModel> FilterDatasetBySearchPhrase(string searchPhrase, List<BaseDatasetModel> dsList)
        {

            IList<string> searchWords = searchPhrase.Trim().ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            List<BaseDatasetModel> rspList =
                dsList.Where(x =>
                    ((x.Category.ToLower() + " " +
                      x.DatasetDesc.ToLower() + " " +
                      x.DatasetName.ToLower() + " " +
                      x.SentryOwnerName.ToLower() + " " +
                      x.CreationFreqDesc.ToLower() + " ") +
                      ((x.Columns != null && x.Columns.Count > 0) ?
                          x.Columns.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " ") +
                      ((x.Metadata != null && x.Metadata.Count > 0) ?
                          x.Metadata.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " "))
                    .Split(new Char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Any(xi => searchWords.Where(s => xi.Contains(s)).Count() > 0)
                ).ToList();

            // DEBUGGING: use "dsList" to look at every dataset
            // foreach (BaseDatasetModel ds in dsList)
            foreach (BaseDatasetModel ds in rspList)
            {
                // DEBUGGING: uncomment this to look at the list of match terms generated for this dataset... 
                //List<string> dsHitList = (
                //    ds.Category.ToLower() + " " +
                //    ds.DatasetDesc.ToLower() + " " +
                //    ds.DatasetName.ToLower() + " " +
                //    ds.SentryOwnerName.ToLower() + " " +
                //    ((ds.Columns != null && ds.Columns.Count > 0) ?
                //        ds.Columns.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " ") +
                //    ((ds.Metadata != null && ds.Metadata.Count > 0) ?
                //        ds.Metadata.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " "))
                //    .Split(new Char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
                //    .ToList();

                ds.SearchHitList = (
                    ds.Category.ToLower() + " " +
                    ds.DatasetDesc.ToLower() + " " +
                    ds.DatasetName.ToLower() + " " +
                    ds.SentryOwnerName.ToLower() + " " +
                    ds.CreationFreqDesc.ToLower() + " " +
                    ds.Columns != null && ds.Columns.Count > 0 ?
                        ds.Columns.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " " +
                    ds.Metadata != null && ds.Metadata.Count > 0 ?
                        ds.Metadata.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " ")
                    .Split(new Char [] {' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x.Any(xi => searchWords.Where(s => x.Contains(s)).Count() > 0))
                    .ToList();
            }

            return rspList;
        }

        private IList<string> GetSentryOwnerList()
        {
            IList<string> var = _datasetContext.GetSentryOwnerList().ToList();
            return var;
        }

        // GET: Dataset/Detail/key
        //public ActionResult Detail(string UniqueKey)
        //{
        //    BaseDatasetModel ds = GetDatasetModel(UniqueKey);
        //    return View(ds);
        //}

        // GET: Dataset/Detail/5
        [HttpGet]
        public ActionResult Detail(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            // IList<String> catList = _datasetContext.GetCategoryList();
            BaseDatasetModel bdm = new BaseDatasetModel(ds);
            bdm.CanDwnldSenstive = SharedContext.CurrentUser.CanDwnldSenstive;
            bdm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            return View(bdm);
        }

        // JCG TODO: Add additional permissions check around CanUpload
        // GET: Dataset/Upload
        [HttpGet]
        public ActionResult Upload()
        {
            UploadDatasetModel udm = new UploadDatasetModel();
            udm.AllCategories = GetCategoryList();
            udm.AllFrequencies = GetDatasetFrequencyListItems();  //load all values for dropdown
            udm.AllOriginationCodes = GetDatasetOriginationListItems(); //load all values for dropdown
            udm.FreqencyID = 6; // preselected NonSchedule
            return View(udm);
        }

        // JCG TODO: Add unit tests for [Post]Upload
        // JCG TODO: Add additional permissions check around CanUpload
        // JCG TODO: Revist moving S3.Transfer.TransferUtility logic to Infrastructure\Core layer
        // POST: Dataset/Upload
        [HttpPost]
        public ActionResult Upload(UploadDatasetModel udm, HttpPostedFileBase DatasetFile)
        {
            try
            {
                
                if (_datasetContext.Datasets.Any(m => m.DatasetName == udm.DatasetName))
                {
                    throw new ValidationException("Dataset name already exists");
                }

                if (DatasetFile == null)
                {
                    throw new ValidationException("Please select file to be uploaded");
                }

                string category = _datasetContext.GetReferenceById<Category>(udm.CategoryIDs).Name;
                string frequency = ((DatasetFrequency)udm.FreqencyID).ToString();
                string originationcode = ((DatasetOriginationCode)udm.OriginationID).ToString();
                string dsfi = System.IO.Path.GetFileName(DatasetFile.FileName);
                string s3key = category + "/" + dsfi;

                if (_datasetContext.s3KeyDuplicate(s3key))
                {
                    throw new ValidationException("File already exsits on S3");
                }
                if (_datasetContext.Datasets.Any(m => m.DatasetName == udm.DatasetName))
                {
                    throw new ValidationException("Dataset name already exsists");
                }

                Sentry.Common.Logging.Logger.Debug("Entered HttpPost <Upload>");
                if (ModelState.IsValid)
                {
                    Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: Started S3 TransferUtility Setup");

                    _s3Service.OnTransferProgressEvent += new EventHandler<TransferProgressEventArgs>(uploadRequest_UploadPartProgressEvent);
                    _s3Service.TransferUtlityUploadStream(category, dsfi, DatasetFile.InputStream);


                    //// 1. upload dataset
                    //Amazon.S3.Transfer.TransferUtility s3tu = new Amazon.S3.Transfer.TransferUtility(S3Client);
                    ////Amazon.S3.Transfer.TransferUtility s3tu = new Amazon.S3.Transfer.TransferUtility(_s3Service.clie);
                    //Amazon.S3.Transfer.TransferUtilityUploadRequest s3tuReq = new Amazon.S3.Transfer.TransferUtilityUploadRequest();
                    ////Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - Set AWS BucketName: " + Configuration.Config.GetSetting("AWSRootBucket"));
                    //s3tuReq.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
                    ////Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - InputStream");
                    //s3tuReq.InputStream = DatasetFile.InputStream;
                    ////Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - Set S3Key: " + category + "/" + dsfi);
                    //s3tuReq.Key = category + "/" + dsfi;
                    //s3tuReq.UploadProgressEvent += new EventHandler<Amazon.S3.Transfer.UploadProgressArgs>(uploadRequest_UploadPartProgressEvent);
                    //s3tuReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                    //s3tuReq.AutoCloseStream = true;
                    ////Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: Starting Upload " + s3tuReq.Key);
                    //s3tu.Upload(s3tuReq);


                    // 2. create dataset metadata
                    List<DatasetMetadata> dsmd = new List<DatasetMetadata>();
                    DateTime dateTimeNow = DateTime.Now;
                    Dataset ds = new Dataset(
                        0, // adding new dataset; ID is disregarded
                        category,
                        udm.DatasetName,
                        udm.DatasetDesc,
                        udm.CreationUserName,
                        udm.SentryOwnerName,
                        _userService.GetCurrentUser().AssociateId,
                        originationcode,
                        udm.DatasetDtm,
                        dateTimeNow,
                        dateTimeNow,
                        frequency,
                        DatasetFile.ContentLength,
                        udm.RecordCount,
                        category + "/" + dsfi,
                        udm.IsSensitive,
                        null);

                    //foreach (_DatasetMetadataModel dsmdmi in udm.RawMetadata)
                    //{
                    //    DatasetMetadata dsmdi = new DatasetMetadata(dsmdmi.Id, dsmdmi.DatasetId, dsmdmi.IsColumn, dsmdmi.Name, dsmdmi.Value, ds);
                    //    ds.RawMetadata.Add(dsmdi);
                    //}

                    _datasetContext.Merge<Dataset>(ds);
                    _datasetContext.SaveChanges();
                    int maxId = _datasetContext.GetMaxId();
                    return RedirectToAction("Detail", new { id = maxId });
                }

            }
            catch (Exception ex)
            {
                if (ex is ValidationException)
                {
                    AddCoreValidationExceptionsToModel(ex as ValidationException);
                    Sentry.Common.Logging.Logger.Error("Error", ex);
                    
                }
                else
                {
                    if (ex is AmazonS3Exception)
                    {
                        Sentry.Common.Logging.Logger.Error("S3 Upload Error", ex);
                        ModelState.AddModelError("Upload", ex);
                    }

                    Sentry.Common.Logging.Logger.Error("Error", ex);
                }
                
            }
            finally
            {
                _datasetContext.Clear();
                udm.AllCategories = GetCategoryList();  //Reload dropdown value list
                udm.AllFrequencies = GetDatasetFrequencyListItems();  //Reload dropdown value list
                udm.AllOriginationCodes = GetDatasetOriginationListItems(); //Reload dropdown value list
                udm.FreqencyID = 6; // preselected NonSchedule
                udm.OriginationID = 1; // preselected Internal
                
            }
            return View(udm);

        }

        /// <summary>
        /// Callback handler for S3 uploads... Amazon calls this to communicate progress; from here we communicate
        /// that progress back to the client for their progress bar...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void uploadRequest_UploadPartProgressEvent(object sender, TransferProgressEventArgs e)
        {
            Sentry.data.Web.Helpers.ProgressUpdater.SendProgress(e.FilePath, e.PercentDone);

            if(e.PercentDone % 10 == 0)
            {
                Sentry.Common.Logging.Logger.Debug("DatasetUpload-S3Event: " + e.FilePath + ": " + e.PercentDone);
            }
            
        }

        //// GET: Dataset/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        // POST: Dataset/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: Dataset/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //// POST: Dataset/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Dataset/ViewVersions/5
        //[HttpGet()]
        //public JsonResult GetLatestVersion(string uniqueKey)
        //{   // the uniqueKey here is the key for the logical file (folder) containing physical files (versions)...
        //    DatasetFolder thisFolder = _dataSetService.GetFolderByUniqueKey(uniqueKey);
        //    string latestDatasetUniqueKey = thisFolder.Datasets.OrderByDescending(ds => ds.Name).FirstOrDefault().Key;
        //    JsonResult jr = new JsonResult();
        //    jr.Data = _dataSetService.GetDatasetDownloadURL(latestDatasetUniqueKey);
        //    jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    return jr;
        //}

        //// GET: DatasetFileVersion/Create
        //[HttpGet()]
        //public ActionResult Create()
        //{
        //    EditDatasetModel item = new EditDatasetModel { LastSchemaChangeDate = DateTime.Now, LastDataRefreshDate = DateTime.Now };
        //    item.AllCategories = GetCategoryList();
        //    return View(item);
        //}

        //// POST: DatasetFileVersion/Create
        //[HttpPost()]
        //public ActionResult Create(EditDatasetModel i)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            DomainUser seller = SharedContext.CurrentUser.DomainUser;
        //            DatasetFileVersion item = CreateItemFromModel(i);
        //            _dataAssetContext.Add(item);
        //            _dataAssetContext.SaveChanges();
        //            return RedirectToAction("Details", new {id = item.Id});
        //        }
        //    }
        //    catch (Sentry.Core.ValidationException ex)
        //    {
        //        AddCoreValidationExceptionsToModel(ex);
        //        _dataAssetContext.Clear();
        //    }

        //    i.AllCategories = GetCategoryList(); //re-populate the category list for re-display
        //    return View(i);
        //}

        // GET: DatasetFileVersion/Edit/5
        // JCG TODO: Add additional permissions check around CanEditDataset
        [HttpGet()]
        public ActionResult Edit(int id)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(id);
            EditDatasetModel item = new EditDatasetModel(ds);
            item.AllFrequencies = GetDatasetFrequencyListItems();  // Load dropdown value list
            item.FreqencyID = (int)(Enum.Parse(typeof(DatasetFrequency), ds.CreationFreqDesc));  //Preselect current value
            item.AllOriginationCodes = GetDatasetOriginationListItems();
            item.OriginationID = (int)(Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode));  //Preselect current value
            return View(item);
        }

        // JCG TODO: Add unit tests for [Post]Edit
        // JCG TODO: Add additional permissions check around CanEditDataset
        // POST: Dataset/Edit/5
        [HttpPost()]
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
            }

            return View(i);
        }

        private IEnumerable<SelectListItem> GetCategoryList()
        {
            IEnumerable<SelectListItem> var = _datasetContext.Categories.OrderByHierarchy().Select((c) => new SelectListItem { Text = c.FullName, Value = c.Id.ToString() });

            return var;
        }

        private IEnumerable<SelectListItem> GetDatasetFrequencyListItems()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(DatasetFrequency)).Cast<DatasetFrequency>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
        }

        private IEnumerable<SelectListItem> GetDatasetOriginationListItems()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(DatasetOriginationCode)).Cast<DatasetOriginationCode>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
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
                    case Dataset.ValidationErrors.uploadDateIsOld:
                        ModelState.AddModelError("UploadDate", vr.Description);
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

        //private DatasetFileVersion CreateItemFromModel(EditDatasetModel itemModel)
        //{
        //    DomainUser seller = SharedContext.CurrentUser.DomainUser;
        //    DatasetFileVersion i = new DatasetFileVersion(itemModel.Name, itemModel.SummaryDescription, itemModel.DetailDescription);
        //    i.ImageUrl = itemModel.ImageUrl;
        //    foreach (int c in itemModel.CategoryIDs)
        //    {
        //        i.AddCategory(_dataAssetContext.GetReferenceById<Category>((int)c));
        //    }
        //    return i;
        //}

        // JCG TODO: Add unit tests for AddDatasetFromModel()
        private void AddDatasetFromModel(Dataset ds, EditDatasetModel eds)
        {
            DateTime now = DateTime.Now;

            //ds.Id = eds.Id;
            ds.Category = eds.Category;
            ds.DatasetName = eds.DatasetName;
            ds.DatasetDesc = eds.DatasetDesc;
            //string creationUserId;
            ds.CreationUserName = eds.CreationUserName;
            //string sentryOwnerId;
            ds.SentryOwnerName = eds.SentryOwnerName;
            ds.UploadUserName = eds.UploadUserName;
            ds.OriginationCode = ds.OriginationCode;
            //string fileExtension;
            ds.DatasetDtm = eds.DatasetDtm;
            ds.ChangedDtm = now;
            //string creationFreqCode;
            ds.CreationFreqDesc = eds.CreationFreqDesc;
            ds.FileSize = eds.FileSize;
            ds.RecordCount = eds.RecordCount;
            //ds.S3key;
            IList<_DatasetMetadataModel> dsmList = new List<_DatasetMetadataModel>();
            if (null != eds.Columns && null != eds.Metadata)
            {
                dsmList = eds.Columns.Union(eds.Metadata).ToList();
            }
            else if (null != eds.Metadata)
            {
                dsmList = eds.Metadata;
            }
            else if (null != eds.Columns)
            {
                dsmList = eds.Columns;
            }
            foreach (_DatasetMetadataModel dsm in dsmList)
            {
                ds.RawMetadata.Add(new DatasetMetadata(dsm.DatasetMetadataId, dsm.DatasetId, dsm.IsColumn, dsm.Name, dsm.Value, ds));
            }
        }

        // JCG TODO: Add unit tests for UpdateDatasetFromModel()
        /// <summary>
        /// Allowed updates:
        ///  - Sentry Owner
        ///  - Creation Frequency
        ///  - ChangedDtm
        ///  - User Metadata
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="eds"></param>
        private void UpdateDatasetFromModel(Dataset ds, EditDatasetModel eds)
        {
            DateTime now = DateTime.Now;

            string frequency = ((DatasetFrequency)eds.FreqencyID).ToString();
            string originationcode = ((DatasetOriginationCode)eds.OriginationID).ToString();

            ds.ChangedDtm = now;
            if (null != eds.Category && eds.Category.Length > 0) ds.Category = eds.Category;
            ds.CreationFreqDesc = frequency;
            if (null != eds.CreationUserName && eds.CreationUserName.Length > 0) ds.CreationUserName = eds.CreationUserName;
            if (null != eds.DatasetDesc && eds.DatasetDesc.Length > 0) ds.DatasetDesc = eds.DatasetDesc;
            if (null != eds.DatasetDtm && eds.DatasetDtm > DateTime.MinValue) ds.DatasetDtm = eds.DatasetDtm;
            if (null != eds.DatasetName && eds.DatasetName.Length > 0) ds.DatasetName = eds.DatasetName;
            ds.OriginationCode = originationcode;
            if (null != eds.SentryOwnerName && eds.SentryOwnerName.Length > 0) ds.SentryOwnerName = eds.SentryOwnerName;
            if (eds.RecordCount > 0) ds.RecordCount = eds.RecordCount;

            //if (eds.md_Metadata != null)
            //{
            //    IList<DatasetMetadata> newRawData = new List<DatasetMetadata>(ds.Columns);
            //    foreach (_DatasetMetadataModel dsmm in eds.md_Metadata)
            //    {
            //        DatasetMetadata dsm = new DatasetMetadata(dsmm.Id, dsmm.DatasetId, dsmm.IsColumn, dsmm.Name, dsmm.Value, ds);
            //        newRawData.Add(dsm);
            //    }
            //    ds.RawMetadata = newRawData;
            //}
        }

        [HttpPost]
        public ActionResult PushToSAS(PushToDatasetModel PushToModel)
        {
            Dataset ds = _datasetContext.GetById(PushToModel.DatasetId);
            string filename = null;
            string filename_orig = null;

            Sentry.Common.Logging.Logger.Debug("<PushToSAS>: DatasetId: " + PushToModel.DatasetId);
            Sentry.Common.Logging.Logger.Debug("File Name Override Value: " + PushToModel.FileNameOverride);

            //Test for an override name; if empty or null, use current value on dataset model
            if (!String.IsNullOrWhiteSpace(PushToModel.FileNameOverride))
            {
                //Test if override name includes an extension; if exists, replace with current value in dataset model
                if (Path.HasExtension(PushToModel.FileNameOverride))
                {
                    Sentry.Common.Logging.Logger.Debug("Has File Extension: " + System.IO.Path.GetExtension(PushToModel.FileNameOverride));
                    Sentry.Common.Logging.Logger.Debug("Dataset Model Extension: " + ds.FileExtension);
                    filename = PushToModel.FileNameOverride.Replace(System.IO.Path.GetExtension(PushToModel.FileNameOverride), ds.FileExtension);
                }
                else
                {
                    Sentry.Common.Logging.Logger.Debug("Has No File Extension");
                    Sentry.Common.Logging.Logger.Debug("Dataset Model Extension: " + ds.FileExtension);
                    filename = (PushToModel.FileNameOverride + ds.FileExtension);
                }
            }
            else
            {
                Sentry.Common.Logging.Logger.Debug(" No Override Value");
                Sentry.Common.Logging.Logger.Debug("Dataset Model S3Key: " + System.IO.Path.GetFileName(ds.S3Key));
                filename = System.IO.Path.GetFileName(ds.S3Key);
            }

            filename_orig = filename;

            //Gerenate SAS friendly file name.
            filename = _sasService.GenerateSASFileName(filename);

            Sentry.Common.Logging.Logger.Debug($"File Name Translation: Original({filename_orig} SASFriendly({filename})");

            string BaseTargetPath = Configuration.Config.GetHostSetting("PushToSASTargetPath");

            //creates category directory if does not exist, otherwise does nothing.
            System.IO.Directory.CreateDirectory(BaseTargetPath + ds.Category);


            try
            {
                _s3Service.OnTransferProgressEvent += new EventHandler<TransferProgressEventArgs>(uploadRequest_UploadPartProgressEvent);
                _s3Service.TransferUtilityDownload(BaseTargetPath, ds.Category, filename, ds.S3Key);

            }
            catch (Exception e)
            {
                Sentry.Common.Logging.Logger.Error("S3 Download Error", e);
                //return Json(new {Success = false});
            }


            //string content = string.Empty;
            //Converting file to .sas7bdat format
            try
            {

                _sasService.ConvertToSASFormat(filename, ds.Category);

            }
            catch (WebException we)
            {
                Sentry.Common.Logging.Logger.Error("Web Error Calling SAS Stored Process", we);
            }
            catch (Exception e)
            {
                Sentry.Common.Logging.Logger.Error("Error calling SAS Stored Process", e);
            }


            //Uri uri = new Uri(@"https://executionsasmidtierqual.sentry.com/SASStoredProcess/do?_program=%2FUser+Folders%2FJered+Gosse%2FMy+Folder%2FSTP_PushToSAS_CSV_Final" + "&FILE_NAME=" + "2015.annual.singlefile.csv" + "&CATEGORY=" + "Government" + "&_username=RA072984" + "&_password={SAS002}CFEE423D534550431C426CC70746FBFA0EEE11BE07E73FA1");
            //WebRequest webRequest = WebRequest.Create(uri);
            ////webRequest.Proxy = new IWebProxy("webproxy.sentry.com", 80);
            //webRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            //WebResponse webResponse = webRequest.GetResponse();
            //var stream = webResponse.GetResponseStream();

            //using (var reader = new StreamReader(stream ?? new MemoryStream(), Encoding.UTF8))
            //    content = reader.ReadToEnd();

            //Sentry.Common.Logging.Logger.Debug(content);

            //string url =  @"https://executionsasmidtierqual.sentry.com/SASStoredProcess/do?_program=%2FUser+Folders%2FJered+Gosse%2FMy+Folder%2FSTP_PushToSAS_CSV_Final" + "&FILE_NAME=" + "2015.annual.singlefile.csv" + "&CATEGORY=" + "Government";

            //WebResponse response = SendGetRequest(url);

            //throw new Exception("Error");

            //return new HttpStatusCodeResult(401, "Custom Error Message");

            return AjaxSuccessJson();

        }

        //public static WebResponse SendGetRequest(string url)
        //{

        //    HttpWebRequest httpRequest = WebRequest.CreateHttp(url);
        //    httpRequest.Method = "GET";

        //    return httpRequest.GetResponse();
        //}
        
        /// <summary>
        /// Callback handler for S3 uploads... Amazon calls this to communicate progress; from here we communicate
        /// that progress back to the client for their progress bar...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //static void downloadRequest_DownloadPartProgressEvent(object sender, WriteObjectProgressArgs e)
        //{
            
        //    Sentry.data.Web.Helpers.ProgressUpdater.SendProgress(e.FilePath, e.PercentDone);
        //    //Sentry.data.Web.Helpers.ProgressUpdater.SendProgress(e., e.TotalNumberOfBytesForCurrentFile, e.TransferredBytesForCurrentFile);
        //    Sentry.Common.Logging.Logger.Debug("DatasetDownload-S3Event: " + e.FilePath + ": " + e.PercentDone);
        //}

        [HttpGet()]
        public PartialViewResult PushToFileNameOverride(int id)
        {
            PushToDatasetModel model = new PushToDatasetModel();

            
            Dataset dataset = _datasetContext.GetById<Dataset>(id);
            model.DatasetId = dataset.DatasetId;
            model.DatasetFileName = System.IO.Path.GetFileName(dataset.S3Key);

            return PartialView("_PushToFilenameOverride", model);

        }


        //[HttpGet()]
        //public void GetWeatherData(string zip)
        //{
        //    //System.IO.File.WriteAllText(@"C:\Temp\WeatherUndergroundData\" + zip + ".xml", _weatherDataProvider.GetWeather("xml"));
        //    //System.IO.File.WriteAllText(@"C:\Temp\WeatherUndergroundData\" + zip + ".json", _weatherDataProvider.GetWeather("json"));


        //    request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
        //    request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

        //    // easily add HTTP Headers
        //    //request.AddHeader("header", "value");

        //    // add files to upload (works with compatible verbs)
        //    //request.AddFile(path);

        //    // execute the request         

        //    // or automatically deserialize result
        //    // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
        //    RestResponse<Person> response2 = client.Execute<Person>(request);
        //    var name = response2.Data.Name;

        //    // easy async support
        //    client.ExecuteAsync(request, response =>
        //    {
        //        Console.WriteLine(response.Content);
        //    });

        //    // async with deserialization
        //    var asyncHandle = client.ExecuteAsync<Person>(request, response =>
        //    {
        //        Console.WriteLine(response.Data.Name);
        //    });

        //    // abort the request on demand
        //    asyncHandle.Abort();
        //}


        //[HttpGet()]
        //public void GetWeatherData(string zip)
        //{
        //    System.IO.File.WriteAllText(@"C:\Temp\WeatherUndergroundData\" + zip + ".xml", _weatherDataProvider.GetWeather("xml"));
        //    System.IO.File.WriteAllText(@"C:\Temp\WeatherUndergroundData\" + zip + ".json", _weatherDataProvider.GetWeather("json"));


        //    //request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
        //    //request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

        //    //// easily add HTTP Headers
        //    ////request.AddHeader("header", "value");

        //    //// add files to upload (works with compatible verbs)
        //    ////request.AddFile(path);

        //    //// execute the request         

        //    //// or automatically deserialize result
        //    //// return content type is sniffed but can be explicitly set via RestClient.AddHandler();
        //    //RestResponse<Person> response2 = client.Execute<Person>(request);
        //    //var name = response2.Data.Name;

        //    //// easy async support
        //    //client.ExecuteAsync(request, response => {
        //    //    Console.WriteLine(response.Content);
        //    //});

        //    //// async with deserialization
        //    //var asyncHandle = client.ExecuteAsync<Person>(request, response => {
        //    //    Console.WriteLine(response.Data.Name);
        //    //});

        //    //// abort the request on demand
        //    //asyncHandle.Abort();
        //}


        //[HttpGet()]
        //public void GetWeather(string zip)
        //{

        //    JsonResult jr = new JsonResult();
        //    jr.Data = GetWeatherByZip(zip);
        //    string json = JsonConvert.SerializeObject(jr.Data);


        //    System.IO.File.WriteAllText(@"C:\Temp\WeatherUndergroundData\54481.txt", json);
        //    //return jr;
        //}

        //public async Task<ActionResult> GetWeatherByZip(string zip)
        //{
        //    string relativePath = zip + "/" + zip + ".json";
        //    return await this.ApiGet(relativePath);
        //}

        //protected async Task<ActionResult> ApiGet(string relativePath)
        //{
        //    ApiResponse response = await this.ApiClient.GetAsync(relativePath);
        //    ActionResult result = response.IsSuccessful ? new ContentResult { Content = response.Data, ContentType = "application/json" } as ActionResult : new JsonResult();
        //    return result;
        //}

        //protected IApiClient ApiClient
        //{
        //    get
        //    {
        //        return this._apiClient;
        //    }
        //}
        
        //public JsonResult AjaxSuccessJson()
        //{
        //    return Json(new { Success = true });
        //}
    }
}
