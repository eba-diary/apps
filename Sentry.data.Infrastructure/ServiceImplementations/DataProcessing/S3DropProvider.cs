﻿using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using Sentry.data.Core.Entities.S3;
using System.IO;
using StructureMap;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    //ActionType attribute utilized to map entity to provider within DataStepProvider.cs
    [ActionType(DataActionType.S3Drop)]
    public class S3DropProvider : IS3DropProvider
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDatasetContext _datasetContext;

        public S3DropProvider(IMessagePublisher messagePublisher, 
            IS3ServiceProvider s3ServiceProvider, IDatasetContext datasetContext)
        {
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _datasetContext = datasetContext;
        }

        public void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"start-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            try
            {
                DateTime startTime = DateTime.Now;
                string fileName = Path.GetFileName(stepEvent.SourceKey);
                step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()} processing event - {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                string versionKey = _s3ServiceProvider.CopyObject(stepEvent.SourceBucket, stepEvent.SourceKey, stepEvent.TargetBucket, $"{stepEvent.TargetPrefix}{Path.GetFileName(stepEvent.SourceKey)}");
                DateTime endTime = DateTime.Now;

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
                                key = $"{stepEvent.TargetPrefix}{fileName}"
                            }
                        }
                    }
                };

                _messagePublisher.PublishDSCEvent("99999", JsonConvert.SerializeObject(s3e));

                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}",Log_Level.Info));
            }
            catch (Exception ex)
            {
                step.Executions.Add(step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-executeaction failed", Log_Level.Error, ex));
                step.LogExecution(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-executeaction", Log_Level.Debug);
            }
        }

        public void PublishStartEvent(DataFlowStep step, string bucket, string key, string flowExecutionGuid, string runInstanceGuid)
        {
            List<DataFlow_Log> logs = new List<DataFlow_Log>();
            try
            {
                DateTime startTime = DateTime.Now;
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
                    SourceBucket = bucket,
                    SourceKey = key,
                    TargetBucket = step.Action.TargetStorageBucket,
                    //add run instance (separated by dash) if not null
                    TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id.ToString()}/" + $"{flowExecutionGuid}{((runInstanceGuid == null) ? String.Empty : "-"+runInstanceGuid)}/",
                    EventType = GlobalConstants.DataFlowStepEvent.S3_DROP_START
                };

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-sendingstartevent {JsonConvert.SerializeObject(stepEvent)}", Log_Level.Debug);

                _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
                DateTime endTime = DateTime.Now;

                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent-successful  start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Info);
            }
            catch (Exception ex)
            {
                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-publishstartevent failed", Log_Level.Error, ex));
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <{step.DataAction_Type_Id.ToString()}>-publishstartevent", Log_Level.Debug);
            }
        }
    }
}
