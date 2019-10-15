using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlowStep : IDataStep
    {
        public virtual int Id { get; set; }
        public virtual DataFlow DataFlow { get; set; }
        public virtual DataActionType DataAction_Type_Id { get; set; }
        public virtual BaseAction Action { get; set; }
        public virtual int ExeuctionOrder { get; set; }
        public virtual string TriggerKey { get; set; }
        public virtual string TargetPrefix { get; set; }
        //public IList<ActionExecution> Executions { get; set; }
    }
}
