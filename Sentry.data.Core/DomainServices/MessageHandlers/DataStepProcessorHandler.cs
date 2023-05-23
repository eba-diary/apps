using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core.Interfaces.DataProcessing;
using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataStepProcessorHandler : IMessageHandler<string>
    {
        private IDataStepService _dataStepProvider;
        private IDataFlowProvider _dataFlowProvider;

        public DataStepProcessorHandler(IDataStepService dataStepProvider, IDataFlowProvider dataFlowProvider)
        {
            _dataStepProvider = dataStepProvider;
            _dataFlowProvider = dataFlowProvider;
        }

        public void Handle(String msg)
        {
            throw new NotImplementedException();
        }

        public async Task HandleAsync(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper().StartsWith("DATAFLOWSTEP"))
                {
                    //TODO: Add feature flag filtering around specific DATAFLOWSTEP events
                    DataFlowStepEvent stepEvent = JsonConvert.DeserializeObject<DataFlowStepEvent>(msg);
                    Logger.Info("DataStepProcessorHandler processing DATAFLOWSTEP message: " + JsonConvert.SerializeObject(stepEvent));
                    await _dataFlowProvider.ExecuteStepAsync(stepEvent).ConfigureAwait(false);
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
