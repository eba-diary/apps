using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowMetricSearchDto
    {
        public string DatasetToSearch { get; set; }
        public string SchemaToSearch { get; set; }
        public string FileToSearch { get; set; }
    }
}
