using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class BusinessIntelligenceSearchController : Controller
    {
        private readonly IUserService _userService;

        public BusinessIntelligenceSearchController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("Search/BusinessIntelligence")]
        [Route("BusinessIntelligence/Search")]
        public ActionResult Search()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            //validate user has permissions
            if (!user.CanViewReports)
            {
                return View("Forbidden");
            }

            ViewBag.Title = "Business Intelligence";
            SearchIndexModel model = new SearchIndexModel()
            {
                SearchType = GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH
            };

            return View(model);
        }
    }
}