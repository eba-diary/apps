using System;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class WebCurrentUserIdProvider : ICurrentUserIdProvider
    {
        private const string IMPERSONATED_USER_ID_COOKIE_NAME = "ImpersonatedUserId";

        public void SetImpersonatedUserId(string userId)
        {
            if (GetRealUserId() == userId)
            {
                ClearImpersonatedUserId();
            }
            else
            {
                HttpCookie impersonationCookie = new System.Web.HttpCookie(IMPERSONATED_USER_ID_COOKIE_NAME);
                impersonationCookie.Value = userId;
                HttpContext.Current.Response.Cookies.Add(impersonationCookie);
            }
        }

        public void ClearImpersonatedUserId()
        {
            HttpCookie impersonationCookie = new System.Web.HttpCookie(IMPERSONATED_USER_ID_COOKIE_NAME);
            impersonationCookie.Value = "";
            impersonationCookie.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(impersonationCookie);
        }

        public string GetImpersonatedUserId()
        {
            HttpCookie impersonationCookie = HttpContext.Current.Request.Cookies[IMPERSONATED_USER_ID_COOKIE_NAME];
            if (impersonationCookie != null)
            {
                return impersonationCookie.Value;
            }
            return null;
        }

        public string GetRealUserId()
        {
            string[] identityName = HttpContext.Current.User.Identity.Name.Split('\\');
            return identityName.Last();
        }
    }
}
