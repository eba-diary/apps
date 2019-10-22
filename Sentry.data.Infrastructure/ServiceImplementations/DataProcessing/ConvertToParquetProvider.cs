using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;

namespace Sentry.data.Infrastructure
{
    public class ConvertToParquetProvider : IConvertToParquetProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public ConvertToParquetProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void ExecuteAction(IDataStep step, DataFlowStepEvent stepEvent)
        {
            throw new System.NotImplementedException();
        }

        public void PublishStartEvent(DataFlowStep step, string bucket, string key, string flowExecutionGuid, string runInstanceGuid)
        {
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);

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
                    EventType = GlobalConstants.DataFlowStepEvent.CONVERT_TO_PARQUET
                };

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"converttoparquetprovider-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"converttoparquetprovider-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);
            }            
        }
    }
}
