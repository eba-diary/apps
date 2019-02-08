using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class BusinessUnitController : BaseController
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IEventService _eventService;

        public BusinessUnitController(IDatasetContext datasetContext, IEventService eventService)
        {
            _datasetContext = datasetContext;
            _eventService = eventService;
        }

        public ActionResult PersonalLines()
        {
            BusinessUnitLandingPageModel model = new BusinessUnitLandingPageModel();

            return View(model);
        }
    }
}