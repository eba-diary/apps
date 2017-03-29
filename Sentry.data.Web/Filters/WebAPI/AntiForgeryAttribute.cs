using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Net.Http.Headers;

namespace Sentry.data.Web.WebApi
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AntiForgeryAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (shouldValidateManually(actionContext))
            {
                if (actionContext.Request.Headers.Referrer != null ||
                       actionContext.Request.Headers.Referrer.Host.Contains(Sentry.Configuration.Config.GetHostSetting("ValidURL")) ||
                      string.IsNullOrEmpty(actionContext.Request.Headers.Referrer.Host))
                {
                    //request is either coming internally or from unknown source, so let it fall into the actual XSRF check
                }
                else
                {
                    //bad referrer header.  This is here mainly to appease a particular security app scan.  It ensures that 
                    //non-sentry.com referrers are blocked from being able to POST to our application.
                    throw new Exception("Bad referrer.  Failing out on a possible CSRF attack.");
                }

                string token = actionContext.Request.Headers.GetValues("__RequestVerificationToken").FirstOrDefault();

                string cookieValue = GetCookieValue(actionContext.Request.Headers, AntiForgeryConfig.CookieName);
                AntiForgery.Validate(cookieValue, token);
            }
            base.OnActionExecuting(actionContext);
        }

        private Boolean shouldValidateManually(HttpActionContext actionContext)
        {
            string httpMethod = actionContext.Request.Method.Method;

            if (httpMethod.CompareTo("POST") != 0)
            {
                return false;
            }

            return true;
        }


        private string GetCookieValue(HttpRequestHeaders header, string cookieName)
        {
            IEnumerable<string> cookies;
            if (header.TryGetValues("Cookie", out cookies))
            {
                foreach (string cookie in cookies)
                {
                    CookieHeaderValue chv;
                    if (CookieHeaderValue.TryParse(cookie, out chv))
                    {
                        if (chv[cookieName] != null)
                        {
                            return chv[cookieName].Value;
                        }
                    }
                }
            }
            return null;
        }
    }
}
