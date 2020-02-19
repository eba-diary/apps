using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sentry.data.Infrastructure
{
    //ActionType attribute utilized to map entity to provider within DataStepProvider.cs
    [ActionType(DataActionType.SchemaMap)]
    public class SchemaMapProvider : BaseActionProvider, ISchemaMapProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private DataFlowStep _step;
        private string _flowGuid;
        private string _runInstGuid;


        public SchemaMapProvider(IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider,
            IDataFlowService dataFlowService) : base(dataFlowService)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
        }

        public override void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
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

                DateTime endTime = DateTime.Now;
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

        public override void PublishStartEvent(DataFlowStep step, string FlowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            Stopwatch stopWatch = new Stopwatch();
            string objectKey = s3Event.s3.Object.key;
            string keyBucket = s3Event.s3.bucket.name;
            try
            {
                foreach (SchemaMap scmMap in step.SchemaMappings)
                {
                    stopWatch.Start();
                    DateTime startTime = DateTime.Now;

                    /**********************************
                     * Determine if incoming file matches search criteria for schema mapping
                     * if the search criteria is null, then process file
                     **********************************/
                    bool processFile = (scmMap.SearchCriteria == null) ? true : Regex.IsMatch(Path.GetFileName(objectKey), scmMap.SearchCriteria);

                    if (processFile)
                    {
                        /*****************************************************
                        *  Find the S3 Drop step for the given schema mapping
                        ******************************************************/
                        DataFlowStep s3DropStep;
                        string targetSchemaS3DropPrefix;
                        string targetSchemaS3Bucket;
                        //DataFlowStep s3DropStep = _dataFlowService.GetS3DropStepForFileSchema(scmMap.MappedSchema);
                        using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                        {
                            IDatasetContext datasetContext = container.GetInstance<IDatasetContext>();

                            string schemaFlowName = _dataFlowService.GetDataFlowNameForFileSchema(scmMap.MappedSchema);
                            DataFlow flow = datasetContext.DataFlow.Where(w => w.Name == schemaFlowName).FirstOrDefault();
                            s3DropStep = datasetContext.DataFlowStep.Where(w => w.DataFlow == flow && w.DataAction_Type_Id == DataActionType.RawStorage).FirstOrDefault();

                            targetSchemaS3DropPrefix = s3DropStep.TriggerKey;
                            targetSchemaS3Bucket = s3DropStep.Action.TargetStorageBucket;
                        }

                        step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);

                        DataFlowStepEvent stepEvent = new DataFlowStepEvent()
                        {
                            DataFlowId = step.DataFlow.Id,
                            DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                            FlowExecutionGuid = FlowExecutionGuid,
                            RunInstanceGuid = runInstanceGuid,
                            StepId = step.Id,
                            ActionId = step.Action.Id,
                            ActionGuid = step.Action.ActionGuid.ToString(),
                            SourceBucket = keyBucket,
                            SourceKey = objectKey,
                            StepTargetBucket = step.Action.TargetStorageBucket,
                            //add run instance (separated by dash) if not null
                            StepTargetPrefix = targetSchemaS3DropPrefix + $"{FlowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/",
                            DownstreamTargets = new List<DataFlowStepEventTarget>() { new DataFlowStepEventTarget() { BucketName = targetSchemaS3Bucket, ObjectKey = targetSchemaS3DropPrefix + $"{FlowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-" + runInstanceGuid)}/" } },
                            EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_MAP_START,
                            FileSize = s3Event.s3.Object.size.ToString(),
                            S3EventTime = s3Event.eventTime.ToString("s"),
                            OriginalS3Event = JsonConvert.SerializeObject(s3Event)
                        };

                        step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                        _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));

                        stopWatch.Stop();
                        DateTime endTime = DateTime.Now;

                        step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Info, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) });
                    }
                    else
                    {
                        /*****************************************************
                        *  Log message stating no match
                        *  Object cannot be deleted since it may be processed by another schema mapping
                        ******************************************************/
                        step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-searchcriteria-nomatch bucket:{keyBucket} file:{objectKey} searchcriteria:{scmMap.SearchCriteria}", Log_Level.Debug);
                    }
                }                
            }
            catch (Exception ex)
            {
                step.Executions.Add(step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-failed", Log_Level.Error, new List<Variable>() { new DoubleVariable("stepduration", stopWatch.Elapsed.TotalSeconds) }, ex));
                step.LogExecution(FlowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
