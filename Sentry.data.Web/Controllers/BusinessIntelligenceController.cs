﻿using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Web.Helpers;
using System;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    [AuthorizeByPermission(PermissionNames.ManageReports)]
    public class BusinessIntelligenceController : BaseController
    {

        private readonly IDatasetContext _datasetContext;
        private readonly IBusinessIntelligenceService _businessIntelligenceService;
        private readonly IEventService _eventService;

        public BusinessIntelligenceController(
            IDatasetContext datasetContext,
            IBusinessIntelligenceService businessIntelligenceService,
            IEventService eventService)
        {
            _datasetContext = datasetContext;
            _businessIntelligenceService = businessIntelligenceService;
            _eventService = eventService;
        }


        public ActionResult Index()
        {
            BusinessIntelligenceHomeModel rhm = _businessIntelligenceService.GetHomeDto().ToModel();

            _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Business Intelligence Home Page", 0);
            return View(rhm);
        }


        [HttpGet]
        public ActionResult Create()
        {
            BusinessIntelligenceModel cdm = new BusinessIntelligenceModel
            {
                DatasetId = 0,
                CreationUserName = SharedContext.CurrentUser.AssociateId,
                UploadUserName = SharedContext.CurrentUser.AssociateId,
                BusinessObjectsEnumValue = (int)ReportType.BusinessObjects
            };

            ReportUtility.SetupLists(_datasetContext, cdm);

            _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Report Creation Page", cdm.DatasetId);
            return View("BusinessIntelligenceForm",cdm);
        }



        [HttpGet]
        public ActionResult Edit(int id)
        {
            BusinessIntelligenceDto dto = _businessIntelligenceService.GetBusinessIntelligenceDto(id);

            BusinessIntelligenceModel model = new BusinessIntelligenceModel(dto);

            ReportUtility.SetupLists(_datasetContext, model);

            _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Report Edit Page", dto.DatasetId);
            return View("BusinessIntelligenceForm",model);
        }



        [HttpPost]
        public ActionResult BusinessIntelligenceForm(BusinessIntelligenceModel crm) 
        {
            AddCoreValidationExceptionsToModel(crm.Validate());

            if (ModelState.IsValid)
            {
                BusinessIntelligenceDto dto = crm.ToDto();

                if(dto.DatasetId == 0)
                { //CREATE A REPORT
                    AddCoreValidationExceptionsToModel(_businessIntelligenceService.Validate(dto));
                    if (ModelState.IsValid)
                    {
                        bool IsSucessful = _businessIntelligenceService.CreateAndSaveBusinessIntelligence(dto);
                        if (IsSucessful)
                        {
                            _eventService.PublishSuccessEvent(GlobalConstants.EventType.CREATED_REPORT, SharedContext.CurrentUser.AssociateId, crm.DatasetName + " was created.", dto.DatasetId);
                            return RedirectToAction("Index");
                        }
                    }
                }
                else
                { //EDIT A REPORT
                    bool IsSucessful = _businessIntelligenceService.UpdateAndSaveBusinessIntelligence(dto);
                    if (IsSucessful)
                    {
                        _eventService.PublishSuccessEvent(GlobalConstants.EventType.UPDATED_REPORT, SharedContext.CurrentUser.AssociateId, crm.DatasetName + " was updated.", dto.DatasetId);
                        return RedirectToAction("Detail", new { id = dto.DatasetId });
                    }
                }
            }

            ReportUtility.SetupLists(_datasetContext, crm);
            return View(crm);
        }



        [HttpPost]
        [Route("BusinessIntelligence/Delete/{id}/")]
        public JsonResult Delete(int id)
        {
            try
            {
                _businessIntelligenceService.Delete(id);
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
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Detail(int id)
        {
            if (!SharedContext.CurrentUser.CanViewReports)
            {
                throw new NotAuthorizedException("User is authenticated but does not have permission");
            }
            BusinessIntelligenceDetailDto dto = _businessIntelligenceService.GetBusinessIntelligenceDetailDto(id);
            BusinessIntelligenceDetailModel model = new BusinessIntelligenceDetailModel(dto);

            _eventService.PublishSuccessEvent(GlobalConstants.EventType.VIEWED, SharedContext.CurrentUser.AssociateId, "Viewed Business Intelligence Detail Page", dto.DatasetId);
            return View(model);
        }

    }
}