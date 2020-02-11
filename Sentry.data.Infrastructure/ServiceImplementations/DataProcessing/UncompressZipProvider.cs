using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Infrastructure
{
    //ActionType attribute utilized to map entity to provider within DataStepProvider.cs
    [ActionType(DataActionType.UncompressZip)]
    public class UncompressZipProvider : IUncompressZipProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;

        public UncompressZipProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
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

                //Copy file to Raw Storage
                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.TargetBucket, $"{stepEvent.TargetPrefix}{fileName}");

                stopWatch.Stop();
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
                                name = stepEvent.TargetBucket
                            },
                            Object = new Sentry.data.Core.Entities.S3.Object()
                            {
                                key = $"{stepEvent.TargetPrefix}{fileName}"
                            }
                        }
                    }
                };
                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
#endif

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
                    TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id}/" + $"{FlowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/",
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
