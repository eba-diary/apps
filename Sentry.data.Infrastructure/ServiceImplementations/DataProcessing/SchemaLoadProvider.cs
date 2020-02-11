using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Sentry.data.Infrastructure
{
    public class SchemaLoadProvider : ISchemaLoadProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private ISchemaService _schemaService;

        public SchemaLoadProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider, ISchemaService schemaService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _schemaService = schemaService;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug));
            try
            {
                DateTime startTime = DateTime.Now;
                stopWatch.Start();
                string fileName = Path.GetFileName(stepEvent.SourceKey);
                logs.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug));

                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.TargetBucket, $"{stepEvent.TargetPrefix}{fileName}");
                DateTime endTime = DateTime.Now;
                stopWatch.Stop();
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
                                name = stepEvent.TargetBucket
                            },
                            _object = new Sentry.data.Core.Entities.S3.Object()
                            {
                                key = $"{stepEvent.TargetPrefix}{fileName}",
                                size = 200124
                            }
                        }
                    }
                };
#endif
                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));

                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-successful", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds)}, null));
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

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            Stopwatch stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}-publishstartevent", Log_Level.Debug);
                string objectKey = s3Event.s3.Object.key;
                string keyBucket = s3Event.s3.bucket.name;

                //Evaluate incoming file matches search criteria of any SchemaMappings for Data Step
                List<SchemaMap> validMappings = GetMatchingSchemaMappings(Path.GetFileName(objectKey), step.SchemaMappings);
                
                if (validMappings.Any())
                {
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()} schemamapping-match {validMappings.Count} matching schema maps for {objectKey}", Log_Level.Debug);
                }
                else
                {
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()} schemamapping-notdetected file will not be processed - {objectKey}", Log_Level.Warning);
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
                        TargetBucket = step.Action.TargetStorageBucket,
                        //<targetstorageprefix>/<dataflowid>/<storagecode>/<flow execution guid>[-<run instance guid>]/
                        TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id}/{item.MappedSchema.StorageCode}/{GenerateGuid(flowExecutionGuid, runInstanceGuid)}/ ",
                        EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD,
                        FileSize = s3Event.s3.Object.size.ToString(),
                        S3EventTime = s3Event.eventTime.ToString("s"),
                        OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                    };

                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Info);

                    _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                }
                stopWatch.Stop();

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  duration:{stopWatch.Elapsed.TotalSeconds.ToString()}", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, null);

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}-publishstartevent>", Log_Level.Debug);
            }
            catch (Exception ex)
            {
                if (stopWatch.IsRunning)
                {
                    stopWatch.Stop();
                }
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex);
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}-publishstartevent>", Log_Level.Debug);
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
                int strtIdx = GetNthIndex(key, '/', 4);
                int endIdx = GetNthIndex(key, '/', 5);
                storageCode = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            return storageCode;
        }

        private string GenerateGuid(string executionGuid, string instanceGuid)
        {
            if (instanceGuid == null)
            {
                return executionGuid;
            }
            else
            {
                return executionGuid + "-" + instanceGuid;
            }            
        }

        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
