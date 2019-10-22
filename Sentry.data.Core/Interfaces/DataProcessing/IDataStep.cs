using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataStep
    {
        int Id { get; set; }
        string TargetPrefix { get; set; }
        DataActionType DataAction_Type_Id { get; set; }
        DataFlow DataFlow { get; set; }
        BaseAction Action { get; set; }
        IList<DataFlow_Log> Executions { get; set; }
        void ProcessEvent(DataFlowStepEvent stepEvent, string flowExecutionGuid);
        //void GenerateStartEvent(string bucket, string key, string FlowExecutionGuid);
        DataFlow_Log LogExecution(string executionGuid, string runInstanceGuid, string log, Log_Level level, Exception ex = null);
    }
}
