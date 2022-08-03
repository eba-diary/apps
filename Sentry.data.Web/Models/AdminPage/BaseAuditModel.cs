﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
namespace Sentry.data.Web
{
    public class BaseAuditModel
    {
        public int DataFlowStepId { get; set; }
        public List<AuditDataFileModel> DatafileModels { get; set; }
    }

    public class AuditDataFileModel
    {
        public string DatasetFileName { get; set; }
        public int DatasetFileId { get; set; }
    }

    public class CompareAuditDataFileModel : AuditDataFileModel
    {
        public int RawqueryRowCount { get; set; }
        public int ParquetRowCount { get; set; }
    }
}