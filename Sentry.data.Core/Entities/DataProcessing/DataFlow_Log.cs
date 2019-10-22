using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlow_Log
    {
        public virtual int Log_Id { get; set; }
        public virtual DataFlow DataFlow { get; set; }
        public virtual string Log_Entry { get; set; }
        public virtual string FlowExecutionGuid { get; set; }
        public virtual string RunInstanceGuid { get; set; }
        public virtual DataFlowStep Step { get; set; }
        public virtual Log_Level Level { get; set; }
        public virtual string Machine_Name { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
    }
}
