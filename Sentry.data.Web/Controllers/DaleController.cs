using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
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
            if( CanDaleView() ) 
            {
                DaleSearchModel searchModel = new DaleSearchModel();
                searchModel.CanDaleSensitiveView = CanDaleSensitiveView();
                searchModel.CanDaleSensitiveEdit = CanDaleSensitiveEdit();

                return View(searchModel);
            }
            else
            {
                return View("Forbidden");
            }
        }

        //use for ServerSide DataTable processing
        //public JsonResult GetSearchResultsServer([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest,string searchCriteria, string destination)
        //{
        //    DaleSearchModel searchModel = new DaleSearchModel();
        //    searchModel.Criteria = searchCriteria;
        //    searchModel.Destiny = destination.ToDaleDestiny();

        //    if (IsCriteriaValid(searchModel))
        //    {
        //        searchModel.DaleResults = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
        //    }
        //    else
        //    {
        //        searchModel.DaleResults = new List<DaleResultModel>();
        //    }

        //    DataTablesQueryableAdapter<DaleResultModel> dtqa = new DataTablesQueryableAdapter<DaleResultModel>(searchModel.DaleResults.AsQueryable(), dtRequest);
        //    DataTablesResponse response = dtqa.GetDataTablesResponse();

        //     return Json(response);
        //}

        //use for ClientSide DataTable processing
        public JsonResult GetSearchResultsClient(string searchCriteria, string destination, bool sensitive=false)
        {
            DaleSearchModel searchModel = new DaleSearchModel();
            searchModel.Criteria = searchCriteria;
            searchModel.Destiny = destination.ToDaleDestiny();
            searchModel.CanDaleSensitiveView = CanDaleSensitiveView();
            searchModel.CanDaleSensitiveEdit = CanDaleSensitiveEdit();

            if (sensitive && searchModel.CanDaleSensitiveView)
            {
                searchModel.Sensitive = DaleSensitive.SensitiveOnly;
            }
            else if (searchModel.CanDaleSensitiveView)
            {
                searchModel.Sensitive = DaleSensitive.SensitiveAll;
            }
            else
            {
                searchModel.Sensitive = DaleSensitive.SensitiveNone;
            }

            //DO NOT perform search if invalid criteria OR sensitive and they lack permissions. NOTE: if they lack permissions, VIEW hides ability to even click sensitive link
            if (!IsCriteriaValid(searchModel) || ( sensitive && !CanDaleSensitiveView() ) )            
            {
                searchModel.DaleResults = new List<DaleResultModel>();
            }
            else
            {
                searchModel.DaleResults = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
            }

            JsonResult result = Json(new { data = searchModel.DaleResults}, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = Int32.MaxValue;  //need to set MaxJsonLength to avoid 500 exceptions because of large json coming back since we are doing client side for max performance

            return result;
        }

        private bool IsCriteriaValid(DaleSearchModel model)
        {
            //if sensitive query, don't bother to validate criteria and immediately return true
            if (model.Sensitive == DaleSensitive.SensitiveOnly)
            {
                return true;
            }

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

        private bool CanDaleSensitiveView()
        {
            if (!SharedContext.CurrentUser.CanDaleSensitiveView)
            {
                return false;
            }

            return true;
        }

        private bool CanDaleView()
        {
            if (!SharedContext.CurrentUser.CanDaleView)
            {
                return false;
            }

            return true;
        }

        private bool CanDaleSensitiveEdit()
        {
            if( (_featureFlags.Dale_Expose_EditSensitive_CLA_2025.GetValue() && SharedContext.CurrentUser.CanDaleSensitiveEdit)
                || SharedContext.CurrentUser.IsAdmin
              )
            {
                return true;
            }

            return false;
        }

        private bool CanDaleUserVerifiedEdit()
        {
            if ((_featureFlags.Dale_Expose_EditUserVerified_CLA_1911.GetValue() && SharedContext.CurrentUser.CanDaleUserVerifiedEdit)
                || SharedContext.CurrentUser.IsAdmin
              )
            {
                return true;
            }

            return false;
        }

        [HttpGet]
        //method called by dale.js to return whether user can edit IsSensitive IND
        public JsonResult GetCanDaleSensitiveEdit()
        {
            //return Json(CanDaleSensitiveEdit(),JsonRequestBehavior.AllowGet);
            return Json(new {canDaleSensitiveEdit = CanDaleSensitiveEdit(), canDaleUserVerifiedEdit = CanDaleUserVerifiedEdit() },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateIsSensitive(List<DaleSensitiveModel> models)
        {
            bool success = false;

            success = _daleService.UpdateIsSensitive(models.ToDto());

            if(success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }
    }
}