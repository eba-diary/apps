using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SchemaLoadProvider : IDataFlowStepProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public SchemaLoadProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void GenerateStartEvent(DataFlowStep step, string bucket, string key)
        {
            DataFlowStepEvent stepEvent = new DataFlowStepEvent()
            {
                DataFlowId = step.DataFlow.Id,
                DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                ActionId = step.Action.Id,
                ActionGuid = step.Action.ActionGuid.ToString(),
                SourceBucket = bucket,
                SourceKey = key,
                TargetBucket = step.Action.TargetStorageBucket,
                TargetPrefix = step.Action.TargetStoragePrefix,
                EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD
            };

            _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
        }
    }
}
