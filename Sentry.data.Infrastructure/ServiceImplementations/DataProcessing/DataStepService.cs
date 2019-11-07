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
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.S3;

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

                SetStepProvider(step.DataAction_Type_Id);

                _provider.ExecuteAction(step, stepEvent);

                dsContext.SaveChanges();

                //step.ProcessEvent(stepEvent, stepEvent.ExecutionGuid);
            }
        }

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.MaxValue;
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _dsContext = container.GetInstance<IDatasetContext>();

                List<DataFlow_Log> Logs = new List<DataFlow_Log>();
                try
                {                
                    startTime = DateTime.Now;
                    SetStepProvider(step.DataAction_Type_Id);

                    if (_provider != null)
                    {
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <datastepservice-publishstartevent>", Log_Level.Debug);

                    _provider.PublishStartEvent(step, flowExecutionGuid, runInstanceGuid, s3Event);
                    endTime = DateTime.Now;

                        ////Step was successfull, therefore, only log single summary record
                        //foreach (DataFlow_Log log in step.Executions.Where(w => w.RunInstanceGuid == runInstanceGuid).ToList())
                        //{
                        //    step.Executions.Remove(log);
                        //}

                        //step.LogExecution(flowExecutionGuid, runInstanceGuid, $"{step.DataAction_Type_Id.ToString()}-step-success start:{startTime} end:{endTime} duration:{endTime - startTime}", Log_Level.Debug);
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    }
                    else
                    {
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"datastepserivce-notconfiguredforprovider provider:{step.DataAction_Type_Id.ToString()}", Log_Level.Warning);
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    }

                    _dsContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    if (endTime == DateTime.MaxValue)
                    {
                        endTime = DateTime.Now;
                    }

                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);

                    _dsContext.SaveChanges();
                }
            }
        }

        private void SetStepProvider(DataActionType actionType)
        {

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
