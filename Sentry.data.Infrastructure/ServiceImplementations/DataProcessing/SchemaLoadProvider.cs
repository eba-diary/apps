using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Helpers;
using Sentry.data.Core.Interfaces.DataProcessing;
using Sentry.data.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SchemaLoadProvider : BaseActionProvider, ISchemaLoadProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;

        public SchemaLoadProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
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

                step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Info, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));
                
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
                step.Executions.Add(step.LogExecution(stepEvent, MetricData, Log_Level.Error, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));

                //Log end of method statement
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

                //Evaluate incoming file matches search criteria of any SchemaMappings for Data Step
                List<SchemaMap> validMappings = GetMatchingSchemaMappings(Path.GetFileName(objectKey), step.SchemaMappings);
                
                if (validMappings.Any())
                {
                    MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()} schemamapping-match {validMappings.Count} matching schema maps for {objectKey}");
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
                }
                else
                {
                    MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()} schemamapping-notdetected file will not be processed - {objectKey}");
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Warning);
                }

                foreach (SchemaMap item in validMappings)
                {
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
                        //<targetstorageprefix>/<dataflowid>/<storagecode>/<flow execution guid>[-<run instance guid>]/
                        StepTargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.FlowStorageCode}/{item.MappedSchema.StorageCode}/{DataFlowHelpers.GenerateGuid(flowExecutionGuid, runInstanceGuid)}/",
                        EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD_START,
                        FileSize = s3Event.s3.Object.size.ToString(),
                        S3EventTime = s3Event.eventTime.ToString("o"),
                        OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                    };
                    
                    base.GenerateDependencyTargets(stepEvent);

                    MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}");
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Info);

                    await _messagePublisher.PublishDSCEventAsync($"{step.DataFlow.Id}-{step.Id}-{RandomString(6)}", JsonConvert.SerializeObject(stepEvent)).ConfigureAwait(false);

                }
                stopWatch.Stop();
                DateTime endTime = DateTime.Now;

                //Add metricdata values
                MetricData.AddOrUpdateValue("duration", $"{stopWatch.ElapsedMilliseconds}");
                MetricData.AddOrUpdateValue("status", "C");
                MetricData.AddOrUpdateValue("log", $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{stopWatch.ElapsedMilliseconds}");

                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Info, new List<Variable>() { new Common.Logging.DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }));

                //Log end of method statement
                MetricData.AddOrUpdateValue("log", $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent");
                step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Debug);
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

        private List<SchemaMap> GetMatchingSchemaMappings(string fileName, IList<SchemaMap> schemaMappings)
        {
            List<SchemaMap> matches = new List<SchemaMap>();
            foreach (SchemaMap map in schemaMappings)
            {
                if (Regex.IsMatch(fileName, map.SearchCriteria))
                {
                    matches.Add(map);
                }
            }
            return matches;
        }

        public string GetStorageCodeFromKey(string key)
        {
            string storageCode = string.Empty;
            //temp-file/schemaloade/<flowId>/<storagecode>/<guids>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX))
            {
                int strtIdx = Sentry.data.Infrastructure.Helpers.ParsingHelpers.GetNthIndex(key, '/', 4);
                int endIdx = Sentry.data.Infrastructure.Helpers.ParsingHelpers.GetNthIndex(key, '/', 5);
                storageCode = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            return storageCode;
        }
    }
}
