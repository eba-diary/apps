using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    public class AccessRequestController : BaseController
    {

        private readonly IObsidianService _obsidianService;
        private readonly IDatasetService _datasetService;
        private readonly INotificationService _notificationService;

        public AccessRequestController(IObsidianService obsidianService, IDatasetService datasetService, INotificationService notificationService)
        {
            _obsidianService = obsidianService;
            _notificationService = notificationService;
            _datasetService = datasetService;
        }

        [HttpPost]
        public ActionResult SubmitAccessRequest(AccessRequestModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = _datasetService.RequestAccessToDataset(ar);

            if (string.IsNullOrEmpty(ticketId))
            {
                return PartialView("_Success", new SuccessModel("There was an error processing your request.", "", false));
            }
            else
            {
                return PartialView("_Success", new SuccessModel("Dataset access was successfully requested.", "HPSM Change Id: " + ticketId, true));
            }
        }

        [HttpGet]
        public ActionResult CheckAdGroup(string adGroup)
        {
            return Json(_obsidianService.DoesGroupExist(adGroup), JsonRequestBehavior.AllowGet);
        }


    }
}