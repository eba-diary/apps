using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowStepDto
    {
        public int Id { get; set; }
        public int DataFlowId { get; set; }
        public string DataFlowName { get; set; }
        public DataActionType DataActionType { get; set; }
        public int DataActionTypeId { get; set; }
        public string DataActionName { get; set; }
        public int ActionId { get; set; }
        public string ActionName { get; set; }
        public int ExeuctionOrder { get; set; }
        public string TriggerKey { get; set; }
        public string TargetPrefix { get; set; }
    }
}
