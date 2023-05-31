using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GoogleSearchConsoleJobProvider : PagingHttpsJobProvider
    {
        private UTF8Encoding UTF8NoBOM;

        public GoogleSearchConsoleJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator, IFileProvider fileProvider, IDataFeatures featureFlags) : base(datasetContext, s3ServiceProvider, authorizationProvider, httpClientGenerator, fileProvider)
        {
        }

        protected override string GetDataPathRegexPattern(RetrieverJob job)
        {
            return "rows";
        }

        protected override Task WriteToFileAsync(Stream contentStream, Stream fileStream, PagingHttpsConfiguration config)
        {
            using (StreamWriter writer = new StreamWriter(fileStream, GetEncoding(), 1024, true))
            {
                if (fileStream.Length == 0)
                {
                    JObject rawBody = JObject.Parse(config.Options.Body);
                    writer.WriteLine("{");
                    writer.WriteLine($"\"request\": {rawBody.ToString(Formatting.None)},");
                    writer.WriteLine("\"data\": [");
                }

                contentStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader streamReader = GetStreamReader(contentStream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject && config.DataPathRegex.IsMatch(jsonReader.Path))
                        {
                            writer.WriteLine(JToken.Load(jsonReader).ToString(Formatting.None) + ",");
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected override void EndFile(Stream fileStream)
        {
            using (StreamWriter writer = new StreamWriter(fileStream, GetEncoding(), 1024, true))
            {
                writer.WriteLine("]");
                writer.WriteLine("}");
            }
        }

        private UTF8Encoding GetEncoding()
        {
            //required to prevent byte order marks from being included on the file which causes issues on the processing platform side
            if (UTF8NoBOM == null)
            {
                UTF8NoBOM = new UTF8Encoding(false);
            }

            return UTF8NoBOM;
        }
    }
}
