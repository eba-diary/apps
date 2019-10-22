using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SchemaLoadProvider : ISchemaLoadProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public SchemaLoadProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void ExecuteAction(IDataStep step, DataFlowStepEvent stepEvent)
        {
            throw new NotImplementedException();
        }

        public void PublishStartEvent(DataFlowStep step, string bucket, string key, string flowExecutionGuid, string runInstanceGuid)
        {
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <schemaloadprovider-publishstartevent", Log_Level.Debug);

                DataFlowStepEvent stepEvent = new DataFlowStepEvent()
                {
                    DataFlowId = step.DataFlow.Id,
                    DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                    ExecutionGuid = flowExecutionGuid,
                    StepId = step.Id,
                    ActionId = step.Action.Id,
                    ActionGuid = step.Action.ActionGuid.ToString(),
                    SourceBucket = bucket,
                    SourceKey = key,
                    TargetBucket = step.Action.TargetStorageBucket,
                    TargetPrefix = step.Action.TargetStoragePrefix,
                    EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD
                };

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"schemaloadprovider-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <schemaloadprovider-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"schemaloadprovider-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <schemaloadprovider-publishstartevent", Log_Level.Debug);
            }            
        }
    }
}
