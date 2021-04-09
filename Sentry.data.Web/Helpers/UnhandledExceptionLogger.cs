using Sentry.Common.Logging;
using System;
using System.Web.Http.ExceptionHandling;

namespace Sentry.data.Web
{
    /// <summary>
    /// Global Web API Unhandled Exception Logger
    /// </summary>
    public class UnhandledExceptionLogger : System.Web.Http.ExceptionHandling.ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            Logger.Fatal(context.Exception.GetType().FullName + ": " + context.Exception.Message + Environment.NewLine + context.Request.Method.ToString() + " " + context.Request.RequestUri.ToString(),
                context.Exception);
            base.Log(context);
        }
    }
}