using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure
{
    public interface IDataFlowStepProvider
    {
        void GenerateStartEvent(DataFlowStep step, string bucket, string key);
    }
}
