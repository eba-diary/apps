using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowMetricSearchDto
    {
        public int DatasetId { get; set; }
        public int SchemaId { get; set; }
        public int DatasetFileId { get; set; }
    }
}
