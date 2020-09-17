using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using Sentry.data.Infrastructure.Helpers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class RawStorageProvider : BaseActionProvider, IRawStorageProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDatasetService _datasetService;
        private readonly IDataFlowService _dataFlowService;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;


        public RawStorageProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDatasetService datasetService, IDataFlowService dataFlowService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetService = datasetService;
            _dataFlowService = dataFlowService;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
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
                //Copy file to Raw Storage
                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.StepTargetBucket, $"{stepEvent.StepTargetPrefix}{fileName}");

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

                _step.Executions.Add(_step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-executeaction-success", Log_Level.Debug, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
            }
            catch(Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
                _step.Executions.Add(_step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-executeaction-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                _step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{_step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            _step = step;
            _flowGuid = flowExecutionGuid;
            _runInstGuid = runInstanceGuid;
            string objectKey = s3Event.s3.Object.key;
            string keyBucket = s3Event.s3.bucket.name;

            try
            {
                stopWatch.Start();
                _step.LogExecution(_flowGuid, _runInstGuid, $"start-method <{_step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

                //Convert FlowExecutionGuid to DateTime, then to local time
                DateTime flowGuidDTM = DataFlowHelpers.ConvertFlowGuidToDateTime(flowExecutionGuid).ToLocalTime();

                DataFlowStepEvent stepEvent = new DataFlowStepEvent()
                {
                    DataFlowId = _step.DataFlow.Id,
                    DataFlowGuid = _step.DataFlow.FlowGuid.ToString(),
                    FlowExecutionGuid = _flowGuid,
                    RunInstanceGuid = _runInstGuid,
                    StepId = _step.Id,
                    ActionId = _step.Action.Id,
                    ActionGuid = _step.Action.ActionGuid.ToString(),
                    SourceBucket = keyBucket,
                    SourceKey = objectKey,
                    StepTargetBucket = _step.Action.TargetStorageBucket,
                    StepTargetPrefix = _step.TargetPrefix + $"{flowGuidDTM.Year.ToString()}/{flowGuidDTM.Month.ToString()}/{flowGuidDTM.Day.ToString()}/{DataFlowHelpers.GenerateGuid(flowExecutionGuid, runInstanceGuid)}/",
                    EventType = GlobalConstants.DataFlowStepEvent.RAW_STORAGE_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                };

                base.GenerateDependencyTargets(stepEvent);

                _step.LogExecution(_flowGuid, _runInstGuid, $"{_step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                _messagePublisher.PublishDSCEvent($"{_step.DataFlow.Id}-{_step.Id}-{RandomString(6)}", JsonConvert.SerializeObject(stepEvent));
                //_messagePublisher.PublishDSCEvent(string.Empty, JsonConvert.SerializeObject(stepEvent));
                stopWatch.Stop();

                _step.LogExecution(_flowGuid, _runInstGuid, $"{_step.DataAction_Type_Id.ToString()}-publishstartevent-successful", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) });
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
                _step.Executions.Add(_step.LogExecution(_flowGuid, _runInstGuid, $"{_step.DataAction_Type_Id.ToString()}-publishstartevent-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                _step.LogExecution(_flowGuid, _runInstGuid, $"end-method <{_step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
