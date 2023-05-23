using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Sentry.data.Core
{
    public static class Logging
    {

        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

    }
}
