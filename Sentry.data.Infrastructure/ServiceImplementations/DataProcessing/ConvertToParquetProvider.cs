using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using Sentry.data.Infrastructure.Helpers;
using StructureMap;
using System;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class ConvertToParquetProvider : BaseActionProvider, IConvertToParquetProvider
    {
        private readonly IMessagePublisher _messagePublisher;

        public ConvertToParquetProvider(IMessagePublisher messagePublisher, IDataFlowService dataFlowService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            throw new System.NotImplementedException();
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
                    ISchemaLoadProvider schemaLoadProvider = container.GetInstance<ISchemaLoadProvider>();
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
                    StepTargetPrefix = step.Action.TargetStoragePrefix + $"{storageCode}/{flowGuidDTM.Year.ToString()}/{flowGuidDTM.Month.ToString()}/{flowGuidDTM.Day.ToString()}/",
                    EventType = GlobalConstants.DataFlowStepEvent.CONVERT_TO_PARQUET_START,
                    FileSize = s3Event.s3.Object.size.ToString(),
                    S3EventTime = s3Event.eventTime.ToString("s"),
                    OriginalS3Event = JsonConvert.SerializeObject(s3Event)
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
