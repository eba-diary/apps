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

        private readonly IAssociateInfoProvider _associateInfoProvider;
        private readonly IReportContext _reportContext;
        private readonly UserService _userService;
        private readonly IBusinessIntelligenceService _businessIntelligenceService;

        public BusinessIntelligenceController(
            IReportContext rptCtxt, 
            IAssociateInfoProvider associateInfoService,
            IBusinessIntelligenceService businessIntelligenceService,
            UserService userService)
        {
            _reportContext = rptCtxt;
            _associateInfoProvider = associateInfoService;
            _userService = userService;
            _businessIntelligenceService = businessIntelligenceService;
        }



        // GET: Report
        [AuthorizeByPermission(PermissionNames.ReportView)]
        public ActionResult Index()
        {
            BusinessIntelligenceHomeModel rhm = _businessIntelligenceService.GetHomeDto().ToModel();

            Event e = new Event
            {
                EventType = _reportContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Business Intelligence Home Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(rhm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        public ActionResult Create()
        {
            BusinessIntelligenceModel cdm = new BusinessIntelligenceModel
            {
                DatasetId = 0,
                CanEditDataset = SharedContext.CurrentUser.CanEditDataset,
                CanUpload = SharedContext.CurrentUser.CanUpload,
                CreationUserName = SharedContext.CurrentUser.AssociateId
            };

            ReportUtility.SetupLists(_reportContext, cdm);

            //Update this to use Jereds new Eventing Service so this whole call is done async.
            Event e = new Event
            {
                EventType = _reportContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Report Creation Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(cdm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        public ActionResult Edit(int id)
        {
            BusinessIntelligenceDto dto = _businessIntelligenceService.GetBusinessIntelligenceDto(id);

            BusinessIntelligenceModel model = new BusinessIntelligenceModel(dto);

            ReportUtility.SetupLists(_reportContext, model);

            Event e = new Event
            {
                EventType = _reportContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _reportContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Report Edit Page"
            };
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


            return View(model);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.ManageReports)]
        public ActionResult Create(BusinessIntelligenceModel crm) //update the name of this to Submit or something
        {
            BusinessIntelligenceDto dto = crm.ToDto();

            AddCoreValidationExceptionsToModel(_businessIntelligenceService.Validate(dto));

            if (ModelState.IsValid)
            {
                bool IsSucessful = _businessIntelligenceService.CreateAndSaveBusinessIntelligenceDataset(dto);

                if (IsSucessful)
                {
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

                    if (dto.DatasetId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Detail", new { id = dto.DatasetId });
                    }
                }
            }

            ReportUtility.SetupLists(_reportContext, crm);

            return View(crm);
        }



    }
}