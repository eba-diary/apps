using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : Controller
    {
        private readonly IUserService _userService;

        public DatasetSearchController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("Search/Dataset")]
        [Route("Dataset/Search")]
        public ActionResult Search()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            //validate user has permissions
            if (!user.CanViewDataset)
            {
                return View("Forbidden");
            }

            ViewBag.Title = "Dataset";
            SearchIndexModel model = new SearchIndexModel()
            {
                SearchType = GlobalConstants.SearchType.DATASET_SEARCH
            };

            return View(model);
        }
    }
}