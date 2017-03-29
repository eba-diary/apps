using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class ProfilingActionFilterAttribute : ActionFilterAttribute
    {
        const string stackKey = "ProfilingActionFilterAttributeStack";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MiniProfiler mp = MiniProfiler.Current;

            if (mp != null)
            {

                Stack<IDisposable> stk = (Stack<IDisposable>)HttpContext.Current.Items[stackKey];
                
                if (stk != null)
                {
                    stk = new Stack<IDisposable>();
                    HttpContext.Current.Items[stackKey] = stk;
                }
                else
                {
                    stk = new Stack<IDisposable>();
                }

                IDisposable prof = MiniProfiler.Current.Step("Controller: " + filterContext.Controller.ToString().Replace("Sentry.SalesCenter.Web.", "") + "." + filterContext.ActionDescriptor.ActionName);
                stk.Push(prof);
            }
            base.OnActionExecuting(filterContext);
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            Stack<IDisposable> stk = (Stack<IDisposable>)HttpContext.Current.Items[stackKey];
            if (stk != null && stk.Count > 0)
            {
                stk.Pop().Dispose();
            }
        }
    }
}
