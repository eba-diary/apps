using Sentry.Core;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class BusinessIntelligenceController : BaseController
    {
        public BusinessIntelligenceController(IDatasetContext dsContext, IReportContext rptCtxt, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _reportContext = rptCtxt;
            _associateInfoProvider = associateInfoService;
            _dsContext = dsContext;
            _userService = userService;
        }

        public readonly IAssociateInfoProvider _associateInfoProvider;
        public readonly IReportContext _reportContext;
        public readonly IDatasetContext _dsContext;
        private readonly UserService _userService;

        // GET: Report
        [AuthorizeByPermission(PermissionNames.ReportView)]
        public ActionResult Index()
        {
            BusinessIntelligenceHomeModel rhm = new BusinessIntelligenceHomeModel();

            rhm.DatasetCount = _reportContext.GetReportCount();
            rhm.Categories = _reportContext.Categories.ToList();
            rhm.CanManageReports = SharedContext.CurrentUser.CanManageReports;

            Event e = new Event();
            e.EventType = _reportContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Business Intelligence Home Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(rhm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        public ActionResult Create()
        {
            CreateBusinessIntelligenceModel cdm = new CreateBusinessIntelligenceModel();

            IApplicationUser user = _userService.GetCurrentUser();

            cdm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            cdm.CanUpload = SharedContext.CurrentUser.CanUpload;
            cdm.CreationUserName = user.AssociateId;

            cdm = (CreateBusinessIntelligenceModel)ReportUtility.setupLists(_reportContext, cdm);

            Event e = new Event();
            e.EventType = _reportContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Report Creation Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(cdm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        public ActionResult Create(CreateBusinessIntelligenceModel crm)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if (isDatasetNameDuplicate(crm.DatasetName))
            {
                AddCoreValidationExceptionsToModel(new ValidationException("DatasetName", "Dataset name already exists within category"));
            }

            //Determine schema of incoming file location (i.e. http, file, etc)
            try
            {
                Uri incomingPath = new Uri(crm.Location);
                crm.LocationType = incomingPath.Scheme;
            }
            catch(Exception)
            {
                AddCoreValidationExceptionsToModel(new ValidationException("Location", "Invalid location value"));
            }

            switch (crm.FileTypeId)
            {
                case (int)ReportType.Tableau:
                    if (!Regex.IsMatch(crm.Location.ToLower(), "^https://tableau.sentry.com"))
                    {
                        AddCoreValidationExceptionsToModel(new ValidationException("Location", "Tableau exhibits should begin with https://Tableau.sentry.com"));
                    }
                    break;
                case (int)ReportType.Excel:
                    if (!Regex.IsMatch(crm.Location.ToLower(), "^\\\\\\\\(sentry.com\\\\share\\\\|sentry.com\\\\appfs)"))
                    {
                        AddCoreValidationExceptionsToModel(new ValidationException("Location", "Excel exhibits should begin with \\\\Sentry.com\\Share or \\\\Sentry.com\\appfs"));
                    }
                    break;
                default:
                    break;
            }


            crm.CreationUserName = user.AssociateId;
            crm.DatasetDtm = DateTime.Now;

            try
            {
                if (ModelState.IsValid)
                {
                    Dataset ds = CreateReportFromModel(crm);
                    ds = _reportContext.Merge<Dataset>(ds);
                    _reportContext.SaveChanges();

                    //Create Generic Data File Config for Dataset            
                    DatasetFileConfig dfc = new DatasetFileConfig()
                    {
                        ConfigId = 0,
                        Name = crm.DatasetName,
                        Description = crm.DatasetDesc,
                        FileTypeId = crm.FileTypeId,
                        ParentDataset = ds,
                        DatasetScopeType = _reportContext.DatasetScopeTypes.Where(w => w.Name == "Point-in-Time").FirstOrDefault(),
                        FileExtension = _reportContext.FileExtensions.Where(w => w.Name == "ANY").FirstOrDefault()
                    };

                    _reportContext.Merge(dfc);
                    _reportContext.SaveChanges();

                    Event e = new Event();
                    e.EventType = _reportContext.EventTypes.Where(w => w.Description == "Created Report").FirstOrDefault();
                    e.Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                    e.Dataset = (_reportContext.Datasets.ToList()).FirstOrDefault(x => x.DatasetName == crm.DatasetName).DatasetId;
                    e.Reason = crm.DatasetName + " was created.";
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                    return RedirectToAction("Index");
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _reportContext.Clear();
                crm = (CreateBusinessIntelligenceModel)ReportUtility.setupLists(_reportContext, crm);
            }

            List<SearchableTag> tagsToReturn = new List<SearchableTag>();
            int[] json = new JavaScriptSerializer().Deserialize<int[]>(crm.TagString);
            for (int i = 0; i < json.Length; i++)
            {
                tagsToReturn.Add(_dsContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault().GetSearchableTag());
            }
            crm.TagString = new JavaScriptSerializer().Serialize(tagsToReturn);

            return View(crm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        public ActionResult Edit(int id)
        {
            Dataset ds = _reportContext.GetById<Dataset>(id);

            EditBusinessIntelligenceModel item = new EditBusinessIntelligenceModel(ds, _associateInfoProvider);

            item = (EditBusinessIntelligenceModel)ReportUtility.setupLists(_reportContext, item);

            Event e = new Event();
            e.EventType = _reportContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Report Edit Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


            return View(item);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        //public ActionResult Edit(int id, EditBusinessIntelligenceModel ebim)
        public ActionResult Edit(int id, EditBusinessIntelligenceModel ebim)
        {
            try
            {
                Dataset item = _reportContext.GetById<Dataset>(ebim.DatasetId);

                //Determine schema of incoming file location (i.e. http, file, etc)
                try
                {
                    Uri incomingPath = new Uri(ebim.Location);
                    ebim.LocationType = incomingPath.Scheme;
                }
                catch (Exception)
                {
                    AddCoreValidationExceptionsToModel(new ValidationException("Location", "Invalid location value"));
                }

                switch (ebim.FileTypeId)
                {
                    case (int)ReportType.Tableau:
                        if (!Regex.IsMatch(ebim.Location.ToLower(), "^https://tableau.sentry.com"))
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("Location", "Tableau exhibits should begin with https://Tableau.sentry.com"));
                        }
                        break;
                    case (int)ReportType.Excel:
                        if (!Regex.IsMatch(ebim.Location.ToLower(), "^\\\\\\\\(sentry.com\\\\share\\\\|sentry.com\\\\appfs)"))
                        {
                            AddCoreValidationExceptionsToModel(new ValidationException("Location", "Excel exhibits should begin with \\\\Sentry.com\\Share or \\\\Sentry.com\\appfs"));
                        }
                        break;
                    default:
                        break;
                }

                if (ModelState.IsValid)
                {
                    item = UpdateReportFromModel(item, ebim);
                    _reportContext.SaveChanges();
                    return RedirectToAction("Detail", new { id = ebim.DatasetId });
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
            }
            finally
            {
                _reportContext.Clear();

                ebim = (EditBusinessIntelligenceModel)ReportUtility.setupLists(_reportContext, ebim);
            }



            //List<SearchableTag> tagsToReturn = new List<SearchableTag>();
            //int[] json = new JavaScriptSerializer().Deserialize<int[]>(ebim.TagString);
            //for (int i = 0; i < json.Length; i++)
            //{
            //    tagsToReturn.Add(_reportContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault().GetSearchableTag());
            //}
            ebim.TagString = new JavaScriptSerializer().Serialize(_reportContext.GetById<Dataset>(ebim.DatasetId).Tags.Select(x => x.GetSearchableTag()));

            return View(ebim);
        }

        private Dataset UpdateReportFromModel(Dataset item, EditBusinessIntelligenceModel ebim)
        {

            item.Category = ebim.Category;
            item.DatasetCategory = _reportContext.GetById<Category>(ebim.CategoryIDs);
            item.DatasetDesc = ebim.DatasetDesc;

            int[] json = new JavaScriptSerializer().Deserialize<int[]>(ebim.TagString);

            item.Tags = new List<MetadataTag>();

            for (int i = 0; i < json.Length; i++)
            {
                item.Tags.Add(_reportContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault());
            }

            item.DatasetFileConfigs.First().FileTypeId = ebim.FileTypeId;
            item.Metadata.ReportMetadata.Frequency = ebim.FreqencyID;
            item.Metadata.ReportMetadata.LocationType = ebim.LocationType;
            item.Metadata.ReportMetadata.Location = ebim.Location;

            if (null != ebim.SentryOwnerName && ebim.SentryOwnerName.Length > 0)
            {
                int n;
                if (int.TryParse(ebim.SentryOwnerName, out n))
                {
                    item.SentryOwnerName = ebim.SentryOwnerName;
                }
                else
                {
                    var associate = _associateInfoProvider.GetAssociateInfoByName(ebim.SentryOwnerName);
                    if (associate.FullName == ebim.SentryOwnerName)
                    {
                        item.SentryOwnerName = associate.Id;
                    }
                }
            }

            return item;

        }

        private Dataset CreateReportFromModel(CreateBusinessIntelligenceModel crm)
        {
            DateTime CreateTime = DateTime.Now;
            string cat = _reportContext.GetById<Category>(crm.CategoryIDs).Name;
            IApplicationUser user = _userService.GetCurrentUser();

            Dataset ds = new Dataset()
            {
                DatasetId = 0,
                Category = cat,
                DatasetCategory = _reportContext.GetById<Category>(crm.CategoryIDs),
                DatasetName = crm.DatasetName,
                DatasetDesc = crm.DatasetDesc,
                DatasetInformation = crm.DatasetInformation,
                CreationUserName = user.DisplayName,
                SentryOwnerName = crm.SentryOwnerName,
                UploadUserName = user.AssociateId,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), 1),  //All reports are internal
                DatasetDtm = CreateTime,
                ChangedDtm = CreateTime,
                S3Key = "Blank S3 Key",
                IsSensitive = false,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null,
                DatasetType = "RPT",
                Metadata = new DatasetMetadata()
                {
                    ReportMetadata = new ReportMetadata()
                    {
                        Location = crm.Location,
                        LocationType = crm.LocationType,
                        Frequency = crm.FreqencyID
                    }
                },
                Tags = new List<MetadataTag>()
            };

            int[] json = new JavaScriptSerializer().Deserialize<int[]>(crm.TagString);                       

            ds.Tags = new List<MetadataTag>();

            for (int i = 0; i < json.Length; i++)
            {
                ds.Tags.Add(_dsContext.Tags.Where(x => x.TagId == json[i]).FirstOrDefault());
            }

            return ds;
        }

        private bool isDatasetNameDuplicate(string datasetName)
        {
            return _reportContext.Datasets.ToList().Where(w => w.DatasetName == datasetName).Count() == 0 ? false : true;
        }
    }
}