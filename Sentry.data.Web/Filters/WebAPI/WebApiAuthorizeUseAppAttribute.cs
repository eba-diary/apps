using Sentry.data.Core;

namespace Sentry.data.Web.WebApi
{
    /// <summary>
    /// This Web API authorization filter is applied globally, and ensures that all users at least have the basic Use App permission
    /// </summary>
    public class WebApiAuthorizeUseAppAttribute : WebApiAuthorizeAttribute
    {
        public override bool DoesUserHaveAccess(IApplicationUser appUser)
        {
            //return appUser.CanUseApp;
            //TEMPORARY! hardcode this for now just for testing until Jered uncouples API from UI
            return true;
        }
    }
}