using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //Adding the following global filter makes every controller action require authorization
            filters.Add(new AuthorizeUseAppAttribute());

            //Adding the following global filter makes any error in any controller redirect to the Shared\Error view
            filters.Add(new HandleErrorAttribute());

            //Adding the following global filter forces an AntiForgeryToken on all forms
            filters.Add(new AntiForgeryAttribute());

            //Adding the following global filter forces all pages to have no-cache, no-store as their cache header (required by security)
            //Static content that goes through the content helper controller will still be cached, since it's programatically overridden there
            filters.Add(new NoCacheAttribute());

            //This filter ensures that the BaseController.SharedContext and ViewData.SharedContext is populated
            filters.Add(new InjectSharedContextAttribute());
        }
    }
}
