using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{

    public class DaleController : BaseController
    {
        #region Fields
        private readonly IDataFeatures _featureFlags;
        private readonly IDaleService _daleService;
        #endregion

        #region Constructor
        public DaleController(IDataFeatures featureFlags, IDaleService daleService)
        {
            _featureFlags = featureFlags;
            _daleService = daleService;
        }
        #endregion

        #region Methods
        //ALLOWED TARGETS=entity,column,SAID
        //https://localhost.sentry.com:44371/DataInventory/Search/?target=column&search=weather&filter=ssn
        //https://localhost.sentry.com:44371/DataInventory/Search/?target=said&search=CODS
        [Route("DataInventory/Search/")]
        public ActionResult DaleSearch(string target=null, string search=null, string filter=null)
        {
            DaleSearchModel searchModel = new DaleSearchModel
            {
                CanDaleSensitiveView = CanDaleSensitiveView(),
                CanDaleSensitiveEdit = CanDaleSensitiveEdit(),
                DaleAdvancedCriteria = new DaleAdvancedCriteriaModel(),
                CLA3550_DATA_INVENTORY_NEW_COLUMNS = _featureFlags.CLA3550_DATA_INVENTORY_NEW_COLUMNS.GetValue(),
                CLA3707_UsingSQLSource = UsingSqlSource()
            };

            if (string.IsNullOrEmpty(search))
            {
                searchModel.Destiny = DaleDestiny.Column;
            }
            else
            {
                searchModel.Criteria = search;

                if (string.Equals(target, "SAID", StringComparison.OrdinalIgnoreCase))
                {
                    searchModel.Destiny = DaleDestiny.SAID;
                }
                else if (string.Equals(target, "ENTITY", StringComparison.OrdinalIgnoreCase))
                {
                    searchModel.Destiny = DaleDestiny.Object;
                }
                else
                {
                    searchModel.Destiny = DaleDestiny.Column;
                }
            }

            return View("DataInventorySearch", searchModel);
        }

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
            DaleSearchModel searchModel = new DaleSearchModel
            {
                Criteria = searchCriteria,
                CanDaleSensitiveView = CanDaleSensitiveView(),
                CanDaleSensitiveEdit = CanDaleSensitiveEdit(),
                CLA3707_UsingSQLSource = UsingSqlSource()
            };

            //if using SQL source, get the destiny enum value as usual
            //if using ELASTIC source, destiny will be null unless doing an advanced search because the radio buttons do not exist
            if (searchModel.CLA3707_UsingSQLSource)
            {
                searchModel.Destiny = destination.ToDaleDestiny();
            }
            else
            {
                searchModel.Destiny = string.IsNullOrEmpty(destination) ? default : DaleDestiny.Advanced;
            }

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
                searchModel.DaleResultModel = new DaleResultModel
                {
                    DaleResults = new List<DaleResultRowModel>()
                };
            }
            else
            {
                searchModel.DaleResultModel = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
            }

            JsonResult result = Json(new { data = searchModel.DaleResultModel.DaleResults}, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = Int32.MaxValue;  //need to set MaxJsonLength to avoid 500 exceptions because of large json coming back since we are doing client side for max performance

            return result;
        }

        [HttpGet]
        //method called by dale.js to return whether user can edit IsSensitive IND
        public JsonResult GetCanDaleSensitive()
        {
            return Json(new
            {
                canDaleSensitiveEdit = CanDaleSensitiveEdit(),
                canDaleOwnerVerifiedEdit = CanDaleOwnerVerifiedEdit(),
                canDaleSensitiveView = CanDaleSensitiveView(),
                CLA3707_UsingSQLSource = UsingSqlSource()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateIsSensitive(List<DaleSensitiveModel> models)
        {
            return Json(new { success = _daleService.UpdateIsSensitive(models.ToDto()) });
        }
        #endregion

        #region Private
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
            if (model.Destiny == DaleDestiny.Advanced && !model.DaleAdvancedCriteria.IsValid())
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

        private bool UsingSqlSource()
        {
            return _featureFlags.CLA3707_DataInventorySource.GetValue() == "SQL";
        }
        #endregion
    }
}