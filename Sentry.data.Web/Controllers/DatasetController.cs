using Hangfire;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.Schema.Elastic;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Helpers;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_VIEW)]
    public class DatasetController : BaseController
    {
        public readonly IAssociateInfoProvider _associateInfoProvider;
        public readonly IDatasetContext _datasetContext;
        private readonly UserService _userService;
        private readonly S3ServiceProvider _s3Service;
        private readonly ISASService _sasService;
        private readonly IObsidianService _obsidianService;
        private readonly IDatasetService _datasetService;
        private readonly IEventService _eventService;
        private readonly IConfigService _configService;
        private readonly IDataFeatures _featureFlags;
        private readonly ISAIDService _saidService;
        private readonly IJobService _jobService;
        private readonly NamedEnvironmentBuilder _namedEnvironmentBuilder;
        private readonly IElasticContext _elasticContext;
        private readonly Lazy<IDataApplicationService> _dataApplicationService;

        public DatasetController(
            IDatasetContext dsCtxt,
            S3ServiceProvider dsSvc,
            UserService userService,
            ISASService sasService,
            IAssociateInfoProvider associateInfoService,
            IObsidianService obsidianService,
            IDatasetService datasetService,
            IEventService eventService,
            IConfigService configService,
            IDataFeatures featureFlags,
            ISAIDService saidService,
            IJobService jobService,
            NamedEnvironmentBuilder namedEnvironmentBuilder,
            IElasticContext elasticContext,
            Lazy<IDataApplicationService> dataApplicationService)
        {
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            _associateInfoProvider = associateInfoService;
            _obsidianService = obsidianService;
            _datasetService = datasetService;
            _eventService = eventService;
            _configService = configService;
            _featureFlags = featureFlags;
            _saidService = saidService;
            _jobService = jobService;
            _namedEnvironmentBuilder = namedEnvironmentBuilder;
            _elasticContext = elasticContext;
            _dataApplicationService = dataApplicationService;
        }

        private IDataApplicationService DataApplicationService
        {
            get { return _dataApplicationService.Value; }
        }

        public ActionResult Index()
        {
            HomeModel hm = new HomeModel
            {
                DatasetCount = _datasetContext.Datasets.Where(w => w.DatasetType == GlobalConstants.DataEntityCodes.DATASET && w.CanDisplay).Count(),
                Categories = _datasetContext.Categories.Where(w => w.ObjectType == GlobalConstants.DataEntityCodes.DATASET).ToList(),
                CanEditDataset = SharedContext.CurrentUser.CanModifyDataset,
                DisplayDataflowMetadata = _featureFlags.Expose_Dataflow_Metadata_CLA_2146.GetValue()
            };

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Dataset Home Page", 0);
            return View(hm);
        }


        #region Dataset Modification

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public async Task<ActionResult> Create()
        {
            DatasetModel cdm = new DatasetModel()
            {
                //these are defaluted for now and disbled on UI but will change to open field.
                ConfigFileName = "Default",
                ConfigFileDesc = "Default Config for Dataset.  Uploaded files that do not match any configs will default to this config",
                UploadUserId = SharedContext.CurrentUser.AssociateId,
            };

            Utility.SetupLists(_datasetContext, cdm);
            cdm.SAIDAssetDropDown = await BuildSAIDAssetDropDown(cdm.SAIDAssetKeyCode);

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(cdm.SAIDAssetKeyCode, cdm.NamedEnvironment);
            cdm.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            cdm.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            cdm.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            _ = _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Creation Page", cdm.DatasetId);

            ViewData["Title"] = "Create Dataset";

            return View("DatasetForm", cdm);
        }

        [HttpGet()]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public async Task<ActionResult> Edit(int id)
        {
            // TODO: CLA-2765 - Add filtering to ensure EDIT only occurs for ACTIVE object status
            UserSecurity us = _datasetService.GetUserSecurityForDataset(id);
            if (us != null && us.CanEditDataset)
            {
                DatasetDto dto = _datasetService.GetDatasetDto(id);
                DatasetModel model = new DatasetModel(dto);
                Utility.SetupLists(_datasetContext, model);
                model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode);

                var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment);
                model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
                model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
                model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

                _ = _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Edit Page", id);
                
                ViewData["Title"] = "Edit Dataset";

                return View("DatasetForm", model);
            }

            return View("Forbidden");
        }

        [HttpDelete]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Dataset/{id}/Delete")]
        public JsonResult Delete(int id)
        {
            try
            {

                UserSecurity us = _datasetService.GetUserSecurityForDataset(id);

                if (us.CanEditDataset)
                {
                    //Issue logical delete
                    DataApplicationService.DeleteDataset(new List<int>() { id }, SharedContext.CurrentUser);
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.DELETE_DATASET, "Deleted Dataset", id);
                    return Json(new { Success = true, Message = "Dataset successfully deleted" });
                }
                return Json(new { Success = false, Message = "You do not have permissions to delete this dataset" });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete dataset - DatasetId:{id} RequestorId:{SharedContext.CurrentUser.AssociateId} RequestorName:{SharedContext.CurrentUser.DisplayName}", ex);
                return Json(new { Success = false, Message = "We failed to delete the dataset.  Please try again later.  Please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a> if problem persists." });
            }
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public async Task<PartialViewResult> _DatasetCreateEdit()
        {
            DatasetModel cdm = new DatasetModel()
            {
                UploadUserId = SharedContext.CurrentUser.AssociateId,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active
            };

            Utility.SetupLists(_datasetContext, cdm);
            cdm.SAIDAssetDropDown = await BuildSAIDAssetDropDown(cdm.SAIDAssetKeyCode);

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(cdm.SAIDAssetKeyCode, cdm.NamedEnvironment);
            cdm.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            cdm.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            cdm.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            _ = _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Creation Page", cdm.DatasetId);
            ViewData["Title"] = "Create Dataset";
            return PartialView("_DatasetCreateEdit", cdm);
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [Route("Dataset/NamedEnvironment")]
        public async Task<PartialViewResult> _NamedEnvironment(string assetKeyCode, string namedEnvironment)
        {
            var model = new DatasetModel()
            {
                SAIDAssetKeyCode = assetKeyCode,
                NamedEnvironment = namedEnvironment
            };

            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(assetKeyCode, namedEnvironment);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);

            return PartialView(model);
        }

        private async Task<List<SelectListItem>> BuildSAIDAssetDropDown(string keyCode)
        {
            List<SelectListItem> output = new List<SelectListItem>();
            List<SAIDAsset> assetList = await _saidService.GetAllAssets().ConfigureAwait(false);

            if (String.IsNullOrWhiteSpace(keyCode) || !assetList.Any(a => a.SaidKeyCode == keyCode))
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
            foreach (SAIDAsset asset in assetList.Where(w => !String.IsNullOrWhiteSpace(w.SaidKeyCode)).OrderBy(o => o.Name))
            {
                output.Add(new SelectListItem
                {
                    Value = asset.SaidKeyCode,
                    Text = $"{asset.Name} ({asset.SaidKeyCode})",
                    Selected = (!String.IsNullOrWhiteSpace(keyCode) && asset.SaidKeyCode == keyCode)
                });
            }

            return output;
        }

        [HttpPost]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public async Task<ActionResult> DatasetForm(DatasetModel model)
        {
            DatasetDto dto = model.ToDto();

            //only validate config settings on dataset create
            if (model.DatasetId == 0)
            {
                FileSchemaDto schemaDto = model.DatasetModelToDto();
                AddCoreValidationExceptionsToModel(_configService.Validate(schemaDto));
            }

            AddCoreValidationExceptionsToModel(await _datasetService.Validate(dto));

            if (ModelState.IsValid)
            {

                if (dto.DatasetId == 0)
                {
                    int datasetId = _datasetService.CreateAndSaveNewDataset(dto);

                    _ = _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.CREATED_DATASET, dto.DatasetName + " was created.", datasetId);
                    return Json(new { Success = true, dataset_id = datasetId });
                    //return RedirectToAction("Detail", new { id = datasetId });
                }
                else
                {
                    _datasetService.UpdateAndSaveDataset(dto);

                    _ = _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.UPDATED_DATASET, dto.DatasetName + " was created.", dto.DatasetId);
                    return Json(new { Success = true, dataset_id = dto.DatasetId });
                    //return RedirectToAction("Detail", new { id = dto.DatasetId });
                }

            }

            Utility.SetupLists(_datasetContext, model);
            if(model.DatasetCategoryIds != null)
            {
                model.CategoryNames = model.AllCategories.Where(cat => model.DatasetCategoryIds.Contains(int.Parse(cat.Value))).Select(cat => cat.Text).ToList();
            }
            model.SAIDAssetDropDown = await BuildSAIDAssetDropDown(model.SAIDAssetKeyCode);
            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDowns(model.SAIDAssetKeyCode, model.NamedEnvironment);
            model.NamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList;
            model.NamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
            model.NamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);
            
            return PartialView("_DatasetCreateEdit", model);
        }


        [HttpGet]
        [Route("Dataset/Detail/{id}/")]
        public ActionResult Detail(int id)
        {
            DatasetDetailDto dto = _datasetService.GetDatesetDetailDto(id);
            if (dto != null)
            {
                DatasetDetailModel model = new DatasetDetailModel(dto);
                model.DisplayDataflowMetadata = _featureFlags.Expose_Dataflow_Metadata_CLA_2146.GetValue();
                model.DisplayTabSections = _featureFlags.CLA3541_Dataset_Details_Tabs.GetValue();
                model.DisplaySchemaSearch = _featureFlags.CLA3553_SchemaSearch.GetValue();
                model.DisplayDataflowEdit = _featureFlags.CLA1656_DataFlowEdit_ViewEditPage.GetValue();
                model.ShowManagePermissionsLink = _featureFlags.CLA3718_Authorization.GetValue();
                _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Dataset Detail Page", dto.DatasetId);

                return View(model);
            }
            else
            {
                return HttpNotFound("Invalid Dataset Id");
            }
        }

        /// <summary>
        /// Controller to deliver partial views to the tab layout.
        /// </summary>
        /// <param name="id">Dataset ID</param>
        /// <param name="tab">Tab name</param>
        /// <param name="data">DatasetDetailModel from the parent view</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Dataset/DetailTab/{id}/{tab}/")]
        public ActionResult DetailTab(int id, string tab, DatasetDetailModel data)
        {
            switch (tab)
            {
                case ("SchemaColumns"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Schema Column Tab", id);
                    return PartialView("Details/_SchemaColumns", data);
                case ("SchemaAbout"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Schema About Tab", id);
                    return PartialView("Details/_SchemaAbout", data);
                case ("DataPreview"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Data Preview Tab", id);
                    return PartialView("Details/_DataPreview", data);
                case ("DataFiles"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Data Files Tab", id);
                    return PartialView("Details/_DataFiles", data);
                case ("SchemaSearch"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Schema Search Tab", id);
                    return PartialView("Details/_SchemaSearch", data);
                default:
                    return HttpNotFound("Invalid Tab");
            }
        }

        /// <summary>
        /// Controller to log view events of the detail tabs.
        /// </summary>
        /// <param name="id">Dataset ID</param>
        /// <param name="tab">Tab name</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Dataset/DetailTab/{id}/{tab}/LogView/")]
        public void DetailTabEventLogger(int id, string tab)
        {
            switch (tab)
            {
                case ("SchemaColumns"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Schema Column Tab", id);
                    return;
                case ("SchemaAbout"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Schema About Tab", id);
                    return;
                case ("DataPreview"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Data Preview Tab", id);
                    return;
                case ("DataFiles"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Detail Data Files Tab", id);
                    return;
                case ("SchemaSearch"):
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Viewed Dataset Schema Search Tab", id);
                    return;
            }
        }

        [HttpGet]
        public async Task<ActionResult> AccessRequest(int datasetId)
        {
            DatasetAccessRequestModel model;

            if (_featureFlags.CLA3718_Authorization.GetValue())
            {
                model = (await _datasetService.GetAccessRequestAsync(datasetId).ConfigureAwait(false)).ToDatasetModel();
                model.AllAdGroups = _obsidianService.GetAdGroups("").Select(x => new SelectListItem() { Text = x, Value = x }).ToList();
                return PartialView("Permission/RequestAccessCLA3723", model);
            }
            model = (await _datasetService.GetAccessRequestAsync(datasetId).ConfigureAwait(false)).ToDatasetModel();
            model.AllAdGroups = _obsidianService.GetAdGroups("").Select(x => new SelectListItem() { Text = x, Value = x }).ToList();
            return PartialView("DatasetAccessRequest", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAccessRequest(DatasetAccessRequestModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = await _datasetService.RequestAccessToDataset(ar);
            
            if (string.IsNullOrEmpty(ticketId))
            {
                return PartialView("_Success", new SuccessModel("There was an error processing your request.", "", false));
            }
            else
            {
                return PartialView("_Success", new SuccessModel("Dataset access was successfully requested.", "Change Id: " + ticketId, true));
            }
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAccessRequestCLA3723([Bind(Prefix = "RequestAccess")] DatasetAccessRequestModel model)
        {
            model.IsAddingPermission = true;
            AccessRequest ar = model.ToCore();
            string ticketId = await _datasetService.RequestAccessToDataset(ar);

            if (string.IsNullOrEmpty(ticketId))
            {
                return PartialView("_Success", new SuccessModel("There was an error processing your request.", "", false));
            }
            else
            {
                return PartialView("_Success", new SuccessModel("Dataset access was successfully requested.", "Change Id: " + ticketId, true));
            }
        }

        [HttpGet]
        public ActionResult CheckAdGroup(string adGroup)
        {
            return Json(_obsidianService.DoesGroupExist(adGroup), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Dataset FILE Modification

        [HttpPost()]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult EditDatasetFile(int id, DatasetFileGridModel i)
        {
            try
            {
                DatasetFile item = _datasetContext.GetById<DatasetFile>(id);

                Event e = new Event
                {
                    EventType = _datasetContext.EventTypes.Where(w => w.Description == "Edited Data File").FirstOrDefault(),
                    Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                    UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                    Dataset = item.Dataset.DatasetId,
                    DataFile = item.DatasetFileId,
                    DataConfig = item.DatasetFileConfig.ConfigId,
                    Reason = "Edited Dataset File"
                };
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
        private void UpdateDatasetfileFromModel(DatasetFile df, DatasetFileGridModel dfgm)
        {
            DateTime now = DateTime.Now;

            df.Information = dfgm.Information;

            df.ModifiedDTM = now;
        }

        [HttpGet()]
        public PartialViewResult EditDatasetFile(int id)
        {
            DatasetFile df = _datasetContext.GetById<DatasetFile>(id);
            DatasetFileGridModel item = new DatasetFileGridModel(df, _associateInfoProvider, _featureFlags);

            return PartialView("EditDataFile", item);

        }

        #endregion

        #region Detail Page



        [Route("Dataset/Detail/{id}/Configuration")]
        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        public ActionResult DatasetConfiguration(int id)
        {
            DatasetDetailDto dto = _datasetService.GetDatesetDetailDto(id);
            DatasetDetailModel model = new DatasetDetailModel(dto);

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Dataset Configuration Page", dto.DatasetId);

            return View("Configuration", model);
        }

        [Route("Dataset/Detail/{datasetId}/SchemaSearch/{schemaId}")]
        [HttpPost]
        public JsonResult SchemaSearch(int datasetId, int schemaId, string search = null)
        {
            ElasticSchemaSearchProvider elasticSchemaSearch = new ElasticSchemaSearchProvider(_elasticContext, datasetId, schemaId);
            List<ElasticSchemaField> results = elasticSchemaSearch.Search(search);
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Subscribe(int id)
        {
            Dataset ds = _datasetContext.GetById(id);
            SubscriptionModel sm = new SubscriptionModel();

            sm.AllIntervals = _datasetContext.GetAllIntervals().Select((c) => new SelectListItem { Text = c.Description, Value = c.Interval_ID.ToString() });
            sm.CurrentSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, id);
            sm.datasetID = ds.DatasetId;
            sm.SentryOwnerName = _userService.GetCurrentUser().AssociateId;

            foreach (Core.EventType et in _datasetContext.EventTypes.Where(w => w.Display && w.Group == "DATASET"))
            {
                if (!sm.CurrentSubscriptions.Any(x => x.EventType.Type_ID == et.Type_ID))
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

                if (!found)
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

        public JsonResult GetDatasetFileInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            //IEnumerable < DatasetFileGridModel > files = _datasetContext.GetAllDatasetFiles().ToList().

            List<DatasetFileGridModel> files = new List<DatasetFileGridModel>();

            UserSecurity us = _datasetService.GetUserSecurityForConfig(Id);


            //Query the Dataset for the following information:
            foreach (DatasetFile df in _datasetContext.GetDatasetFilesForDatasetFileConfig(Id, x => !x.IsBundled).ToList())
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(df, _associateInfoProvider, _featureFlags)
                {
                    HasDataAccess = us.CanViewData,
                    HasDataFileEdit = us.CanEditDataset,
                    HasFullViewDataset = us.CanViewFullDataset
                };
                files.Add(dfgm);
            }

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);

            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBundledFileInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            //IEnumerable < DatasetFileGridModel > files = _datasetContext.GetAllDatasetFiles().ToList().
            List<DatasetFileGridModel> files = new List<DatasetFileGridModel>();

            UserSecurity us = _datasetService.GetUserSecurityForConfig(Id);

            List<DatasetFile> bundledList = _datasetContext.GetDatasetFilesForDatasetFileConfig(Id, w => w.IsBundled).ToList();

            foreach (DatasetFile df in bundledList)
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(df, _associateInfoProvider, _featureFlags)
                {
                    HasDataAccess = us.CanViewData,
                    HasDataFileEdit = us.CanEditDataset,
                    HasFullViewDataset = us.CanViewFullDataset
                };
                files.Add(dfgm);
            }

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);

            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetVersionsOfDatasetFileForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            DatasetFile df = _datasetContext.DatasetFileStatusActive.Where(x => x.DatasetFileId == Id).Fetch(x => x.DatasetFileConfig).FirstOrDefault();

            List<DatasetFileGridModel> files = new List<DatasetFileGridModel>();

            UserSecurity us = _datasetService.GetUserSecurityForConfig(Id);
            List<DatasetFile> datasetFiles = _datasetContext.DatasetFileStatusActive.Where(x => x.Dataset.DatasetId == df.Dataset.DatasetId &&
                                                                                                                        x.DatasetFileConfig.ConfigId == df.DatasetFileConfig.ConfigId &&
                                                                                                                        x.FileName == df.FileName).
                                                                                                    Fetch(x => x.DatasetFileConfig).ToList();
            foreach (DatasetFile dfversion in datasetFiles)
            {
                DatasetFileGridModel dfgm = new DatasetFileGridModel(dfversion, _associateInfoProvider, _featureFlags)
                {
                    HasDataAccess = us.CanViewData,
                    HasDataFileEdit = us.CanEditDataset,
                    HasFullViewDataset = us.CanViewFullDataset
                };
                files.Add(dfgm);
            }

            DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetAllDatasetFileInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        //{
        //    IEnumerable<DatasetFileGridModel> files = _datasetContext.DatasetFile.Where(x => x.ParentDatasetFileId == null).Fetch(x => x.DatasetFileConfig).
        //        Select((f) => new DatasetFileGridModel(f, _associateInfoProvider, _featureFlags));

        //    DataTablesQueryableAdapter<DatasetFileGridModel> dtqa = new DataTablesQueryableAdapter<DatasetFileGridModel>(files.AsQueryable(), dtRequest);
        //    return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);

        //}

        [HttpGet]
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

        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
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

        #region Manage Permissions

        [Route("Dataset/Detail/{datasetId}/Permissions")]
        [HttpGet]
        public ActionResult ManagePermissions(int datasetId)
        {
            var perms = _datasetService.GetDatasetPermissions(datasetId);
            var model = new ManagePermissionsModel(perms);
            return View("Permission/ManagePermissions", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitInheritanceRequest([Bind(Prefix = "Inheritance")] RequestPermissionInheritanceModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = await _datasetService.RequestAccessToDataset(ar);

            if (string.IsNullOrEmpty(ticketId))
            {
                return PartialView("_Success", new SuccessModel("There was an error processing your request.", "", false));
            }
            else
            {
                return PartialView("_Success", new SuccessModel("Dataset permission inheritance change was successfully requested.", "Change Id: " + ticketId, true));
            }
        }

        [Route("Dataset/Detail/{datasetId}/Permissions/GetLatestInheritanceTicket")]
        [HttpGet]
        public ActionResult GetLatestInheritanceTicket(int datasetId)
        {
            var ticket = _datasetService.GetLatestInheritanceTicket(datasetId);
            if(ticket == null)
            {
                ticket = new SecurityTicket();
            }
            return Json(ticket, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Helpers

        [HttpGet()]
        public JsonResult GetDatasetFileDownloadURL(int id)
        {
            DatasetFile df = _datasetContext.DatasetFileStatusActive.Where(x => x.DatasetFileId == id).Fetch(x => x.DatasetFileConfig).FirstOrDefault();

            UserSecurity us = _datasetService.GetUserSecurityForDataset(df.Dataset.DatasetId);
            if (!us.CanViewFullDataset)
            {
                throw new UnauthorizedAccessException();
            }

            Event e = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Downloaded Data File").FirstOrDefault(),
                DataFile = df.DatasetFileId,
                Dataset = df.Dataset.DatasetId,
                DataConfig = df.DatasetFileConfig.ConfigId,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId
            };

            try
            {
                //Testing if object exists in S3, response is not used.
                string objectKey = (df.FileKey) ?? df.FileLocation;
                _s3Service.GetObjectMetadata(df.FileBucket, objectKey, null);

                JsonResult jr = new JsonResult();
                jr.Data = _s3Service.GetDatasetDownloadUrl(objectKey, df.FileBucket);
                jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

                e.Reason = "Successfully Downloaded Data File";
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                return jr;
            }
            catch (Exception ex)
            {
                e.Reason = "Failed to Download Data File";
                e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                var datasetId = df.Dataset != null ? df.Dataset.DatasetId.ToString() : "HUGE PROBLEM.  NO DATASET FOUND.";

                Logger.Fatal($"S3 Data File Not Found - DatasetID:{datasetId} DatasetFile_ID:{id}", ex);
                return Json(new { message = "Encountered Error Retrieving File.<br />If this problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>" }, JsonRequestBehavior.AllowGet);
            }


        }


        [HttpPost]
        public ActionResult PushToSAS(int id, string fileOverride, string delimiter, int guessingrows)
        {
            Event _event = new Event
            {
                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Pushed Data File to SAS").FirstOrDefault(),
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId
            };

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

                string category = ds.Dataset.DatasetCategories.First().Name;
                //creates category directory if does not exist, otherwise does nothing.
                System.IO.Directory.CreateDirectory(BaseTargetPath + category);

                try
                {

                    _s3Service.TransferUtilityDownload(BaseTargetPath, category, filename, ds.FileLocation, ds.VersionId);

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
        public PartialViewResult PushToFileNameOverride(int id)
        {
            PushToDatasetModel model = new PushToDatasetModel();

            DatasetFile datafile = _datasetContext.GetById<DatasetFile>(id);
            model.DatasetFileId = datafile.DatasetFileId;
            model.DatasetFileName = datafile.FileName;

            return PartialView("_PushToFilenameOverride_new", model);

        }

        [HttpGet()]
        public PartialViewResult GetDatasetFileVersions(int id)
        {
            DatasetFileVersionsModel model = new DatasetFileVersionsModel();

            model.DatasetFileId = id;

            return PartialView("_DatasetFileVersions", model);

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
        public ActionResult UploadDatafile(int id, int configId)
        {//JsonResult
            if (Request.Files.Count > 0 && id != 0)
            {
                DatasetFileConfig dfc = _datasetContext.getDatasetFileConfigs(configId);

                UserSecurity us = _datasetService.GetUserSecurityForConfig(configId);
                if (!us.CanUploadToDataset)
                {
                    throw new UnauthorizedAccessException();
                }

                IApplicationUser user = _userService.GetCurrentUser();

                try
                {
                    if (_datasetService.GetUserSecurityForConfig(configId).CanUploadToDataset)
                    {
                        HttpFileCollectionBase files = Request.Files;

                        HttpPostedFileBase file = files[0];

                        string dsfi;

                        //Getting file name
                        dsfi = System.IO.Path.GetFileName(file.FileName);

                        if (dfc != null)
                        {
                            //Create Dataset Loader request
                            //Find job DFSBasic generic job associated with this config and add ID to request.
                            RetrieverJob dfsBasicJob = null;
                            List<RetrieverJob> jobList = _datasetContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == configId).ToList();
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

                            //Create Upload Success Event
                            Event e = new Event
                            {
                                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault(),
                                Status = _datasetContext.EventStatus.Where(w => w.Description == "Started").FirstOrDefault(),
                                UserWhoStartedEvent = user.AssociateId,
                                Dataset = dfc.ParentDataset.DatasetId,
                                DataConfig = dfc.ConfigId,
                                Reason = $"Successfully sent file to drop location [<b>{System.IO.Path.GetFileName(file.FileName)}</b>] for dataset [<b>{dfc.ParentDataset.DatasetName} - {dfc.Schema.Name}</b>]"
                            };
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

        [HttpGet]
        [Route("Dataset/Upload/{datasetId}/Config/{configId}")]
        public ActionResult Upload(int datasetId, int configId = 0)
        {
            UserSecurity us = _datasetService.GetUserSecurityForDataset(datasetId);
            if (!us.CanUploadToDataset)
            {
                throw new UnauthorizedAccessException();
            }

            CreateDataFileModel cd = new CreateDataFileModel();
            //If a value was passed, load appropriate information
            if (datasetId != 0)
            {
                cd = new CreateDataFileModel(_datasetService.GetDatesetDetailDto(datasetId));
            }

            ViewBag.CurrentConfigId = configId;

            return PartialView("_UploadDataFile", cd);
        }

        #endregion

        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [HttpPost]
        public ActionResult RunRetrieverJob(int id)
        {
            try
            {
                BackgroundJob.Enqueue<RetrieverJobService>(RetrieverJobService => RetrieverJobService.RunRetrieverJob(id, JobCancellationToken.Null, null));
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Enqueing Retriever Job ({id}).", ex);
                return Json(new { Success = false, Message = "Failed to queue job, please try later! Contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a> if problem persists." });
            }

            return Json(new { Success = true, Message = "Job successfully queued." });
        }

        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [HttpPost]
        public ActionResult DisableRetrieverJob(int id)
        {
            try
            {
                _jobService.DisableJob(id);

                return Json(new { Success = true, Message = "Job has been marked as disabled and will be removed from the job scheduler." });
            }
            catch (Exception ex)
            {
                Logger.Error($"Error disabling retriever job ({id}).", ex);
                return Json(new { Success = false, Message = "Failed disabling job.  If problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>." });
            }
        }

        [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATASET_MODIFY)]
        [HttpPost]
        public ActionResult EnableRetrieverJob(int id)
        {
            try
            {
                _jobService.EnableJob(id);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error enabling retriever job ({id}).", ex);
                return Json(new { Success = false, Message = "Failed enabling job.  If problem persists, please contact <a href=\"mailto:DSCSupport@sentry.com\">Site Administration</a>." });
            }

            return Json(new { Success = true, Message = "Job has been marked as enabled and will be added to the job scheduler." });
        }

        public JsonResult LoadDatasetList(int id)
        {
            IEnumerable<Dataset> dfList = Utility.GetDatasetByCategoryId(_datasetContext, id);

            SelectListGroup group = new SelectListGroup() { Name = _datasetContext.GetCategoryById(id).Name };

            IEnumerable<SelectListItem> sList = dfList.Select(m => new SelectListItem()
            {
                Text = m.DatasetName,
                Value = m.DatasetId.ToString(),
                Group = group
            });

            return Json(sList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet()]
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

        public JsonResult GetAllDatasetsForQueryPermission()
        {
            List<SelectListItem> sList = new List<SelectListItem>();

            var groupedDatasets = _datasetService.GetDatasetsForQueryTool().GroupBy(x => x.DatasetCategories.First());

            foreach (var ds in groupedDatasets)
            {
                sList.AddRange(ds.Select(m => new SelectListItem()
                {
                    Text = m.DatasetName,
                    Value = m.DatasetId.ToString(),
                    Group = new SelectListGroup() { Name = ds.Key.Name }
                }));
            }

            return Json(sList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSourceDescription(string DiscrimatorValue)
        {
            var obj = _datasetContext.DataSourceTypes.Where(x => x.DiscrimatorValue == DiscrimatorValue).Select(x => x.Description);

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public ActionResult QueryTool()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            ViewBag.LivyURL = Sentry.Configuration.Config.GetHostSetting("ApacheLivy");
            ViewBag.IsAdmin = user.IsAdmin;

            _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, "Viewed Query Tool Page");

            return View("QueryTool");
        }





        //CONTROLLER ACTION called from JS to return the snowflake query
        [HttpPost]
        public ActionResult DelroyGenerateQuery(List<Sentry.data.Web.Models.ApiModels.Schema.SchemaFieldModel> models, List<string> snowflakeViews, List<Models.ApiModels.Schema.SchemaFieldModel> structTracker)
        {
            bool outerfirst = true;
            bool columnExists = false;

            string alias = DelroyAliasMonster(structTracker);
            string query = GenerateSnow(models, alias, ref outerfirst, ref columnExists, structTracker);
            if (!columnExists)
            {
                query = "*" + Environment.NewLine;
            }
            query = "SELECT " + System.Environment.NewLine + query;
            query += DelroyCreateFrom(snowflakeViews);
            query += DelroyCreateLateralFlatten(structTracker);
            query += "LIMIT 10";

            return Json(new { snowQuery = query });
        }

        //RECURSIVE FUNCTION
        //pass array of Fields and will format a line for each child field it finds
        //IF child field is a NON ARRAY STRUCT, call itself again and pass the STRUCT's children and print out all children and keep going
        private string GenerateSnow(List<Models.ApiModels.Schema.SchemaFieldModel> models, string alias, ref bool first, ref bool columnExists, List<Models.ApiModels.Schema.SchemaFieldModel> structTracker)
        {
            StringBuilder line = new StringBuilder();

            foreach (var field in models)
            {
                if (field.FieldType != "STRUCT")
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        line.Append(",");
                    }
                    line.Append(alias).Append(field.Name).Append(DelroyCastMonster(field) + Environment.NewLine);
                    columnExists = true;
                }
                else if (!field.IsArray)
                {
                    //pass "parentStructs" plus append current field so child nodes can get all parent structs appended
                    //pass "first" as reference to know whether to append a comma or not
                    line.Append(GenerateSnow(field.Fields, alias + field.Name + ":", ref first, ref columnExists, structTracker));
                }
            }
            return line.ToString();
        }


        //CAST Each column to its native datatype
        private string DelroyCastMonster(Models.ApiModels.Schema.SchemaFieldModel field)
        {
            string cast = "::" + field.FieldType;

            if (field.FieldType.ToUpper() == "DECIMAL")
            {
                cast += "(" + field.Precision.ToString() + "," + field.Scale.ToString() + ") ";
            }
            else if (field.FieldType.ToUpper() == "VARCHAR")
            {
                cast += "(" + field.Length.ToString() + ") ";
            }

            return cast;
        }


        //Set initial alias of query:
        //RULE: find the nearest ARRAY STRUCT and make that the initial ALIAS followed by all non ARRAY STRUCTS
        //Then the GenerateSnow() will append all STRUCTS it digs through
        //e.g. gary_flatten.value:austin:lily  gary here is an array struct
        private string DelroyAliasMonster(List<Models.ApiModels.Schema.SchemaFieldModel> structTracker)
        {
            StringBuilder alias = new StringBuilder();

            if (structTracker == null)
            {
                return alias.ToString();
            }
            else
            {
                //get the last struct that is an ARRAY which would be the closest parent ARRAY.  This will be our starting ALIAS
                Models.ApiModels.Schema.SchemaFieldModel closestArray = structTracker.LastOrDefault(w => w.IsArray);

                //if we found an array somewhere in the parent hierarchy, then need to start appending structs after that ONLY
                if (closestArray != null)
                {
                    bool parentFound = false;
                    alias.Append(closestArray.Name + "_flatten.value:");

                    //start at top and work through each struct until you hit the closest parent, then after you start appending all structs
                    foreach (var s in structTracker)
                    {
                        if (parentFound)
                        {
                            alias.Append(s.Name + ":");
                        }
                        else if (s.FieldGuid == closestArray.FieldGuid)
                        {
                            parentFound = true;
                        }
                    }
                }
                else
                {
                    //NO ARRAY EXISTS so assume our alias turns into all parent STRUCTS
                    foreach (var s in structTracker)
                    {
                        alias.Append(s.Name + ":");
                    }
                }
            }
            return alias.ToString();
        }




        //CREATE FROM STATEMENT FOR SNOWFLAKE
        private string DelroyCreateFrom(List<string> snowflakeViews)
        {
            StringBuilder fromStatement = new StringBuilder();
            bool first = true;

            //CREATE FROM
            //there can be multiple views associated with a given schema, just pick first one for now
            foreach (var s in snowflakeViews)
            {
                if (first)
                {
                    fromStatement.Append(" FROM ").Append(s).Append(Environment.NewLine);
                    first = false;
                }
            }

            return fromStatement.ToString();
        }

        //CREATE LATERAL FLATTEN STATEMENT FOR SNOWFLAKE
        private string DelroyCreateLateralFlatten(List<Models.ApiModels.Schema.SchemaFieldModel> structTracker)
        {

            StringBuilder flattenStatement = new StringBuilder();
            bool first = true;
            string parentFlatten = String.Empty;
            string currentFlatten = String.Empty;

            if (structTracker == null)
            {
                return flattenStatement.ToString();
            }
            else
            {
                //CREATE FLATTEN FOR ARRAYS ONLY
                foreach (var s in structTracker.Where(w => w.IsArray))
                {
                    currentFlatten = s.Name + "_flatten";
                    if (first)
                    {
                        flattenStatement.Append(",LATERAL FLATTEN(" + s.Name + ") " + currentFlatten);
                        first = false;
                    }
                    else
                    {
                        flattenStatement.Append(",LATERAL FLATTEN(" + parentFlatten + ".value:" + s.Name + ") " + currentFlatten);
                    }
                    parentFlatten = currentFlatten;
                    flattenStatement.Append(Environment.NewLine);

                }

                return flattenStatement.ToString();
            }
        }

        protected override void AddCoreValidationExceptionsToModel(ValidationException ex)
        {
            foreach (ValidationResult vr in ex.ValidationResults.GetAll())
            {
                switch (vr.Id)
                {
                    //Base Model Errors                    
                    case Dataset.ValidationErrors.datasetNameDuplicate:
                    case GlobalConstants.ValidationErrors.NAME_IS_BLANK:
                        ModelState.AddModelError(nameof(DatasetModel.DatasetName), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetShortNameRequired:
                    case Dataset.ValidationErrors.datasetShortNameInvalid:
                    case Dataset.ValidationErrors.datasetShortNameDuplicate:
                        ModelState.AddModelError(nameof(DatasetModel.ShortName), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetDescriptionRequired:
                        ModelState.AddModelError(nameof(DatasetModel.DatasetDesc), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetCreatedByRequired:
                        ModelState.AddModelError(nameof(DatasetModel.CreationUserId), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetUploadedByRequired:
                        ModelState.AddModelError(nameof(DatasetModel.UploadUserId), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetContactRequired:
                        ModelState.AddModelError(nameof(DatasetModel.PrimaryContactId), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetCategoryRequired:
                        ModelState.AddModelError(nameof(DatasetModel.DatasetCategoryIds), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetScopeRequired:
                        ModelState.AddModelError(nameof(DatasetModel.DatasetScopeTypeId), vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.SAID_ASSET_REQUIRED:
                        ModelState.AddModelError(nameof(DatasetModel.SAIDAssetKeyCode), vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_INVALID:
                        ModelState.AddModelError(nameof(DatasetModel.NamedEnvironment), vr.Description);
                        break;
                    case GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID:
                        ModelState.AddModelError(nameof(DatasetModel.NamedEnvironmentType), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetDateRequired:
                        ModelState.AddModelError(nameof(DatasetModel.DatasetDtm), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetLocationRequired:
                        ModelState.AddModelError(nameof(ReportMetadata.Location), vr.Description);
                        break;
                    case Dataset.ValidationErrors.datasetOriginationRequired:
                        ModelState.AddModelError(nameof(DatasetModel.OriginationID), vr.Description);
                        break;
                    default:
                        ModelState.AddModelError(string.Empty, vr.Description);
                        break;
                }
            }
        }

    }
}