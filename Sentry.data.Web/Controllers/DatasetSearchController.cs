using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : BaseSearchableController
    {
        public DatasetSearchController(IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            
        }

        //[Route("Search/Dataset")]
        //[Route("Dataset/Search")]
        //public ActionResult Search()
        //{
        //    //validate user has permissions
        //    if (!SharedContext.CurrentUser.CanViewDataset)
        //    {
        //        return View("Forbidden");
        //    }

        //    ViewBag.Title = "Dataset";
        //    SearchIndexModel model = new SearchIndexModel()
        //    {
        //        SearchType = SearchType.DATASET_SEARCH
        //    };

        //    return View(model);
        //}

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Dataset",
                SearchType = SearchType.DATASET_SEARCH,
                IconPath = "~/Images/Icons/DatasetsBlue.svg",
                ResultView = "~/Views/Search/TileResults.cshtml",
                DefaultSearch = searchModel
            };
        }
    }
}