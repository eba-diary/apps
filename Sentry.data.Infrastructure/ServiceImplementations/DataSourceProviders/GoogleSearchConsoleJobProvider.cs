using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GoogleSearchConsoleJobProvider : PagingHttpsJobProvider
    {
        public GoogleSearchConsoleJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator, IFileProvider fileProvider) : base(datasetContext, s3ServiceProvider, authorizationProvider, httpClientGenerator, fileProvider)
        {
        }

        protected override string GetDataPath(RetrieverJob job)
        {
            return "rows";
        }

        protected override Task WriteToFileAsync(Stream contentStream, Stream fileStream, JToken data, PagingHttpsConfiguration config)
        {
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, 1024, true))
            {
                if (fileStream.Length == 0)
                {
                    JObject rawBody = JObject.Parse(config.Options.Body);
                    writer.WriteLine("{");
                    writer.WriteLine($"\t\"request\": {rawBody.ToString(Formatting.None)},");
                    writer.WriteLine("\t\"data\": [");
                }

                foreach (JToken row in data)
                {
                    writer.WriteLine("\t\t" + row.ToString(Formatting.None) + ",");
                }
            }

            return Task.CompletedTask;
        }

        protected override void EndFile(Stream fileStream)
        {
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, 1024, true))
            {
                writer.WriteLine("\t]");
                writer.WriteLine("}");
            }
        }
    }
}
