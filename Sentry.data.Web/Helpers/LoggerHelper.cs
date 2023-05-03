using Microsoft.Extensions.Logging;
using Sentry.EnterpriseLogging;
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class LoggerHelper
    {
        /// <summary>
        /// Wire up logging to use Sentry.Common.Logging
        /// </summary>
        public static ILoggerFactory ConfigureLogger()
        {
            Sentry.Common.Logging.Logger.LoggingFrameworkAdapter =
                new Sentry.Common.Logging.Adapters.Log4netAdapter("Sentry.Common.Logging");

            // The following is the new ILoggerFactory/ILogger setup for modern logging that is 
            // ready for .NET 5.
            var loggerFactory = new SentryLoggerFactory(new LoggerFactory());
            var log4netOptions = new Log4NetProviderOptions();
            if (Configuration.Config.GetDefaultEnvironmentName().ToUpper() == "DEV")
            {
                log4netOptions.Log4NetConfigFileName = "Log4net\\log4net.local.config";
            }
            else if (Configuration.Config.GetDefaultEnvironmentName().ToUpper() == "TEST")
            {
                log4netOptions.Log4NetConfigFileName = "Log4net\\log4net.server.test.config";
            }
            else if (Configuration.Config.GetDefaultEnvironmentName().ToUpper() == "NRTEST")
            {
                log4netOptions.Log4NetConfigFileName = "Log4net\\log4net.server.nrtest.config";
            }
            else if (Configuration.Config.GetDefaultEnvironmentName().ToUpper() == "QUAL")
            {
                log4netOptions.Log4NetConfigFileName = "Log4net\\log4net.server.qual.config";
            }
            else if (Configuration.Config.GetDefaultEnvironmentName().ToUpper() == "PROD")
            {
                log4netOptions.Log4NetConfigFileName = "Log4net\\log4net.server.prod.config";
            }

            loggerFactory.AddLog4Net(log4netOptions);

            return loggerFactory;

        }

        /// <summary>
        /// Logs an exception that occurred in the web layer
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="context">The current httpcontext</param>
        public static void LogWebException(Exception exception, System.Web.HttpContext context)
        {
            var logger = DependencyResolver.Current.GetService<ILogger<LoggerHelper>>();

            string logMessage = GetLogMessage(exception, context);

            if (logMessage.Contains("The remote host closed the connection"))
            {
                logger.LogInformation(logMessage);
            }
            else if (Is404Exception(exception))
            {
                logger.LogWarning(logMessage);
            }
            else
            {
                logger.LogCritical(logMessage);
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
