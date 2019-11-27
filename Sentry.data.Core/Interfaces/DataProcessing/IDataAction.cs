using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataAction
    {
        void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent, string ExecutionGuid);
        void PublishStartEvent(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);
    }
}
