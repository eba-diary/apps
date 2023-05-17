using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Helpers;
using Sentry.data.Core.Interfaces.DataProcessing;
using Sentry.data.Infrastructure.Helpers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ConvertToParquetProvider : BaseActionProvider, IConvertToParquetProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFeatures _featureFlags;
        private readonly Lazy<IDatasetContext> _datasetContext;
        private readonly Lazy<ISchemaService> _schemaService;
        private JObject _metricData;

        public JObject MetricData
        {
            get
            {
                if (_metricData == null)
                {
                    _metricData = new JObject();
                    return _metricData;
                }

                return _metricData;
            }
            set { _metricData = value; }
        }

        public ConvertToParquetProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider, 
            IDataFlowService dataFlowService, IDataFeatures dataFeatures, Lazy<IDatasetContext> datasetContext,
            Lazy<ISchemaService> schemaService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _featureFlags = dataFeatures;
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
            MetricData.AddOrUpdateValue("log", $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
            step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

            if (!_featureFlags.Remove_ConvertToParquet_Logic_CLA_747.GetValue())
            {
                List<EventMetric> logs = new List<EventMetric>();
                Stopwatch stopWatch = new Stopwatch();
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug));
                try
                {
                    stopWatch.Start();
                    string fileName = Path.GetFileName(stepEvent.SourceKey);
                    logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug));
                    /***************************************
                     *  Perform provider specific processing
                     ***************************************/
                    //This step does not perform any processing

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
                                        key = $"{target.ObjectKey}{fileName}",
                                        size = 200124
                                    }
                                }
                            }
                        };
                        await _messagePublisher.PublishDSCEventAsync("99999", JsonConvert.SerializeObject(s3e)).ConfigureAwait(false);
#endif
                    }

                    stopWatch.Stop();

                    step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-successful", Log_Level.Info, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, null));
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
            else
            {
                MetricData.AddOrUpdateValue("log", "converttoparquetprovider-executeaction-dotnet disabled-by-featureflag");
                step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
                step.LogExecution(stepEvent, MetricData, Log_Level.Debug);
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            throw new NotImplementedException();
        }

        public override async Task PublishStartEventAsync(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            Stopwatch stopWatch = new Stopwatch();

            try
            {
                stopWatch.Start();
                DateTime startTime = DateTime.Now;
                MetricData.AddOrUpdateValue("start_process_time", $"{DateTime.Now.ToString()}");
                MetricData.AddOrUpdateValue("s3_to_process_lag", $"{((int)(startTime.ToUniversalTime() - s3Event.eventTime.ToUniversalTime()).TotalMilliseconds)}");
                MetricData.AddOrUpdateValue("message_value", $"{JsonConvert.SerializeObject(s3Event)}");
                MetricData.AddOrUpdateValue("log", $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);

                string objectKey = s3Event.s3.Object.key;
                string keyBucket = s3Event.s3.bucket.name;
                string storageCode;
                FileSchema schema;
                Dataset _dataset;

                //Get StorageCode and FileSchema
                storageCode = GetStorageCode(objectKey);
                schema = SchemaService.GetFileSchemaByStorageCode(storageCode);
                _dataset = DatasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;
                
                //Convert FlowExecutionGuid to DateTime, then to local time
                DateTime flowGuidDTM = DataFlowHelpers.ConvertFlowGuidToDateTime(flowExecutionGuid).ToLocalTime();

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
                    StepTargetBucket = step.TargetBucket,
                    StepTargetPrefix = step.TargetPrefix + $"{flowGuidDTM.Year.ToString()}/{flowGuidDTM.Month.ToString()}/{flowGuidDTM.Day.ToString()}/",
                    EventType = GlobalConstants.DataFlowStepEvent.CONVERT_TO_PARQUET_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("o"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                    DatasetID = _dataset.DatasetId,
                    SchemaId = schema.SchemaId
                };

                base.GenerateDependencyTargets(stepEvent);

                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);

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

        private string GetStorageCode(string key)
        {
            string storageCode = string.Empty;
            //temp-file/parquet/<flowId>/<storagecode>/<guids>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX))
            {
                int strtIdx = Sentry.data.Infrastructure.Helpers.ParsingHelpers.GetNthIndex(key, '/', 4);
                int endIdx = Sentry.data.Infrastructure.Helpers.ParsingHelpers.GetNthIndex(key, '/', 5);
                storageCode = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            return storageCode;
        }
    }
}
