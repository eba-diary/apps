using Sentry.data.Core.Interfaces.DataProcessing;
using System;

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
        public virtual string TargetStoragePrefix { get; set; }
        public virtual string TargetStorageBucket { get; set; }

        public virtual void ExecuteAction(IDataStep step, DataFlowStepEvent stepEvent, string ExecutionGuid)
        {
            _baseActionProvider.ExecuteAction(step, stepEvent);
        }
        public virtual void PublishStartEvent(DataFlowStep step, string bucket, string key, string FlowExecutionGuid, string runInstanceGuid)
        {
            _baseActionProvider.PublishStartEvent(step, bucket, key, FlowExecutionGuid, runInstanceGuid);
        }
    }
}
