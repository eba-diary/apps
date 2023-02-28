using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public abstract class BaseAction : IDataAction
    {
        protected IBaseActionProvider _baseActionProvider;

        public BaseAction()
        {
        }

        public BaseAction(IBaseActionProvider baseActionprovider)
        {
            _baseActionProvider = baseActionprovider;
        }

        public virtual int Id { get; set; }
        public virtual Guid ActionGuid { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string TargetStoragePrefix { get; set; }
        public virtual string TargetStorageBucket { get; set; }
        public virtual bool TargetStorageSchemaAware { get; set; }
        public virtual string TriggerPrefix { get; set; }


        public virtual void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent, string ExecutionGuid)
        {
            _baseActionProvider.ExecuteAction(step, stepEvent);
        }
        public virtual void PublishStartEvent(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            _baseActionProvider.PublishStartEvent(step, FlowExecutionGuid, runInstanceGuid, s3Event);
        }

        public virtual async Task PublishStartEventAsync(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            await _baseActionProvider.PublishStartEventAsync(step, FlowExecutionGuid, runInstanceGuid, s3Event).ConfigureAwait(false);
        }
    }
}
