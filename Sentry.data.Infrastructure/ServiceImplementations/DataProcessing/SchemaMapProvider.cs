using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Sentry.data.Infrastructure
{
    //ActionType attribute utilized to map entity to provider within DataStepProvider.cs
    [ActionType(DataActionType.SchemaMap)]
    public class SchemaMapProvider : ISchemaMapProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;


        public SchemaMapProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider, IDataFlowService dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug));
            try
            {
                DateTime startTime = DateTime.Now;
                stopWatch.Start();
                string fileName = Path.GetFileName(stepEvent.SourceKey);
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug));

                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.TargetBucket, $"{stepEvent.TargetPrefix}{fileName}");
                DateTime endTime = DateTime.Now;
                stopWatch.Stop();

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
                                key = $"{stepEvent.TargetPrefix}{fileName}",
                                size = 200124
                            }
                        }
                    }
                };

                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));

                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-successful", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, null));
                logs = null;
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-failed", Log_Level.Error, ex));
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug));
                foreach (var log in logs)
                {
                    step.Executions.Add(log);
                }
            }
        }

        public void PublishStartEvent(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            string objectKey = s3Event.s3._object.key;
            string keyBucket = s3Event.s3.bucket.name;
            try
            {
                foreach (SchemaMap scmMap in step.SchemaMappings)
                {
                    stopWatch.Start();
                    DateTime startTime = DateTime.Now;

                    DataFlowStep s3DropStep = _dataFlowService.GetS3DropByFileSchema(scmMap.MappedSchema);

                    string targetSchemaS3DropPrefix = s3DropStep.TargetPrefix;

                    step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

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
                        //add run instance (separated by dash) if not null
                        TargetPrefix = targetSchemaS3DropPrefix + $"{s3DropStep.DataFlow.Id.ToString()}/" + $"{FlowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/",
                        EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_MAP,
                        FileSize = s3Event.s3._object.size.ToString(),
                        S3EventTime = s3Event.eventTime.ToString("s"),
                        OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                    };

                    step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                    _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
                    stopWatch.Stop();
                    DateTime endTime = DateTime.Now;

                    step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) });
                }                
            }
            catch (Exception ex)
            {
                step.Executions.Add(step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
