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
    [ActionType(DataActionType.UncompressZip)]
    public class UncompressZipProvider : BaseActionProvider, IUncompressZipProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFeatures _featureFlags;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;

        public UncompressZipProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService, IDataFeatures dataFeatures) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _featureFlags = dataFeatures;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            if (!_featureFlags.Remove_Mock_Uncompress_Logic_CLA_759.GetValue())
            {
                Stopwatch stopWatch = new Stopwatch();
                _step = step;
                _flowGuid = stepEvent.FlowExecutionGuid;
                _runInstGuid = stepEvent.RunInstanceGuid;

                _step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{_step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);

                try
                {
                    stopWatch.Start();
                    string fileName = Path.GetFileName(stepEvent.SourceKey);

                    /***************************************
                        *  Perform provider specific processing
                        ***************************************/
                    // This step is performed by an external process


                    /***************************************
                        *  Trigger dependent data flow steps
                        ***************************************/
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

                    _step.Executions.Add(_step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-executeaction-success", Log_Level.Debug, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
                }
                catch (Exception ex)
                {
                    if (stopWatch.IsRunning)
                    {
                        stopWatch.Stop();
                    }
                    _step.Executions.Add(_step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-executeaction-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                    _step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{_step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
                }
            }
            else
            {
                Logger.Debug("uncompresszipprovider-executeaction-dotnet disabled-by-featureflag");
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
                string objectKey = s3Event.s3.Object.key;
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
                    StepTargetBucket = step.Action.TargetStorageBucket,
                    StepTargetPrefix = (step.TargetPrefix == null) ? null : step.TargetPrefix + $"{flowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/",
                    EventType = GlobalConstants.DataFlowStepEvent.UNCOMPRESS_ZIP_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                };

                base.GenerateDependencyTargets(stepEvent);

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                //_messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
                _messagePublisher.PublishDSCEvent(null, JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
