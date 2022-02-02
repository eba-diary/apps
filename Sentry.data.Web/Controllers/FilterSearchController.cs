using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class FilterSearchController : Controller
    {
        [HttpPost]
        public ActionResult FilterCategories(List<FilterCategoryModel> filterCategories)
        {
            return PartialView("~/Views/Search/FilterCategories.cshtml", filterCategories.ToList());
        }

        [HttpPost]
        public ActionResult FilterShowAll(List<FilterCategoryModel> filterCategories)
        {
            return PartialView("~/Views/Search/FilterShowAll.cshtml", filterCategories.ToList());
        }
    }
}