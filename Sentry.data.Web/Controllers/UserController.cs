using Sentry.data.Core;
using System.Web.Mvc;
using System.Web.SessionState;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class UserController : BaseController
    {
        private WebCurrentUserIdProvider _webCurrentUserIdProvider;
        private IAssociateInfoProvider _associateInfoProvider;

        public UserController(WebCurrentUserIdProvider webCurrentUserIdProvider, IAssociateInfoProvider associateInfoProvider)
        {
            _webCurrentUserIdProvider = webCurrentUserIdProvider;
            _associateInfoProvider = associateInfoProvider;
        }

        // GET: User/Switch
        [AllowUnAuthorized()]
        public ActionResult Switch()
        {
            UserSwitchModel model = new UserSwitchModel();
            model.CurrentRealUserName = SharedContext.CurrentRealUser.DisplayName;
            if ((SharedContext.CurrentUser).GetType() == typeof(ImpersonatedApplicationUser))
            {
                model.CurrentImpersonatedUserName = SharedContext.CurrentUser.DisplayName;
                model.ImpersonatedUser = SharedContext.CurrentUser;
            }

            


            return View(model);
        }

        [HttpPost]
        [AuthorizeUserSwitch()]
        public ActionResult Switch(UserSwitchModel model)
        {
            if (ModelState.IsValid)
            {
                if (_associateInfoProvider.GetAssociateInfo(model.OwnerID) != null)
                {
                    _webCurrentUserIdProvider.SetImpersonatedUserId(model.OwnerID);
                    return RedirectToAction("Switch");
                }
                else
                {
                    ModelState.AddModelError("OwnerID", "User Id not found");
                }
            }
            return View(model);
        }

        [AuthorizeUserSwitch()]
        public ActionResult RequestSwitch(string id)
        {
            _webCurrentUserIdProvider.SetImpersonatedUserId(id);
            return RedirectToAction("Switch");
        }

        [AllowUnAuthorized()]
        public ActionResult Restore()
        {
            _webCurrentUserIdProvider.ClearImpersonatedUserId();
            return RedirectToAction("Switch");
        }
    }
}
