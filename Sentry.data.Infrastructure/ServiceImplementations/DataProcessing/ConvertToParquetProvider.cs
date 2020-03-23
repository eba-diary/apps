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
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class ConvertToParquetProvider : BaseActionProvider, IConvertToParquetProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFeatures _featureFlags;

        public ConvertToParquetProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider, 
            IDataFlowService dataFlowService, IDataFeatures dataFeatures) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _featureFlags = dataFeatures;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            if (!_featureFlags.Remove_ConvertToParquet_Logic_CLA_747.GetValue())
            {
                List<DataFlow_Log> logs = new List<DataFlow_Log>();
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
                        _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
#endif
                    }

                    stopWatch.Stop();

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
            else
            {
                Logger.Debug("converttoparquetprovider-executeaction disabled-by-featureflag");
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            try
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);
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

                    storageCode = GetStorageCode(objectKey);
                    schema = schemaService.GetFileSchemaByStorageCode(storageCode);
                    _dataset = datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;
                }

                //Convert FlowExecutionGuid to DateTime
                DateTime flowGuidDTM = DataFlowHelpers.ConvertFlowGuidToDateTime(flowExecutionGuid);

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
                    StepTargetPrefix = step.TargetPrefix + $"{flowGuidDTM.Year.ToString()}/{flowGuidDTM.Month.ToString()}/{flowGuidDTM.Day.ToString()}/",
                    EventType = GlobalConstants.DataFlowStepEvent.CONVERT_TO_PARQUET_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                    DatasetID = _dataset.DatasetId,
                    SchemaId = schema.SchemaId
                };

                base.GenerateDependencyTargets(stepEvent);

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"converttoparquetprovider-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"converttoparquetprovider-publishstartevent failed", Log_Level.Error, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <converttoparquetprovider-publishstartevent", Log_Level.Debug);
            }
        }

        private string GetStorageCode(string key)
        {
            string storageCode = string.Empty;
            //temp-file/parquet/<flowId>/<storagecode>/<guids>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX))
            {
                int strtIdx = ParsingHelpers.GetNthIndex(key, '/', 4);
                int endIdx = ParsingHelpers.GetNthIndex(key, '/', 5);
                storageCode = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            return storageCode;
        }
    }
}
