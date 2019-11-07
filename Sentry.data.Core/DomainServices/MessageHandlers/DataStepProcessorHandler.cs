using System;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.Common.Logging;
using Newtonsoft.Json;
using StructureMap;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core
{
    public class DataStepProcessorHandler : IMessageHandler<string>
    {
        private IDataStepService _dataStepProvider;

        public DataStepProcessorHandler(IDataStepService dataStepProvider)
        {
            _dataStepProvider = dataStepProvider;
        }
        public void Handle(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper().StartsWith("DATAFLOWSTEP"))
                {
                    DataFlowStepEvent stepEvent = JsonConvert.DeserializeObject<DataFlowStepEvent>(msg);
                    Logger.Info("DataStepProcessorHandler processing DATAFLOWSTEP message: " + JsonConvert.SerializeObject(stepEvent));
                    Task.Factory.StartNew(() => _dataStepProvider.ExecuteStep(stepEvent),
                                                                    TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                    TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    Logger.Info($"DataStepProcessorHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"DataStepProcessorHandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }

        }

        public bool HandleComplete()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }
        private void TaskException(Task t)
        {
            Logger.Fatal("Exception occurred on main Windows Service Task. Stopping Service immediately.", t.Exception);
        }
    }
}
