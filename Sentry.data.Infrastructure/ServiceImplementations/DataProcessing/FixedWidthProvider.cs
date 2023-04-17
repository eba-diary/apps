using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Helpers;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class FixedWidthProvider : BaseActionProvider, IFixedWidthProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private readonly Lazy<IDatasetContext> _datasetContext;
        private readonly Lazy<ISchemaService> _schemaService;
        private DataFlowStep _step;

        public FixedWidthProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService, Lazy<IDatasetContext> datasetContext,
            Lazy<ISchemaService> schemaService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
            _datasetContext = datasetContext;
            _schemaService = schemaService;
        }

        public IDatasetContext DatasetContext
        {
            get { return _datasetContext.Value; }
        }
        public ISchemaService SchemaService
        {
            get { return _schemaService.Value; }
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            throw new NotImplementedException();
        }

        public override async Task ExecuteActionAsync(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            Stopwatch stopWatch = new Stopwatch();

            //log standard start method statement
            MetricData.AddOrUpdateValue("log", $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
            step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

#if (DEBUG)
            _step = step;

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
                    await _messagePublisher.PublishDSCEventAsync("99999", JsonConvert.SerializeObject(s3e)).ConfigureAwait(false);

                }

                stopWatch.Stop();

                _step.Executions.Add(_step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-executeaction-success", Log_Level.Debug, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
                _step.Executions.Add(_step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-executeaction-failed", Log_Level.Error, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                _step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{_step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            }
#endif
#if (!DEBUG)
            //Log end of method statement
            MetricData.AddOrUpdateValue("log", $"<{step.DataAction_Type_Id.ToString()}>-executeaction is processed by another service");
            step.LogExecution(stepEvent, MetricData, Log_Level.Debug);
#endif
            //Log end of method statement
            MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
            step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            throw new NotImplementedException();
        }

        public override async Task PublishStartEventAsync(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            Stopwatch stopWatch = new Stopwatch();
            _step = step;

            try
            {
                stopWatch.Start();
                DateTime startTime = DateTime.Now;
                MetricData.AddOrUpdateValue("start_process_time", $"{DateTime.Now.ToString()}");
                MetricData.AddOrUpdateValue("s3_to_process_lag", $"{((int)(startTime.ToUniversalTime() - s3Event.eventTime.ToUniversalTime()).TotalMilliseconds)}");
                MetricData.AddOrUpdateValue("message_value", $"{JsonConvert.SerializeObject(s3Event)}");
                MetricData.AddOrUpdateValue("log", $"start-method <{_step.DataAction_Type_Id.ToString()}>-publishstartevent");
                _step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);

                string objectKey = s3Event.s3.Object.key;
                string keyBucket = s3Event.s3.bucket.name;
                string storageCode;
                FileSchema schema;
                Dataset _dataset;

                //Get StorageCode and FileSchema
                storageCode = _dataFlowService.GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                schema = SchemaService.GetFileSchemaByStorageCode(storageCode);
                _dataset = DatasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;

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
                    StepTargetBucket = null,
                    StepTargetPrefix = null, //This step does push data to long term storage, only pushes result to next steps
                    EventType = GlobalConstants.DataFlowStepEvent.FIXEDWIDTH_PREPROCESSING_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("o"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                    SchemaId = schema.SchemaId,
                    DatasetID = _dataset.DatasetId
                };

                base.GenerateDependencyTargets(stepEvent);

                MetricData.AddOrUpdateValue("log", $"{_step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}");
                _step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);

                await _messagePublisher.PublishDSCEventAsync($"{step.DataFlow.Id}-{step.Id}-{RandomString(6)}", JsonConvert.SerializeObject(stepEvent)).ConfigureAwait(false);

                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                //Add metricdata values
                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "C");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{stopWatch.ElapsedMilliseconds}");
                step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Info, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                step.LogExecution(stepEvent, MetricData, Log_Level.Debug);
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }

                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "F");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-publishstartevent-failed");

                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Error, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
            }
        }
    }
}
