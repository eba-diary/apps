using System;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if ((filterContext.Result.GetType() != typeof(FileResult)) &&
                (filterContext.Result.GetType() != typeof(FileContentResult)))
            {

                HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                HttpContext.Current.Response.Cache.SetValidUntilExpires(false);
                //uncomment this line to get a must-revalidate added in the header also
                //HttpContext.Current.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches) 
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                HttpContext.Current.Response.Cache.SetNoStore();
            }

            base.OnResultExecuting(filterContext);
        }

    }
}
