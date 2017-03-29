
using System;
using System.Linq;
using System.Web.Mvc;
using Sentry.Common.Logging;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public abstract class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        /// <summary>
        /// Returns a value that is used to check if a user has access to a certain part of the app.
        /// A True return value indicates that the user has access.  False means they are denied access.
        /// </summary>
        /// <param name="appUser">The application user object</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract Boolean DoesUserHaveAccess(IApplicationUser appUser);

        //MustOverride ReadOnly Property PropertyToCheck As Func(Of IApplicationUser, Boolean)
        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (ShouldAuthenticate(filterContext))
            {
                base.OnAuthorization(filterContext);

                try
                {
                    UserService userService = DependencyResolver.Current.GetService<UserService>();
                    IApplicationUser user = userService.GetCurrentUser();
                    if (DoesUserHaveAccess(user) == false)
                    {
                        HandleUnauthorizedRequest(filterContext);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error occurred when verifying if user {0} has access to application", System.Web.HttpContext.Current.User.Identity.Name), ex);
                    HandleUnauthorizedRequest(filterContext);
                }
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                    throw new NotAuthorizedException("User is authenticated but does not have permission");
            }
            else
            {
                    base.HandleUnauthorizedRequest(filterContext);
            }
        }

        public Boolean ShouldAuthenticate(System.Web.Mvc.AuthorizationContext filterContext)
        {

            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowUnAuthorizedAttribute), false).Any() ||
               filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AllowUnAuthorizedAttribute), false).Any())
            {
                return false;
            }

            return true;
        }
    }

}
