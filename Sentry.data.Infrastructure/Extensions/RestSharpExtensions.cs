using System.Linq;
using RestSharp;
using Sentry.Common.Logging;
using System.Net.Mime;
using Sentry.data.Core;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public static class RestSharpExtensions
    {
        public static string ParseContentType(this IRestResponse resp)
        {
            string contentType = resp.ContentType;

            //Mime types
            //https://technet.microsoft.com/en-us/library/cc995276.aspx
            //https://www.iana.org/assignments/media-types/media-types.xhtml

            Logger.Info($"incoming_contenttype - {contentType}");

            var content = new ContentType(contentType);

            using (IContainer Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _datasetContext = Container.GetInstance<IDatasetContext>();

                MediaTypeExtension extensions = _datasetContext.MediaTypeExtensions.Where(w => w.Key == content.MediaType).FirstOrDefault();

                if (extensions == null)
                {
                    Logger.Warn($"Detected new MediaType ({content.MediaType}), defaulting to txt");
                    return "txt";
                }

                Logger.Info($"detected_mediatype - {extensions.Value}");
                return extensions.Value;
            }
        }

        public static string ToLowerFirstChar(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}
