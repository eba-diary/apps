using System;
using System.Web.Mvc;
using Sentry.data.Web.Controllers;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class InjectSharedContextAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            BaseController controller = null;
            try
            {
                controller = (BaseController)filterContext.Controller;
            }
            catch (Exception)
            {

            }
            
            if(controller != null)
            {
                UserService userService = DependencyResolver.Current.GetService<UserService>();
                SharedContextModel sharedContext = new SharedContextModel();
                sharedContext.CurrentRealUser = userService.GetCurrentRealUser();
                sharedContext.CurrentUser = userService.GetCurrentUser();
                controller.SharedContext = sharedContext;
            }

            base.OnActionExecuting(filterContext);  
        }
    }
}
