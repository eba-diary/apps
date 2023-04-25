using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class GlobalDatasetSearchController : BaseSearchableController
    {
        private readonly IGlobalDatasetService _globalDatasetService;

        public GlobalDatasetSearchController(IGlobalDatasetService globalDatasetService, IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            _globalDatasetService = globalDatasetService;
        }

        [ChildActionOnly]
        public override ActionResult Results(Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            throw new NotImplementedException();
        }
    }
}