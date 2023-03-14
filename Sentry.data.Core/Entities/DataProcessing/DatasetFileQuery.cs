using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DatasetFileQuery : DatasetFileV2 
    {
        public virtual int DatasetFileQueryID { get; set; }

        public virtual int GetId()
        {
            return DatasetFileQueryID;
        }
    }
}
