﻿using Newtonsoft.Json;
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

namespace Sentry.data.Infrastructure
{
    public class QueryStorageProvider : BaseActionProvider, IQueryStorageProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;

        public QueryStorageProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            Stopwatch stopWatch = new Stopwatch();
            _step = step;
            _flowGuid = stepEvent.FlowExecutionGuid;
            _runInstGuid = stepEvent.RunInstanceGuid;

            //log standard start method statement
            MetricData.AddOrUpdateValue("log", $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
            step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

            try
            {
                stopWatch.Start();
                DateTime startTime = DateTime.Now;
                MetricData.AddOrUpdateValue("start_process_time", $"{startTime}");
                MetricData.AddOrUpdateValue("message_value", $"{JsonConvert.SerializeObject(stepEvent)}");
                MetricData.AddOrUpdateValue("s3_to_process_lag", $"{((int)(startTime.ToUniversalTime() - DateTime.Parse(stepEvent.S3EventTime).ToUniversalTime()).TotalMilliseconds)}");

                string fileName = Path.GetFileName(stepEvent.SourceKey);

                MetricData.AddOrUpdateValue("filename", fileName);
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}");
                step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

                /***************************************
                 *  Perform provider specific processing
                 ***************************************/
                bool IsRegisterSuccessful = false;


                string targetFileName = DataFlowHelpers.AddFlowExecutionGuidToFilename(Path.GetFileName(stepEvent.SourceKey), _flowGuid);
                string targetKey = stepEvent.StepTargetPrefix + targetFileName;

                //Copy file to RawQuery Storage
                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.StepTargetBucket, $"{targetKey}");

                //Pass step event to Schema to register new file
                //Register file with schema
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();
                    ISchemaService schemaSerivce = container.GetInstance<ISchemaService>();

                    FileSchema schema = datasetContext.GetById<FileSchema>(stepEvent.SchemaId);

                    IsRegisterSuccessful = schemaSerivce.RegisterRawFile(schema, targetKey, versionKey, stepEvent);
                }

                /***************************************
                 *  Trigger dependent data flow steps
                 ***************************************/

                foreach (DataFlowStepEventTarget target in stepEvent.DownstreamTargets)
                {
                    _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, target.BucketName, $"{target.ObjectKey}{targetFileName}");
#if (DEBUG)
                    //Mock for testing... sent mock s3object created 
                    S3Event s3e = null;
                    s3e = new S3Event
                    {
                        EventType = "S3EVENT",
                        PayLoad = new S3ObjectEvent()
                        {
                            eventName = "ObjectCreated:Put",
                            eventTime = DateTime.Now,
                            s3 = new S3()
                            {
                                bucket = new Bucket()
                                {
                                    name = target.BucketName
                                },
                                Object = new Sentry.data.Core.Entities.S3.Object()
                                {
                                    key = $"{target.ObjectKey}{targetFileName}",
                                    size = 200124
                                }
                            }
                        }
                    };
                    _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
#endif
                }

                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                //Set Standard metric data
                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "C");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-executeaction-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}");

                step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
                
                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
                step.LogExecution(stepEvent, MetricData, Log_Level.Debug);
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }

                //Set statndard metric data
                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "F");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-executeaction-failed");

                //Log standard metric data
                _step.Executions.Add(_step.LogExecution(stepEvent, MetricData, Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{_step.DataAction_Type_Id.ToString()}>-executeaction");
                _step.LogExecution(stepEvent, MetricData, Log_Level.Debug);
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<EventMetric> logs = new List<EventMetric>();
            Stopwatch stopWatch = new Stopwatch();
            _step = step;
            _flowGuid = flowExecutionGuid;
            _runInstGuid = runInstanceGuid;
            string schemaStorageCode;
            FileSchema schema;
            Dataset _dataset;
            string objectKey = s3Event.s3.Object.key;
            string keyBucket = s3Event.s3.bucket.name;

            try
            {
                stopWatch.Start();
                DateTime startTime = DateTime.Now;
                MetricData.AddOrUpdateValue("start_process_time", $"{DateTime.Now.ToString()}");
                MetricData.AddOrUpdateValue("s3_to_process_lag", $"{((int)(startTime.ToUniversalTime() - s3Event.eventTime.ToUniversalTime()).TotalMilliseconds)}");
                MetricData.AddOrUpdateValue("message_value", $"{JsonConvert.SerializeObject(s3Event)}");
                MetricData.AddOrUpdateValue("log", $"start-method <{_step.DataAction_Type_Id.ToString()}>-publishstartevent");
                _step.LogExecution(_flowGuid, _runInstGuid, MetricData, Log_Level.Debug);

                //Get StorageCode and FileSchema
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    ISchemaService schemaService = container.GetInstance<ISchemaService>();
                    IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();

                    schemaStorageCode = _dataFlowService.GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                    schema = schemaService.GetFileSchemaByStorageCode(schemaStorageCode);
                    _dataset = datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;
                }

                //Convert FlowExecutionGuid to DateTime, then to local time
                DateTime flowGuidDTM = DataFlowHelpers.ConvertFlowGuidToDateTime(_flowGuid).ToLocalTime();

                if (schema != null)
                {
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
                        //key structure /<storage prefix>/<storage code>/<YYYY>/<MM>/<DD>/<sourceFileName>_<FlowExecutionGuid>.<sourcefileextension>                    
                        StepTargetPrefix = _step.TargetPrefix + $"{flowGuidDTM.Year.ToString()}/{flowGuidDTM.Month.ToString()}/{flowGuidDTM.Day.ToString()}/",
                        EventType = GlobalConstants.DataFlowStepEvent.QUERY_STORAGE_START,
                        FileSize = s3Event.s3.Object.size.ToString(),
                        S3EventTime = s3Event.eventTime.ToString("o"),
                        OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                        SchemaId = schema.SchemaId,
                        DatasetID = _dataset.DatasetId
                    };

                    base.GenerateDependencyTargets(stepEvent);

                    MetricData.AddOrUpdateValue("log", $"{_step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}");
                    _step.LogExecution(_flowGuid, _runInstGuid, MetricData, Log_Level.Debug);

                    _messagePublisher.PublishDSCEvent($"{_step.DataFlow.Id}-{_step.Id}-{RandomString(12)}", JsonConvert.SerializeObject(stepEvent));

                    stopWatch.Stop();
                    DateTime endTime = DateTime.Now;

                    //Add metricdata values
                    MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                    MetricData.AddOrUpdateValue("status", "C");
                    MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{stopWatch.ElapsedMilliseconds}");

                    step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));

                    //Log end of method statement
                    MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                    step.LogExecution(stepEvent, MetricData, Log_Level.Debug);
                }
                else
                {
                    MetricData.AddOrUpdateValue("log", $"{_step.DataAction_Type_Id.ToString()}-publishstartevent-failed schema-not-found objectkey:{objectKey}");
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Error);
                }
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

                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
            }
        }
    }
}