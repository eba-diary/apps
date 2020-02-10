using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Infrastructure
{
    public class UncompressZipProvider : IUncompressZipProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public UncompressZipProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            //executed external to this application
            throw new NotImplementedException();
        }

        public void PublishStartEvent(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            try
            {
                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"start-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);
                string objectKey = s3Event.s3.Object.key;
                string keyBucket = s3Event.s3.bucket.name;

                DataFlowStepEvent stepEvent = new DataFlowStepEvent()
                {
                    DataFlowId = step.DataFlow.Id,
                    DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                    FlowExecutionGuid = FlowExecutionGuid,
                    RunInstanceGuid = runInstanceGuid,
                    StepId = step.Id,
                    ActionId = step.Action.Id,
                    ActionGuid = step.Action.ActionGuid.ToString(),
                    SourceBucket = keyBucket,
                    SourceKey = objectKey,
                    TargetBucket = step.Action.TargetStorageBucket,
                    TargetPrefix = step.Action.TargetStoragePrefix,
                    EventType = GlobalConstants.DataFlowStepEvent.UNCOMPRESS_ZIP,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                };

                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"uncompresszipprovider-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"end-method <uncompresszipprovider-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"uncompresszipprovider-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"end-method <uncompresszipprovider-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
