using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;

namespace Sentry.data.Infrastructure
{
    public class S3DropProvider : IDataFlowStepProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public S3DropProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void GenerateStartEvent(DataFlowStep step, string bucket, string key, string FlowExecutionGuid)
        {
            //To support concurrent file processing, we add epoch time (acting as guid) to target prefix
            int Epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            DataFlowStepEvent stepEvent = new DataFlowStepEvent()
            {
                DataFlowId = step.DataFlow.Id,
                DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                ExecutionGuid = Epoch.ToString(),
                ActionId = step.Action.Id,
                ActionGuid = step.Action.ActionGuid.ToString(),
                SourceBucket = bucket,
                SourceKey = key,
                TargetBucket = step.Action.TargetStorageBucket,
                TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id.ToString()}/" + $"{Epoch.ToString()}/",
                EventType = GlobalConstants.DataFlowStepEvent.S3_DROP_START
            };
            
            _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));


        }
    }
}
