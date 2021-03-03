﻿using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;

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

                SetStepProvider(step.DataAction_Type_Id, container);

                _provider.ExecuteAction(step, stepEvent);

                dsContext.SaveChanges();

                //step.ProcessEvent(stepEvent, stepEvent.ExecutionGuid);
            }
        }

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _dsContext = container.GetInstance<IDatasetContext>();

                List<EventMetric> Logs = new List<EventMetric>();
                try
                {                
                    SetStepProvider(step.DataAction_Type_Id, container);

                    if (_provider != null)
                    {
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <datastepservice-publishstartevent>", Log_Level.Debug);
                        _provider.PublishStartEvent(step, flowExecutionGuid, runInstanceGuid, s3Event);
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    }
                    else
                    {
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"datastepserivce-notconfiguredforprovider provider:{step.DataAction_Type_Id.ToString()}", Log_Level.Warning);
                        step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    }

                    _dsContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    _dsContext.SaveChanges();
                }
            }
        }

        private void SetStepProvider(DataActionType actionType, IContainer container)
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
            switch (actionType)
            {
                case DataActionType.S3Drop:
                case DataActionType.ProducerS3Drop:
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
                case DataActionType.UncompressZip:
                    _provider = container.GetInstance<IUncompressZipProvider>();
                    break;
                case DataActionType.UncompressGzip:
                    _provider = container.GetInstance<IUncompressGzipProvider>();
                    break;
                case DataActionType.SchemaMap:
                    _provider = container.GetInstance<ISchemaMapProvider>();
                    break;
                case DataActionType.GoogleApi:
                    _provider = container.GetInstance<IGoogleApiActionProvider>();
                    break;
                case DataActionType.ClaimIq:
                    _provider = container.GetInstance<IClaimIQActionProvider>();
                    break;
                case DataActionType.FixedWidth:
                    _provider = container.GetInstance<IFixedWidthProvider>();
                    break;
                case DataActionType.None:
                default:
                    break;
            }
        }
    }
}