using Sentry.data.Core;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace Sentry.data.Web.Filters.AuthorizationFilters
{
    public abstract class WebApiAuthorizeAttribute : System.Web.Http.Filters.AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (ShouldAuthenticate(actionContext)){
                base.OnAuthorization(actionContext);

                try
                {
                    UserService userService = DependencyResolver.Current.GetService<UserService>();
                    IApplicationUser user = userService.GetCurrentUser();
                    if (!DoesUserHaveAccess(user))
                    {
                        HandleUnauthorizedRequest(actionContext);
                    }

                }
                catch (Exception ex)
                {
                    Sentry.Common.Logging.Logger.Error($"Error occurred when verifying if user {System.Web.HttpContext.Current.User.Identity.Name} has access to application", ex);
                    HandleUnauthorizedRequest(actionContext);
                }
            }            
        }

        private void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Returns a value that is used to check if a user has access to a certain part of the app.
        /// A True return value indicates that the user has access.  False means they are denied access.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public abstract Boolean DoesUserHaveAccess(IApplicationUser appUser);


        public bool ShouldAuthenticate(HttpActionContext actionContext)
        {
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowUnAuthorizedAttribute>(false).Any() || actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<System.Web.Http.AllowAnonymousAttribute>(false).Any())
            {
                return false;
            }

            return true;
        }
    }
}