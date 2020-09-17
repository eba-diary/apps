using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using Sentry.data.Core.Entities.S3;
using System.IO;
using StructureMap;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sentry.data.Infrastructure
{
    //ActionType attribute utilized to map entity to provider within DataStepProvider.cs
    [ActionType(DataActionType.S3Drop)]
    public class S3DropProvider : BaseActionProvider, IS3DropProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDatasetContext _datasetContext;

        public S3DropProvider(IMessagePublisher messagePublisher, 
            IS3ServiceProvider s3ServiceProvider, IDatasetContext datasetContext,
            IDataFlowService dataFlowService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetContext = datasetContext;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            try
            {
                

                stopWatch.Start();
                DateTime startTime = DateTime.Now;
                string fileName = Path.GetFileName(stepEvent.SourceKey);
                step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                /***************************************
                 *  Perform provider specific processing
                 ***************************************/
                //S3 drop steps do not store the output in a long term storage area, all files are immediately sent to the raw storage step
                

                /***************************************
                 *  Trigger dependent data flow steps
                 ***************************************/
                //Copy file to all dependent targets for further processing
                foreach (DataFlowStepEventTarget target in stepEvent.DownstreamTargets)
                {
                    _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, target.BucketName, $"{target.ObjectKey}{fileName}");

#if (DEBUG)
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
                                    name = target.BucketName
                                },
                                Object = new Sentry.data.Core.Entities.S3.Object()
                                {
                                    key = $"{target.ObjectKey}{fileName}"
                                }
                            }
                        }
                    };
                    _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
#endif
                }

                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}",Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            string objectKey = s3Event.s3.Object.key;
            string keyBucket = s3Event.s3.bucket.name;
            try
            {
                stopWatch.Start();
                DateTime startTime = DateTime.Now;
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

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
                    StepTargetBucket = step.Action.TargetStorageBucket,
                    //add run instance (separated by dash) if not null
                    StepTargetPrefix = (step.TargetPrefix == null) ? null : step.TargetPrefix + $"{flowExecutionGuid}{((runInstanceGuid == null) ? string.Empty : "-" + runInstanceGuid)}/",
                    EventType = GlobalConstants.DataFlowStepEvent.S3_DROP_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                };

                base.GenerateDependencyTargets(stepEvent);

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                //_messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
                _messagePublisher.PublishDSCEvent(string.Empty, JsonConvert.SerializeObject(stepEvent));
                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) });
            }
            catch (Exception ex)
            {
                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
