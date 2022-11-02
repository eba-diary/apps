using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DataFlowMetricSearchModel
    {
        public int DatasetId { get; set; }
        public int SchemaId { get; set; }
        public int[] DatasetFileIds { get; set; }
    }
}