using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlowStep
    {
        public int Id { get; set; }
        public DataFlow DataFlow { get; set; }
        public DataActionType DataAction_Type_Id { get; set; }
        public BaseAction Action { get; set; }
        public int ExeuctionOrder { get; set; }
        //public IList<ActionExecution> Executions { get; set; }
    }
}
