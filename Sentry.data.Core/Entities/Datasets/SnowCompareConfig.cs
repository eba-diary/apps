using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SnowCompareConfig
    {
        public string SourceDb { get; set; }
        public string TargetDb { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string QueryParameter { get; set; }
        public AuditSearchType AuditSearchType { get; set; }
    }
}