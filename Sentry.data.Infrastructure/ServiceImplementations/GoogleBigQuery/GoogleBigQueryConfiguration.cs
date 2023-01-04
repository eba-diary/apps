using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure
{
    public class GoogleBigQueryConfiguration
    {
        public string RelativeUri { get; set; }
        public string ProjectId { get; set; }
        public string DatasetId { get; set; }
        public string TableId { get; set; }
        public string PageToken { get; set; }
        public int LastIndex { get; set; }
        public int TotalRows { get; set; }
        public RetrieverJob Job { get; set; }
        public DataFlowStep S3DropStep { get; set; }
    }
}
