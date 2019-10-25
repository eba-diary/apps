using Sentry.data.Core;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.DataProcessing.Actions;
using System.Reflection;
using Newtonsoft.Json;

namespace Sentry.data.Infrastructure.ServiceImplementations.DataProcessing
{
    public class DataStepService : IDataStepService
    {
        private IBaseActionProvider _provider;
        public void ExecuteStep(DataFlowStepEvent stepEvent)
        {
            using(IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext dsContext = container.GetInstance<IDatasetContext>();

                DataFlowStep step = dsContext.GetById<DataFlowStep>(stepEvent.StepId);

                //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                //var types = from type in assemblies.GetType()
                //            where Attribute.IsDefined(type, typeof(ActionTypeAttribute)) && ((ActionTypeAttribute)type).ActionType == step.DataAction_Type_Id
                //            select type

                //foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                //{
                //    foreach(Type item in assembly.GetTypes())
                //    {
                //        ActionTypeAttribute attribs = from type in assembly.GetTypes()
                //                                      where Attribute.IsDefined(type, typeof(ActionTypeAttribute)) && ((ActionTypeAttribute)type).ActionType == step.DataAction_Type_Id
                //                                      select type
                //    }
                //    List<ActionTypeAttribute = assembly.GetTypes().Where(w => w.IsDefined(typeof(ActionTypeAttribute)))
                //}

                SetStepProvider(step.DataAction_Type_Id);

                _provider.ExecuteAction(step, stepEvent);

                dsContext.SaveChanges();

                //step.ProcessEvent(stepEvent, stepEvent.ExecutionGuid);
            }
        }

        public void PublishStartEvent(DataFlowStep step, string bucket, string key, string flowExecutionGuid, string runInstanceGuid)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _dsContext = container.GetInstance<IDatasetContext>();

                List<DataFlow_Log> Logs = new List<DataFlow_Log>();
                try
                {                
                    DateTime startTime = DateTime.Now;
                    SetStepProvider(step.DataAction_Type_Id);

                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <datastepservice-publishstartevent>", Log_Level.Debug);

                    _provider.PublishStartEvent(step, bucket, key, flowExecutionGuid, runInstanceGuid);
                    DateTime endTime = DateTime.Now;

                    ////Step was successfull, therefore, only log single summary record
                    //foreach (DataFlow_Log log in step.Executions.Where(w => w.RunInstanceGuid == runInstanceGuid).ToList())
                    //{
                    //    step.Executions.Remove(log);
                    //}

                    //step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-step-success start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Debug);
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    _dsContext.SaveChanges();
                }
                catch
                {
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    _dsContext.SaveChanges();
                }
            }

            //using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            //{
            //    IMessagePublisher _messagePublisher = container.GetInstance<IMessagePublisher>();

            //    DataFlowStepEvent stepEvent = new DataFlowStepEvent()
            //    {
            //        DataFlowId = step.DataFlow.Id,
            //        DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
            //        ExecutionGuid = FlowExecutionGuid,
            //        StepId = step.Id,
            //        ActionId = step.Action.Id,
            //        ActionGuid = step.Action.ActionGuid.ToString(),
            //        SourceBucket = bucket,
            //        SourceKey = key,
            //        TargetBucket = step.Action.TargetStorageBucket,
            //        TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.Id.ToString()}/" + $"{Epoch.ToString()}/",
            //        EventType = GlobalConstants.DataFlowStepEvent.S3_DROP_START
            //    };


            //    switch (step.DataAction_Type_Id)
            //    {
            //        case DataActionType.S3Drop:
            //            stepEvent.EventType = GlobalConstants.DataFlowStepEvent.S3_DROP_START;
            //            break;
            //        case DataActionType.RawStorage:
            //            stepEvent.EventType = GlobalConstants.DataFlowStepEvent.RAW_STORAGE_START;
            //            break;
            //        case DataActionType.QueryStorage:
            //            stepEvent.EventType = GlobalConstants.DataFlowStepEvent.QUERY_STORAGE;
            //            break;
            //        case DataActionType.SchemaLoad:
            //            stepEvent.EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD;
            //            break;
            //        case DataActionType.ConvertParquet:
            //            stepEvent.EventType = GlobalConstants.DataFlowStepEvent.CONVERT_TO_PARQUET;
            //            break;
            //        case DataActionType.None:
            //        default:
            //            break;
            //    }

            //    _messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JsonConvert.SerializeObject(stepEvent));
            //}
        }

        private void SetStepProvider(DataActionType actionType)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                switch (actionType)
                {
                    case DataActionType.S3Drop:
                        _provider = container.GetInstance<IS3DropProvider>();
                        break;
                    case DataActionType.RawStorage:
                        _provider = container.GetInstance<IRawStorageProvider>();
                        break;
                    case DataActionType.QueryStorage:
                        _provider = container.GetInstance<IQueryStorageProvider>();
                        break;
                    case DataActionType.SchemaLoad:
                        _provider = container.GetInstance<ISchemaLoadProvider>();
                        break;
                    case DataActionType.ConvertParquet:
                        _provider = container.GetInstance<IConvertToParquetProvider>();
                        break;
                    case DataActionType.None:
                    default:
                        break;
                }
            }
        }
    }
}
