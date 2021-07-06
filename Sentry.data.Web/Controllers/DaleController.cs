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

        //ALLOWED TARGETS=entity,column,SAID
        //https://localhost.sentry.com:44371/DataInventory/Search/?target=column&search=weather&filter=ssn
        //https://localhost.sentry.com:44371/DataInventory/Search/?target=said&search=CODS
        [Route("DataInventory/Search/")]
        public ActionResult DaleSearch(string target=null, string search=null, string filter=null)
        {
            if( CanDaleView() ) 
            {
                DaleSearchModel searchModel = new DaleSearchModel();
                searchModel.CanDaleSensitiveView = CanDaleSensitiveView();
                searchModel.CanDaleSensitiveEdit = CanDaleSensitiveEdit();
                searchModel.DaleAdvancedCriteria = new DaleAdvancedCriteriaModel() { };

                
                if(String.IsNullOrEmpty(search))
                {
                    searchModel.Destiny = DaleDestiny.Column;
                }
                else
                {
                    searchModel.Criteria = search;                       
                    bool targetFound = false;

                    if(IsTargetValid(target))
                    {
                        if (target.ToUpper() == "SAID")
                        {
                            searchModel.Destiny = DaleDestiny.SAID;
                            targetFound = true;
                            
                        }
                        else if (target.ToUpper() == "ENTITY")
                        {
                            searchModel.Destiny = DaleDestiny.Object;
                            targetFound = true;
                        }
                    }
                    
                    if(!targetFound)
                    {
                        searchModel.Destiny = DaleDestiny.Column;           //default to column
                    }
                }

                return View(searchModel);
            }
            else
            {
                return View("Forbidden");
            }
        }

        private bool IsTargetValid(string target)
        {
            bool valid = false;

            if (String.IsNullOrEmpty(target))
            {
                return false;
            }

            if (target.ToUpper() == "SAID")
            {
                valid = true;
            }
            
            if (target.ToUpper() == "ENTITY")
            {
                valid = true;
            }

            return valid;
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
        public JsonResult GetSearchResultsClient(string searchCriteria, 
                                                string destination,
                                                string asset,
                                                string server,
                                                string database,
                                                string daleObject,
                                                string objectType,
                                                string column,
                                                string sourceType,
                                                bool sensitive=false
         )
        {
            DaleSearchModel searchModel = new DaleSearchModel();
            searchModel.Criteria = searchCriteria;
            searchModel.Destiny = destination.ToDaleDestiny();
            searchModel.CanDaleSensitiveView = CanDaleSensitiveView();
            searchModel.CanDaleSensitiveEdit = CanDaleSensitiveEdit();

            searchModel.DaleAdvancedCriteria = new DaleAdvancedCriteriaModel()
            {
                Asset = asset,
                Server = server,
                Database = database,
                Object = daleObject,
                ObjectType = objectType,
                Column = column,
                SourceType = sourceType
            };




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
                searchModel.DaleResultModel = new DaleResultModel();
                searchModel.DaleResultModel.DaleResults = new List<DaleResultRowModel>();
            }
            else
            {
                searchModel.DaleResultModel = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
            }

            JsonResult result = Json(new { data = searchModel.DaleResultModel.DaleResults}, JsonRequestBehavior.AllowGet);
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
            if (model.Destiny != DaleDestiny.Advanced && String.IsNullOrWhiteSpace(model.Criteria))
            {
                return false;
            }

            //validate to ensure valid destination
            if ((model.Destiny != DaleDestiny.Object) && (model.Destiny != DaleDestiny.Column) && (model.Destiny != DaleDestiny.SAID) && (model.Destiny != DaleDestiny.Advanced))
            {
                return false;
            }

            //validate that if advanced search is happening at least something is filled in
            if (model.Destiny == DaleDestiny.Advanced
                                    && (model.DaleAdvancedCriteria.AssetIsEmpty)
                                     && (model.DaleAdvancedCriteria.ServerIsEmpty)
                                     && (model.DaleAdvancedCriteria.DatabaseIsEmpty)
                                     && (model.DaleAdvancedCriteria.ObjectIsEmpty)
                                     && (model.DaleAdvancedCriteria.ObjectTypeIsEmpty)
                                     && (model.DaleAdvancedCriteria.ColumnIsEmpty)
                                     && (model.DaleAdvancedCriteria.SourceTypeIsEmpty)
                )
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
            return true;
        }

        private bool CanDaleSensitiveEdit()
        {
            if( SharedContext.CurrentUser.CanDaleSensitiveEdit || SharedContext.CurrentUser.IsAdmin )
            {
                return true;
            }

            return false;
        }

        private bool CanDaleOwnerVerifiedEdit()
        {
            if ((_featureFlags.Dale_Expose_EditOwnerVerified_CLA_1911.GetValue() && SharedContext.CurrentUser.CanDaleOwnerVerifiedEdit)
                || SharedContext.CurrentUser.IsAdmin
              )
            {
                return true;
            }

            return false;
        }

        [HttpGet]
        //method called by dale.js to return whether user can edit IsSensitive IND
        public JsonResult GetCanDaleSensitive()
        {
            return Json(new {canDaleSensitiveEdit = CanDaleSensitiveEdit(), canDaleOwnerVerifiedEdit = CanDaleOwnerVerifiedEdit(), canDaleSensitiveView = CanDaleSensitiveView() },JsonRequestBehavior.AllowGet);
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