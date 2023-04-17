using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Helpers;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    //ActionType attribute utilized to map entity to provider within DataStepProvider.cs
    [ActionType(DataActionType.SchemaMap)]
    public class SchemaMapProvider : BaseActionProvider, ISchemaMapProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private readonly Lazy<IDatasetContext> _datasetContext;

        public SchemaMapProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService, Lazy<IDatasetContext> datasetContext) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
            _datasetContext = datasetContext;
        }

        public IDatasetContext DatasetContext
        {
            get { return _datasetContext.Value; }
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            throw new NotImplementedException();
        }

        public override async Task ExecuteActionAsync(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            List<EventMetric> logs = new List<EventMetric>();
            Stopwatch stopWatch = new Stopwatch();

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
                //logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug));

                /***************************************
                 *  Perform provider specific processing
                 ***************************************/
                //This step does not perform any processing, it only copies file to the target schema specific data flow s3 drop step location


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
                            eventTime = DateTime.Now,
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
                    await _messagePublisher.PublishDSCEventAsync("99999", JsonConvert.SerializeObject(s3e));
#endif
                }

                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                //Set Standard metric data
                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "C");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-executeaction-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}");

                //Log standard metric data
                step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Info, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction");
                step.LogExecution(stepEvent, MetricData, Log_Level.Debug);

            }
            catch (Exception ex)
            {
                //Stop stopwatch if running
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }

                //Set statndard metric data
                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "F");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-executeaction-failed");

                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-failed", Log_Level.Error, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            }
        }

        public override void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            throw new NotImplementedException();
        }

        public override async Task PublishStartEventAsync(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            Stopwatch stopWatch = new Stopwatch();
            string objectKey = s3Event.s3.Object.key;
            string keyBucket = s3Event.s3.bucket.name;
            DateTime stepStartTime = DateTime.Now;

            
            try
            {
                MetricData.AddOrUpdateValue("start_process_time", $"{stepStartTime}");
                MetricData.AddOrUpdateValue("s3_to_process_lag", $"{((int)(stepStartTime.ToUniversalTime() - s3Event.eventTime.ToUniversalTime()).TotalMilliseconds)}");
                MetricData.AddOrUpdateValue("message_value", $"{JsonConvert.SerializeObject(s3Event)}");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

                foreach (SchemaMap scmMap in step.SchemaMappings)
                {
                    stopWatch.Start();
                    DateTime startTime = DateTime.Now;


                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);
                    /**********************************
                     * Determine if incoming file matches search criteria for schema mapping
                     * if the search criteria is null, then process file
                     **********************************/
                    string fileName = Path.GetFileName(objectKey);
                    bool processFile = (scmMap.SearchCriteria == null) ? true : Regex.IsMatch(fileName, scmMap.SearchCriteria);

                    if (processFile)
                    {
                        /*****************************************************
                        *  Find the S3 Drop step for the given schema mapping
                        ******************************************************/
                        DataFlowStep s3DropStep;
                        string targetSchemaS3DropPrefix;
                        string targetSchemaS3Bucket;

                        string schemaFlowName = _dataFlowService.GetDataFlowNameForFileSchema(scmMap.MappedSchema);
                        DataFlow flow = DatasetContext.DataFlow.Where(w => w.Name == schemaFlowName).FirstOrDefault();
                        s3DropStep = DatasetContext.DataFlowStep.Where(w => w.DataFlow == flow && w.DataAction_Type_Id == DataActionType.RawStorage).FirstOrDefault();

                        targetSchemaS3DropPrefix = s3DropStep.TriggerKey;
                        targetSchemaS3Bucket = s3DropStep.TriggerBucket;

                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

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
                            //add run instance (separated by dash) if not null
                            StepTargetPrefix = targetSchemaS3DropPrefix + $"{flowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/",
                            DownstreamTargets = new List<DataFlowStepEventTarget>() { new DataFlowStepEventTarget() { BucketName = targetSchemaS3Bucket, ObjectKey = targetSchemaS3DropPrefix + $"{flowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/" } },
                            EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_MAP_START,
                            FileSize = s3Event.s3.Object.size.ToString(),
                            S3EventTime = s3Event.eventTime.ToString("o"),
                            OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                        };

                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                        await _messagePublisher.PublishDSCEventAsync($"{step.DataFlow.Id}-{step.Id}-{RandomString(6)}", JsonConvert.SerializeObject(stepEvent)).ConfigureAwait(false);
                        //_messagePublisher.PublishDSCEvent(string.Empty, JsonConvert.SerializeObject(stepEvent));

                        stopWatch.Stop();
                        DateTime endTime = DateTime.Now;

                        MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                        MetricData.AddOrUpdateValue("status", "C");
                        MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{stopWatch.ElapsedMilliseconds}");
                        
                        step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Info, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
                        
                        //Log end of method statement
                        MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
                    }
                    else
                    {
                        /*****************************************************
                        *  Log message stating no match
                        *  Object cannot be deleted since it may be processed by another schema mapping
                        ******************************************************/
                        stopWatch.Stop();
                        DateTime endTime = DateTime.Now;

                        //Add metricdata values
                        MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                        MetricData.AddOrUpdateValue("status", "C");
                        MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-searchcriteria-nomatch bucket:{keyBucket} file:{objectKey} searchcriteria:{scmMap.SearchCriteria}");
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Info);

                        //Log end of method statement
                        MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
                    }
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

                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Error, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
            }
        }
    }
}
