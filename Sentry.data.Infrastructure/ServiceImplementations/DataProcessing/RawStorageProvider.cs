using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class RawStorageProvider : IRawStorageProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public RawStorageProvider(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
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

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            try
            {
                DateTime startTime = DateTime.Now;
                logs.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug));
                string objectKey = s3Event.s3._object.key;
                string keyBucket = s3Event.s3.bucket.name;

                DataFlowStepEvent stepEvent = new DataFlowStepEvent()
                {
                    DataFlowId = step.DataFlow.Id,
                    DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                    FlowExecutionGuid = flowExecutionGuid,
                    RunInstanceGuid = runInstanceGuid,
                    StepId = step.Id,
                    ActionId = step.Action.Id,
                    ActionGuid = step.Action.ActionGuid.ToString(),
                    SourceBucket = keyBucket,
                    SourceKey = objectKey,
                    TargetBucket = step.Action.TargetStorageBucket,
                    TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id}/" + $"{flowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/",
                    EventType = GlobalConstants.DataFlowStepEvent.RAW_STORAGE_START,
                    FileSize = s3Event.s3._object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                };

                logs.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug));
                
                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
                DateTime endTime = DateTime.Now;

                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Info));                
            }
            catch (Exception ex)
            {
                logs.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent failed", Log_Level.Error, ex));
                logs.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug));

                foreach (var log in logs)
                {
                    step.Executions.Add(log);
                }
            }
        }
    }
}
