using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    [Serializable]
    public class ReportMetadata
    {
        public virtual string Location { get; set; }
        public virtual string LocationType { get; set; }
        public virtual int Frequency { get; set; }
        public bool GetLatest { get; set; }
    }
}
