﻿using Newtonsoft.Json;
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
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class FixedWidthProvider : BaseActionProvider, IFixedWidthProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private readonly IDataFeatures _featureFlags;
        private DataFlowStep _step;

        public FixedWidthProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService, IDataFeatures dataFeatures) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _featureFlags = dataFeatures;
            _dataFlowService = dataFlowService;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            Stopwatch stopWatch = new Stopwatch();
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

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            _step = step;
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{_step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
                string objectKey = s3Event.s3.Object.key;
                string keyBucket = s3Event.s3.bucket.name;
                string storageCode;
                FileSchema schema;
                Dataset _dataset;

                //Get StorageCode and FileSchema
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    ISchemaService schemaService = container.GetInstance<ISchemaService>();
                    IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();

                    storageCode = _dataFlowService.GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                    schema = schemaService.GetFileSchemaByStorageCode(storageCode);
                    _dataset = datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;
                }

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
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                    SchemaId = schema.SchemaId,
                    DatasetID = _dataset.DatasetId
                };

                base.GenerateDependencyTargets(stepEvent);

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{_step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{_step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{_step.DataAction_Type_Id.ToString()} - publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{_step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
