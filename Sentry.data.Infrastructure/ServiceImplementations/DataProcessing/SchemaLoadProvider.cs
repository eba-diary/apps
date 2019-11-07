using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class SchemaLoadProvider : ISchemaLoadProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private ISchemaService _schemaService;

        public SchemaLoadProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider, ISchemaService schemaService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _schemaService = schemaService;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug));
            try
            {
                DateTime startTime = DateTime.Now;
                string fileName = Path.GetFileName(stepEvent.SourceKey);
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug));

                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.TargetBucket, $"{stepEvent.TargetPrefix}{fileName}");
                DateTime endTime = DateTime.Now;

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

                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Info));
                logs = null;
            }
            catch (Exception ex)
            {
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction failed", Log_Level.Error, ex));
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug));
                foreach (var log in logs)
                {
                    step.Executions.Add(log);
                }
            }
        }

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <schemaloadprovider-publishstartevent", Log_Level.Debug);
                string objectKey = s3Event.s3._object.key;
                string keyBucket = s3Event.s3.bucket.name;
                SchemaMap mapping = step.SchemaMappings.FirstOrDefault();

                DateTime loadDate = DateTime.Now;

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
                    //<targetstorageprefix>/<dataflowid>/<storagecode>/<flow execution guid>[-<run instance guid>]/
                    TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id}/{mapping.MappedSchema.StorageCode}/{GenerateGuid(flowExecutionGuid, runInstanceGuid)}/ ",
                    EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD,
                    FileSize = s3Event.s3._object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                };

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"schemaloadprovider-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <schemaloadprovider-publishstartevent>", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"schemaloadprovider-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <schemaloadprovider-publishstartevent>", Log_Level.Debug);
            }            
        }

        private string GenerateGuid(string executionGuid, string instanceGuid)
        {
            if (instanceGuid == null)
            {
                return executionGuid;
            }
            else
            {
                return executionGuid + "-" + instanceGuid;
            }            
        }
    }
}
