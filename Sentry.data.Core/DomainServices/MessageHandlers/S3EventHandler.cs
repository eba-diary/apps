using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;


namespace Sentry.data.Core
{
    public class S3EventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IDataFlowProvider _dataFlowProvider;
        private readonly IDataFeatures _dataFeatures;
        #endregion

        #region Constructor
        public S3EventHandler(IDataFlowProvider dataFlowProvider, IDataFeatures dataFeatures)
        {
            _dataFlowProvider = dataFlowProvider;
            _dataFeatures = dataFeatures;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            throw new NotImplementedException();
        }

        async Task IMessageHandler<string>.HandleAsync(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper() == "S3EVENT")
                {
                    S3Event s3Event = JsonConvert.DeserializeObject<S3Event>(msg);
                    Logger.Info("s3eventhandler processing S3EVENT message: " + JsonConvert.SerializeObject(s3Event));

                    switch (s3Event.PayLoad.eventName.ToUpper())
                    {
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_PUT:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_POST:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COPY:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COMPLETEMULTIPARTUPLOAD:
                            Logger.Info($"S3EventHandler processing AWS event - {JsonConvert.SerializeObject(s3Event)}");
                            await _dataFlowProvider.ExecuteDependenciesAsync(s3Event.PayLoad.s3.bucket.name, s3Event.PayLoad.s3.Object.key, s3Event.PayLoad).ConfigureAwait(false);
                            break;
                        default:
                            Logger.Info($"s3eventhandler not configured to handle AWS event type ({s3Event.EventType}), skipping event.");
                            break;
                    }
                }
                else
                {
                    Logger.Info($"s3eventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"s3eventhandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("S3EventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("S3EventHandlerInitialized");
        }
    }
}
