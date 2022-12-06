using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Infrastructure
{
    public class PagingHttpsConfiguration
    {
        public HTTPSSource Source { get; set; }
        public DataFlowStep S3DropStep { get; set; }
        public string DataPath { get; set; }
        public string RequestUri { get; set; }
        public int PageNumber { get; set; }
        public string Filename { get; set; }
        public RetrieverJob Job { get; set; }
        public HttpsOptions Options { get; set; }
        public DataSourceToken Token { get; set; }
    }
}
