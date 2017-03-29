using System;
using System.Web;
using Sentry.Configuration;
using System.Text;

namespace Sentry.data.Web
{
    public class LoggerHelper
    {
        /// <summary>
        /// Wire up logging to use Sentry.Common.Logging
        /// </summary>
        public static void ConfigureLogger()
        {
            Sentry.Common.Logging.Logger.LoggingFrameworkAdapter =
                new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));
            Sentry.Web.Logging.Logger.LoggingFrameworkAdapter =
                new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("SentryWebLogger"));
            Sentry.Smarts.Logging.Logger.LoggingFrameworkAdapter =
                new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("SmartsLogger"));
        //TODO: If you are using any other Sentry components (like Sentry.Eventing, etc.) you can wire up their loggers here also
        }

        /// <summary>
        /// Logs an exception that occurred in the web layer
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="context">The current httpcontext</param>
        public static void LogWebException(Exception exception, System.Web.HttpContext context)
        {
            string logMessage = GetLogMessage(exception, context);

            if (logMessage.Contains("The remote host closed the connection"))
            {
                Sentry.Common.Logging.Logger.Info(logMessage);
            }
            else if (Is404Exception(exception))
            {
                Sentry.Common.Logging.Logger.Warn(logMessage);
            }
            else
            {
                Sentry.Common.Logging.Logger.Fatal(logMessage);
            }
        }

        /// <summary>
        /// Safely determine if the exception that was thrown was a 404 Not Found error
        /// </summary>
        /// <returns>True if the exception is an HTTP 404 Not Found error</returns>
        private static Boolean Is404Exception(System.Exception exception)
        {
            return exception != null && 
                exception.GetType() == typeof(HttpException) && 
                ((HttpException)exception).GetHttpCode() == 404;
        }

        /// <summary>
        /// Get a formatted version of the exception/context/session info that can be logged to a file
        /// </summary>
        /// <returns>A string representation of the exception that can be logged to a file</returns>
        private static string GetLogMessage(System.Exception exception, System.Web.HttpContext context)
        {

            StringBuilder _message = new StringBuilder();

            try
            {
                if (exception != null)
                {
                    _message.Append(exception.Message);
                }
                else
                {
                    _message.Append("Exception is nothing!");
                }
                _message.Append(Environment.NewLine);

                _message.Append("User ID: ");
                if (context.User != null && context.User.Identity != null)
                {
                    _message.Append(context.User.Identity.Name);
                }
                _message.Append(Environment.NewLine);

                _message.Append("URL: ");
                _message.Append(context.Request.Url.OriginalString);
                _message.Append(Environment.NewLine);

                _message.Append("Server Name: ");
                _message.Append(Environment.MachineName);
                _message.Append(Environment.NewLine);

                _message.Append("Browser: ");
                _message.Append(context.Request.UserAgent);
                _message.Append(Environment.NewLine);

                _message.Append("Remote IP: ");
                _message.Append(context.Request.UserHostAddress);
                _message.Append(Environment.NewLine);

                Exception ex = exception;
                //strip off meaningless exception message:
                while (ex.Message.Contains("System.Web.HttpUnhandledException"))
                {
                    ex = ex.InnerException;
                }

                do
                {
                    _message.Append(string.Format("[{0}]", ex.Message));
                    _message.Append(Environment.NewLine);
                    _message.Append(ex.StackTrace);
                    _message.Append(Environment.NewLine);
                    ex = ex.InnerException;
                } while (ex != null);
            }
            catch (Exception ex)
            {
                _message.Append(Environment.NewLine);
                _message.Append("In addition, an error occurred while formatting the log message: " + ex.ToString());
            }
            finally
            {
                _message.Append(Environment.NewLine);
            }
            return _message.ToString();

        }
    }
}
