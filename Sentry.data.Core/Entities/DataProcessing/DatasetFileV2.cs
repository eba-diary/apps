using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public abstract class DatasetFileV2 : BaseDatasetFileV2
    {
        public virtual int DatasetFileDrop { get; set; }
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual string RunInstanceGUID { get; set; }
        public virtual DateTime UpdateDTM { get; set; }
    }
}
