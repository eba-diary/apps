using Sentry.data.Core;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public abstract class BaseDataInventoryController : BaseController
    {
        protected readonly IDataFeatures _featureFlags;

        protected BaseDataInventoryController(IDataFeatures featureFlags)
        {
            _featureFlags = featureFlags;
        }

        [HttpGet]
        //method called by dale.js to return whether user can edit IsSensitive IND
        public JsonResult GetCanDaleSensitive()
        {
            return Json(new
            {
                canDaleSensitiveEdit = CanEditSensitive(),
                canDaleOwnerVerifiedEdit = CanEditOwnerVerified(),
                canDaleSensitiveView = CanViewSensitive(),
                CLA3707_UsingSQLSource = UsingSqlSource()
            }, JsonRequestBehavior.AllowGet);
        }

        protected bool CanViewSensitive()
        {
            return SharedContext.CurrentUser.CanDaleSensitiveView;
        }

        protected bool CanEditSensitive()
        {
            return SharedContext.CurrentUser.CanDaleSensitiveEdit || SharedContext.CurrentUser.IsAdmin;
        }

        protected bool CanEditOwnerVerified()
        {
            return (_featureFlags.Dale_Expose_EditOwnerVerified_CLA_1911.GetValue() && SharedContext.CurrentUser.CanDaleOwnerVerifiedEdit) || SharedContext.CurrentUser.IsAdmin;
        }

        protected bool UsingSqlSource()
        {
            return _featureFlags.CLA3707_DataInventorySource.GetValue() == "SQL";
        }
    }
}