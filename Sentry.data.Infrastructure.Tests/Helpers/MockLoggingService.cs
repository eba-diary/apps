using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Sentry.data.Infrastructure.Tests.Helpers
{
    public class MockLoggingService<T> : ILogger<T>
    {
        protected StringBuilder _stringBuilder = new StringBuilder();

        // get log messages if you need to inspect them
        public string GetLogMessages()
        {
            return _stringBuilder.ToString().Trim();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception ex, Func<TState, Exception, string> formatter)
        {
            // TState is a string in our case containing the log message
            var message = state.ToString();

            switch (logLevel)
            {
                case LogLevel.Debug:
                    _stringBuilder.AppendLine($"Debug {message}");
                    break;
                case LogLevel.Information:
                    _stringBuilder.AppendLine($"Info {message}");
                    break;
                case LogLevel.Warning:
                    if (ex == null)
                    {
                        _stringBuilder.AppendLine($"Warn {message}");
                    }
                    else
                    {
                        _stringBuilder.AppendLine($"Warn {message} ^^exception^^: {ex.Message}");
                    }
                    break;
                case LogLevel.Error:
                    if (ex == null)
                    {
                        _stringBuilder.AppendLine($"Error {message}");
                    }
                    else
                    {
                        _stringBuilder.AppendLine($"Error {message} ^^exception^^: {ex.Message}");
                    }
                    break;
                case LogLevel.Critical:
                    if (ex == null)
                    {
                        _stringBuilder.AppendLine($"Critical {message}");
                    }
                    else
                    {
                        _stringBuilder.AppendLine($"Critical {message} ^^exception^^: {ex.Message}");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
