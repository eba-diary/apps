using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class AuditDto
    {
        public int DatasetFileId { get; set; }
        public string DatasetFileName { get; set; }
    }

    public class CompareAuditDto : AuditDto
    {
        public int RawqueryRowCount { get; set; }
        public int ParquetRowCount { get; set; }
    }
}