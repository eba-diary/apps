using System.Web.Mvc;
using System.Web.SessionState;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class LayoutController : BaseController
    {
        [ChildActionOnly()]
        public ActionResult GetHeader()
        {
            HeaderModel headerModel = new HeaderModel();
            headerModel.CanUserSwitch = (SharedContext.CurrentUser.CanUserSwitch && Configuration.Config.GetHostSetting("ShowUserChoice").ToLower() == "true");
            headerModel.CurrentUserName = SharedContext.CurrentUser.DisplayName;
            if (SharedContext.CurrentUser.GetType() == typeof(ImpersonatedApplicationUser))
            {
                headerModel.IsImpersonating = true;
                headerModel.RealUserName = SharedContext.CurrentRealUser.DisplayName;
            }
            else
            {
                headerModel.IsImpersonating = false;
            }
            //###BEGIN SENTRYBAY### - Code below is SentryBay-specific

            //###END SENTRYBAY### - Code above is SentryBay-specific
            headerModel.CanUseApp = SharedContext.CurrentUser.CanUseApp;
            headerModel.EnvironmentName = Sentry.Configuration.Config.GetHostSetting("EnvironmentName");
            

            return PartialView("_Header", headerModel);
        }

        [ChildActionOnly()]
        public ActionResult GetFooter()
        {
            FooterModel footerModel = new FooterModel();
            
            // Replace the following if necessary to load the correct version number for your app
            footerModel.AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return PartialView("_Footer", footerModel);
        }
    }
}
