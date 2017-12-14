using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.DatasetRetriever
{
    class RequestOptions
    {
        public Boolean IsCompressed { get; set; }
        public CompressionTypes? CompressionFormat { get; set; }
        public int DataConfigId { get; set; }
        public List<CompressedFileRule> CompressedFileRules { get; set; }

    }
}
