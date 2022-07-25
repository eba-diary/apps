using System.Linq;
using Sentry.Common.Logging;
using System.Net.Mime;
using Sentry.data.Core;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public static class RestSharpExtensions
    {
        public static string ToLowerFirstChar(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}
