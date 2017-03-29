using System;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BypassAntiForgeryAttribute : ActionFilterAttribute
    {

    }
}
