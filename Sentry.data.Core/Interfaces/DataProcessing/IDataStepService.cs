using Newtonsoft.Json.Linq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataStepService
    {
        void ExecuteStep(DataFlowStepEvent stepEvent);
        Task ExecuteStepAsync(DataFlowStepEvent stepEvent);
        void PublishStartEvent(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);
        Task PublishStartEventAsync(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);
    }
}
