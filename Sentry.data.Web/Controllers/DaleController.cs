using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class DaleController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly IDataFeatures _featureFlags;
        private readonly IDaleService _daleService;

        public DaleController(IEventService eventService,IDataFeatures featureFlags, IDaleService daleService)
        {
            _eventService = eventService;
            _featureFlags = featureFlags;
            _daleService = daleService;
        }

        public ActionResult DaleSearch()
        {
            if ( _featureFlags.Expose_DaleSearch_CLA_1450.GetValue() || SharedContext.CurrentUser.IsAdmin)
            {
                DaleSearchModel searchModel = new DaleSearchModel();
                return View(searchModel);
            }
            else
            {
                return View("Forbidden");
            }
        }

        //use for ServerSide DataTable processing
        public JsonResult GetSearchResultsServer([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest,string searchCriteria, string destination)
        {
            DaleSearchModel searchModel = new DaleSearchModel();
            searchModel.Criteria = searchCriteria;
            searchModel.Destiny = destination.ToDaleDestiny();

            if(IsCriteriaValid(searchModel))
            {
                searchModel.DaleResults = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
            }
            else
            {
                searchModel.DaleResults = new List<DaleResultModel>();
            }

            DataTablesQueryableAdapter<DaleResultModel> dtqa = new DataTablesQueryableAdapter<DaleResultModel>(searchModel.DaleResults.AsQueryable(), dtRequest);
            DataTablesResponse response = dtqa.GetDataTablesResponse();

             return Json(response);
        }

        //use for ClientSide DataTable processing
        public JsonResult GetSearchResultsClient(string searchCriteria, string destination)
        {
            DaleSearchModel searchModel = new DaleSearchModel();
            searchModel.Criteria = searchCriteria;
            searchModel.Destiny = destination.ToDaleDestiny();

            if (IsCriteriaValid(searchModel))
            {
                searchModel.DaleResults = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
            }
            else
            {
                searchModel.DaleResults = new List<DaleResultModel>();
            }

            JsonResult result = Json(new { data = searchModel.DaleResults}, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = Int32.MaxValue;  //need to set MaxJsonLength to avoid 500 exceptions because of large json coming back since we are doing client side for max performance

            return result;
        }


        private bool IsCriteriaValid(DaleSearchModel model)
        {
            //validate for white space only, null, empty string in criteria
            if (String.IsNullOrWhiteSpace(model.Criteria))
            {
                return false;
            }

            //validate to ensure valid destination
            if ((model.Destiny != DaleDestiny.Object) && (model.Destiny != DaleDestiny.Column))
            {
                return false;
            }
            return true;
        }
    }
}