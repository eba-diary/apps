using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Sentry.Common.Logging;
using Sentry.data.Core;

namespace Sentry.data.Web.WebApi
{
    public abstract class WebApiAuthorizeAttribute : System.Web.Http.Filters.AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (ShouldAuthenticate(actionContext))
            {
                base.OnAuthorization(actionContext);

                try
                {
                    var userService = DependencyResolver.Current.GetService<UserService>();
                    var user = userService.GetCurrentUser();
                    if (!DoesUserHaveAccess(user))
                    {
                        HandleUnauthorizedRequest(actionContext, user);
                    }
                }
                catch (NotAuthorizedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error occurred when verifying if user {0} has access to application", System.Web.HttpContext.Current.User.Identity.Name), ex);
                    HandleUnauthorizedRequest(actionContext);
                }
            }
        }

        /// <summary>
        /// Returns a value that is used to check if a user has access to a certain part of the app.
        /// A True return value indicates that the user has access.  False means they are denied access.
        /// </summary>
        /// <param name="appUser">The application user object</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool DoesUserHaveAccess(IApplicationUser appUser);

        protected void HandleUnauthorizedRequest(HttpActionContext actionContext, IApplicationUser appUser = null)
        {
            throw new NotAuthorizedException($"User {appUser?.AssociateId} is authenticated but does not have permission");
        }

        public bool ShouldAuthenticate(HttpActionContext actionContext)
        {
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowUnAuthorizedAttribute>(false).Any() ||
                actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>(false).Any())
            {
                return false;
            }                

            return true;
        }
    }

}