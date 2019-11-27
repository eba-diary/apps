using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlowVersion
    {
        public int DataFlowVersion_ID { get; set; }
        public IList<BaseAction> Actions { get; set; }
    }
}
