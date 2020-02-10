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

namespace Sentry.data.Infrastructure
{
    public class QueryStorageProvider : IQueryStorageProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;

        public QueryStorageProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider)
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
                bool IsRegisterSuccessful = false;

                string targetFileName = Path.GetFileNameWithoutExtension(stepEvent.SourceKey) + "_" + _flowGuid + Path.GetExtension(stepEvent.SourceKey);
                string targetKey = stepEvent.TargetPrefix + targetFileName;

                //Copy file to Raw Storage
                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.TargetBucket, $"{targetKey}");


                //Pass step event to Schema to register new file
                //Register file with schema
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();
                    ISchemaService schemaSerivce = container.GetInstance<ISchemaService>();

                    FileSchema schema = datasetContext.GetById<FileSchema>(stepEvent.SchemaId);

                    IsRegisterSuccessful = schemaSerivce.RegisterRawFile(schema, targetKey, versionKey, stepEvent);
                }

                ////Mock for testing... sent mock s3object created 
                //S3Event s3e = null;
                //s3e = new S3Event
                //{
                //    EventType = "S3EVENT",
                //    PayLoad = new S3ObjectEvent()
                //    {
                //        eventName = "ObjectCreated:Put",
                //        s3 = new S3()
                //        {
                //            bucket = new Bucket()
                //            {
                //                name = stepEvent.TargetBucket
                //            },
                //            Object = new Sentry.data.Core.Entities.S3.Object()
                //            {
                //                key = $"{targetKey}"
                //            }
                //        }
                //    }
                //};

                //_messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));
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

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            _step = step;
            _flowGuid = flowExecutionGuid;
            _runInstGuid = runInstanceGuid;
            string storageCode;
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
                    ISchemaLoadProvider schemaLoadProvider = container.GetInstance<ISchemaLoadProvider>();
                    ISchemaService schemaService = container.GetInstance<ISchemaService>();
                    IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();

                    storageCode = schemaLoadProvider.GetStorageCodeFromKey(objectKey);
                    schema = schemaService.GetFileSchemaByStorageCode(storageCode);
                    _dataset = datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).FirstOrDefault().ParentDataset;
                }

                //Convert FlowExecutionGuid to DateTime
                DateTime flowGuidDTM = ConvertFlowGuidToDateTime();

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
                        TargetBucket = _step.Action.TargetStorageBucket,
                        //key structure /<storage prefix>/<storage code>/<YYYY>/<MM>/<DD>/<sourceFileName>_<FlowExecutionGuid>.<sourcefileextension>                    
                        TargetPrefix = _step.Action.TargetStoragePrefix + $"{storageCode}/{flowGuidDTM.Year.ToString()}/{flowGuidDTM.Month.ToString()}/{flowGuidDTM.Day.ToString()}/",
                        EventType = GlobalConstants.DataFlowStepEvent.QUERY_STORAGE,
                        FileSize = s3Event.s3.Object.size.ToString(),
                        S3EventTime = s3Event.eventTime.ToString("s"),
                        OriginalS3Event = JsonConvert.SerializeObject(s3Event),
                        SchemaId = schema.SchemaId,
                        DatasetID = _dataset.DatasetId
                    };

                    //GetIdsFromS3Key(stepEvent);

                
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
