using System;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web;

namespace Sentry.data.Web
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AntiForgeryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (shouldValidateManually(filterContext))
            {
                if (filterContext.RequestContext.HttpContext.Request.UrlReferrer != null ||
                      filterContext.RequestContext.HttpContext.Request.UrlReferrer.Host.Contains(Sentry.Configuration.Config.GetHostSetting("ValidURL")) ||
                      string.IsNullOrEmpty(filterContext.RequestContext.HttpContext.Request.UrlReferrer.Host))
                {
                    //request is either coming internally or from unknown source, so let it fall into the actual XSRF check
                }
                else
                {
                    //bad referrer header.  This is here mainly to appease a particular security app scan.  It ensures that 
                    //non-sentry.com referrers are blocked from being able to POST to our application.
                    throw new Exception("Bad referrer.  Failing out on a possible CSRF attack.");
                }

                //The token may be in the http headers.  This happens if you make an ajax request using $.ajax or one of its
                //relatives.  If we find one there, let's use it
                string token = filterContext.HttpContext.Request.Headers["__RequestVerificationToken"];

                if (string.IsNullOrEmpty(token) == false)
                {
                    HttpCookie cookie = filterContext.HttpContext.Request.Cookies[AntiForgeryConfig.CookieName];
                    string cookieValue = cookie != null ? cookie.Value : null;
                    AntiForgery.Validate(cookieValue, token);
                }
                else
                {
                    // We didn't find an http header with the token, so let's assume it's in the form data and use the
                    // default out-of-the-box request forgery checking mechanism
                    AuthorizationContext authorizationContext = new AuthorizationContext(filterContext.Controller.ControllerContext, filterContext.ActionDescriptor);
                    ValidateAntiForgeryTokenAttribute validate = new ValidateAntiForgeryTokenAttribute();

                    validate.OnAuthorization(authorizationContext);

                }
            }
            base.OnActionExecuting(filterContext);
        }

        public Boolean shouldValidateManually(ActionExecutingContext filterContext)
        {
            string httpMethod = filterContext.HttpContext.Request.HttpMethod;

            if(httpMethod.CompareTo("POST") != 0)
            {
                return false;
            }

            var antiForgeryAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false);

            if (antiForgeryAttributes.Length > 0)
            {
                return false;
            }
            var ignoreAntiForgeryAttribute = filterContext.ActionDescriptor.GetCustomAttributes(typeof(BypassAntiForgeryAttribute), false);

            if(ignoreAntiForgeryAttribute.Length > 0)
            {
                return false;
            }

            return true;
        }
    }
}
