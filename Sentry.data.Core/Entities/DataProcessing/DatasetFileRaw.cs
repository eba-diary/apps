using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetFileRaw : DatasetFileV2
    {
        public virtual int DatasetFileRawID { get; set; }        

        public virtual int GetId()
        {
            return this.DatasetFileRawID;
        }
    }
}
