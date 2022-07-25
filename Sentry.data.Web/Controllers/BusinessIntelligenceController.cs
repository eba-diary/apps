using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_VIEW)]
    public class BusinessIntelligenceController : BaseController
    {

        private readonly IDatasetContext _datasetContext;
        private readonly IBusinessIntelligenceService _businessIntelligenceService;
        private readonly IEventService _eventService;
        private readonly ITagService _tagService;
        private readonly IDataFeatures _featureFlags;

        public BusinessIntelligenceController(
            IDatasetContext datasetContext,
            IBusinessIntelligenceService businessIntelligenceService,
            IEventService eventService,
            ITagService tagService,
            IDataFeatures featureFlags)
        {
            _datasetContext = datasetContext;
            _businessIntelligenceService = businessIntelligenceService;
            _eventService = eventService;
            _tagService = tagService;
            _featureFlags = featureFlags;
        }


        public ActionResult Index()
        {
            BusinessIntelligenceHomeModel rhm = _businessIntelligenceService.GetHomeDto().ToModel();

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Business Intelligence Home Page", 0);
            return View(rhm);
        }


        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult Create()
        {
            BusinessIntelligenceModel cdm = new BusinessIntelligenceModel
            {
                DatasetId = 0,
                BusinessObjectsEnumValue = (int)ReportType.BusinessObjects,
                CreationUserId = SharedContext.CurrentUser.AssociateId,
                UploadUserId = SharedContext.CurrentUser.AssociateId,
                CLA1130_SHOW_ALTERNATE_EMAIL = _featureFlags.CLA1130_SHOW_ALTERNATE_EMAIL.GetValue()          //REMOVE WHEN TURNED ON LATER
            };

            ReportUtility.SetupLists(_datasetContext, cdm);

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Report Creation Page", cdm.DatasetId);

            ViewData["Title"] = "Create Exhibit";

            return View("BusinessIntelligenceForm",cdm);
        }



        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult Edit(int id)
        {
            UserSecurity us = _businessIntelligenceService.GetUserSecurityById(id);

            if (us != null && us.CanEditReport)
            {
                BusinessIntelligenceDto dto = _businessIntelligenceService.GetBusinessIntelligenceDto(id);
                BusinessIntelligenceModel model = new BusinessIntelligenceModel(dto);
                model.CLA1130_SHOW_ALTERNATE_EMAIL = _featureFlags.CLA1130_SHOW_ALTERNATE_EMAIL.GetValue();          //REMOVE WHEN TURNED ON LATER


                ReportUtility.SetupLists(_datasetContext, model);

                _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Report Edit Page", dto.DatasetId);

                ViewData["Title"] = "Edit Exhibit";

                return View("BusinessIntelligenceForm", model);
            }
            return View("Forbidden");
        }



        [HttpPost]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult BusinessIntelligenceForm(BusinessIntelligenceModel crm) 
        {

            crm.CLA1130_SHOW_ALTERNATE_EMAIL = _featureFlags.CLA1130_SHOW_ALTERNATE_EMAIL.GetValue();          //REMOVE WHEN TURNED ON LATER

            AddCoreValidationExceptionsToModel(crm.Validate());

            if (ModelState.IsValid)
            {
                BusinessIntelligenceDto dto = crm.ToDto();
                
                AddCoreValidationExceptionsToModel(_businessIntelligenceService.Validate(dto));
                if (ModelState.IsValid)
                {
                    if (dto.DatasetId == 0)
                    { //CREATE A REPORT
                        bool IsSucessful = _businessIntelligenceService.CreateAndSaveBusinessIntelligence(dto);
                        if (IsSucessful)
                        {
                            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.CREATED_REPORT, crm.DatasetName + " was created.", dto.DatasetId);
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    { //EDIT A REPORT
                        bool IsSucessful = _businessIntelligenceService.UpdateAndSaveBusinessIntelligence(dto);
                        if (IsSucessful)
                        {
                            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.UPDATED_REPORT, crm.DatasetName + " was updated.", dto.DatasetId);
                            return RedirectToAction("Detail", new { id = dto.DatasetId });
                        }
                    }
                }
            }

            ReportUtility.SetupLists(_datasetContext, crm);
            return View(crm);
        }



        [HttpPost]
        [Route("BusinessIntelligence/Delete/{id}/")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public JsonResult Delete(int id)
        {
            try
            {
                _businessIntelligenceService.Delete(id);
                _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.DELETED_REPORT, "Deleted Report", id);
                return Json(new { Success = true, Message = "Exhibit was successfully deleted" });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete dataset - DatasetId:{id} RequestorId:{SharedContext.CurrentUser.AssociateId} RequestorName:{SharedContext.CurrentUser.DisplayName}", ex);
                return Json(new { Success = false, Message = "We failed to delete exhibit.  Please try again later." });
            }

        }



        [HttpGet]
        [Route("BusinessIntelligence/Detail/{id}/")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_VIEW)]
        public ActionResult Detail(int id)
        {
            BusinessIntelligenceDetailDto dto = _businessIntelligenceService.GetBusinessIntelligenceDetailDto(id);
            BusinessIntelligenceDetailModel model = new BusinessIntelligenceDetailModel(dto);
            model.CLA1130_SHOW_ALTERNATE_EMAIL = _featureFlags.CLA1130_SHOW_ALTERNATE_EMAIL.GetValue();

            _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Viewed Business Intelligence Detail Page", dto.DatasetId);
            return View(model);
        }

        [HttpGet]
        [Route("businessIntelligence/{id}/Favorites/")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_VIEW)]
        public JsonResult FavoritesDetail(int Id)
        {
            List<FavoriteDto> favList = _businessIntelligenceService.GetDatasetFavoritesDto(Id);
            FavoritesDetailModel model = favList.ToModel();
            return Json(JsonConvert.SerializeObject(model), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult CreateTag()
        {
            TagModel model = new TagModel()
            {
                AllTagGroups = Utility.BuildSelectListitem(_businessIntelligenceService.GetAllTagGroups(), ""),
                CreationUserId = SharedContext.CurrentUser.AssociateId,
            };

            return PartialView("_TagForm", model);
        }

        [HttpPost]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult TagForm(TagModel model)
        {
            TagDto dto = model.ToDto();

            AddCoreValidationExceptionsToModel(_tagService.Validate(dto));

            if (ModelState.IsValid)
            {
                if (dto.TagId == 0)
                {
                    bool IsSuccessful = _tagService.CreateAndSaveNewTag(dto);
                    if (IsSuccessful)
                    {
                        _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.CREATED_TAG, model.TagName + " was created.", dto.TagId);
                        return PartialView("_Success", new SuccessModel("Tag successfully added.", "", true));
                    }
                }
                else
                {
                    bool IsSuccessful = _tagService.UpdateAndSaveTag(dto);
                    if (IsSuccessful)
                    {
                        _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.UPDATED_TAG, model.TagName + " was updated.", dto.TagId);
                        return PartialView("_Success", new SuccessModel("Tag successfully updated.", "", true));
                    }
                }

                return PartialView("_Success", new SuccessModel("There was an error adding the tag.", "", false));
            }

            return PartialView("_TagForm", model);
        }


        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_VIEW)]
        public ActionResult GetImage(string url, int? t)
        {
            if (url == null || !url.StartsWith("images/"))
            {
                return HttpNotFound();
            }

            byte[] img = _businessIntelligenceService.GetImageData(url, t);

            return File(img, "image/jpg", "image_" + System.IO.Path.GetFileName(url));
        }


        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult UploadPreviewImage()
        {
            HttpFileCollectionBase Files;
            Files = Request.Files;


            ImageDto dto = BusinessIntelligenceExtensions.ToDto(new ImageModel(), Files[0], 0, true);
            bool result = _businessIntelligenceService.SaveTemporaryPreviewImage(dto);

            ImageModel model = ImageExtensions.ToModel(dto);

            return PartialView("_Images", model);
            //return Json(model, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeByPermission(GlobalConstants.PermissionCodes.REPORT_MODIFY)]
        public ActionResult NewImage(int index = 0)
        {
            ImageModel im = new ImageModel();
            ViewData["index"] = index;
            return PartialView("_Images", im);
        }
    }
}