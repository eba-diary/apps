using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class QueryStorageProvider : IQueryStorageProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public QueryStorageProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void ExecuteAction(IDataStep step, DataFlowStepEvent stepEvent)
        {
            string fileName = Path.GetFileName(stepEvent.SourceKey);
            //Mock for testing... sent mock s3object created 
            S3Event s3e = null;
            s3e = new S3Event
            {
                EventType = "S3EVENT",
                PayLoad = new S3ObjectEvent()
                {
                    eventName = "ObjectCreated:Put",
                    s3 = new S3()
                    {
                        bucket = new Bucket()
                        {
                            name = stepEvent.TargetBucket
                        },
                        _object = new Sentry.data.Core.Entities.S3.Object()
                        {
                            key = $"{stepEvent.TargetPrefix}{fileName}"
                        }
                    }
                }
            };

            _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
        }

        public void PublishStartEvent(DataFlowStep step, string bucket, string key, string flowExecutionGuid, string runInstanceGuid)
        {
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <querystorageprovider-publishstartevent", Log_Level.Debug);

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
                    EventType = GlobalConstants.DataFlowStepEvent.QUERY_STORAGE
                };

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"querystorageprovider-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <querystorageprovider-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"querystorageprovider-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <querystorageprovider-publishstartevent", Log_Level.Debug);

            }
            
        }
    }
}
