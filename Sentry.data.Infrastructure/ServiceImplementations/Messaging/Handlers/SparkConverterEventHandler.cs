using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SparkConverterEventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IEventService _eventService;
        #endregion

        #region Constructor
        public SparkConverterEventHandler(IEventService eventService)
        {
            _eventService = eventService;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            throw new NotImplementedException();
        }

        async Task IMessageHandler<string>.HandleAsync(string msg)
        {
            HandleLogic(msg);
        }

        public void HandleLogic(string msg)
        {
            Logger.Info($"Start method <sparkconvertereventhandler-handle>");
            BaseEventMessage baseEvent = null;

            try
            {
                baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            }
            catch (Exception ex)
            {
                Logger.Error($"sparkconvertereventhandler failed to convert incoming event", ex);
            }

            try
            {
                if(baseEvent != null && baseEvent.EventType != null)
                {
                    switch (baseEvent.EventType.ToUpper())
                    {
                        case "SPARKCONVERTERSTATUS":

                            SparkConverterModel sparkEvent = JsonConvert.DeserializeObject<SparkConverterModel>(msg);
                            Logger.Info($"sparkconvertereventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(sparkEvent)}");
                           
                            if (sparkEvent.Status != null && sparkEvent.Status.ToUpper() == "SUCCESS" && sparkEvent.DatasetID > 0)
                            {
                                _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.CREATED_FILE, GlobalConstants.EventType.CREATED_FILE, sparkEvent.DatasetID);
                                Logger.Info($"sparkconvertereventhandler processed {baseEvent.EventType.ToUpper()} message");
                            }
                            else
                            {
                                Logger.Info($"sparkconvertereventhandler handle {baseEvent.EventType.ToUpper()} event type was null or not marked success or empty DatasetID, skipping event.");
                            }
                            break;

                        default:
                            Logger.Info($"sparkconvertereventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                            break;
                    }
                }
                else
                {
                    Logger.Error($"sparkconvertereventhandler failed to parse baseEvent or EventType.  BaseEvent or baseEvent.EventType is null");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"sparkconvertereventhandler failed to process message: Msg:({msg})", ex);
            }
            Logger.Info($"End method <sparkconvertereventhandler-handle>");
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("SparkConverterEventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("SparkConverterHandlerInitialized");
        }
    }
}
