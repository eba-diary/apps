using System;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    public class SecurityController : BaseController
    {

        private readonly ISecurityService _securityService;

        public SecurityController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        [HttpGet]
        public ActionResult RequestAccess(RequestAccessModel model)
        {
            model.RequestorsId = SharedContext.CurrentUser.AssociateId;

            bool isSuccessfull = _securityService.RequestPermission(model.ToCore());

            return Json(isSuccessfull, JsonRequestBehavior.AllowGet);
        }
    }
}