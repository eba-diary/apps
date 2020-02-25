using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Sentry.data.Infrastructure.Helpers;

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

            _step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{_step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);

            try
            {
                stopWatch.Start();
                /***************************************
                 *  Perform provider specific processing
                 ***************************************/
                bool IsRegisterSuccessful = false;

                string targetFileName = Path.GetFileNameWithoutExtension(stepEvent.SourceKey) + "_" + _flowGuid + Path.GetExtension(stepEvent.SourceKey);
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
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
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
                _step.LogExecution(_flowGuid, _runInstGuid, $"start-method <{_step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

                //Get StorageCode and FileSchema
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    ISchemaService schemaService = container.GetInstance<ISchemaService>();
                    IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();

                    schemaStorageCode = _dataFlowService.GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                    schema = schemaService.GetFileSchemaByStorageCode(schemaStorageCode);
                    _dataset = datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;
                }

                //Convert FlowExecutionGuid to DateTime
                DateTime flowGuidDTM = DataFlowHelpers.ConvertFlowGuidToDateTime(_flowGuid);

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
                        S3EventTime = s3Event.eventTime.ToString("s"),
                        OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                        SchemaId = schema.SchemaId,
                        DatasetID = _dataset.DatasetId
                    };

                    base.GenerateDependencyTargets(stepEvent);

                    _step.LogExecution(_flowGuid, _runInstGuid, $"{_step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                    _messagePublisher.PublishDSCEvent($"{_step.DataFlow.Id}-{_step.Id}", JsonConvert.SerializeObject(stepEvent));
                    stopWatch.Stop();

                    _step.LogExecution(_flowGuid, _runInstGuid, $"{_step.DataAction_Type_Id.ToString()}-publishstartevent-successful", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) });
                }
                else
                {
                    _step.LogExecution(_flowGuid, _runInstGuid, $"{_step.DataAction_Type_Id.ToString()}-publishstartevent-failed schema-not-found objectkey:{objectKey}", Log_Level.Error);
                }
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

        private string GetStorageCode(string key)
        {
            string storageCode = string.Empty;
            //temp-file/rawquery/<flowId>/<storagecode>/<guids>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX))
            {
                int strtIdx = ParsingHelpers.GetNthIndex(key, '/', 4);
                int endIdx = ParsingHelpers.GetNthIndex(key, '/', 5);
                storageCode = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            return storageCode;
        }

        private DateTime ConvertFlowGuidToDateTime()
        {
            CultureInfo provider = new CultureInfo(GlobalConstants.DataFlowGuidConfiguration.GUID_CULTURE);
            DateTime flowGuidDTM = DateTime.ParseExact(_flowGuid, GlobalConstants.DataFlowGuidConfiguration.GUID_FORMAT, provider);
            return flowGuidDTM;
        }

        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
