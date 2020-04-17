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
    public class DaleController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly IDataFeatures _featureFlags;

        public DaleController(IEventService eventService,IDataFeatures featureFlags)
        {
            _eventService = eventService;
            _featureFlags = featureFlags;
        }

        //TODO create this to return model view
        public ActionResult DaleSearch()
        {
            if ( _featureFlags.Expose_DaleSearch_CLA_1450.GetValue() || SharedContext.CurrentUser.IsAdmin)
            {
                DaleSearchModel model = new DaleSearchModel();
                return View(model);
            }
            else
            {
                return View("Forbidden");
            }
        }
    }
}