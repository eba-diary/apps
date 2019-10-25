using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataAction
    {
        void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent, string ExecutionGuid);
        void PublishStartEvent(DataFlowStep step, string bucket, string key, string FlowExecutionGuid, string runInstanceGuid);
    }
}
