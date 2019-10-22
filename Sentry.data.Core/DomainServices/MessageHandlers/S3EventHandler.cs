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
        private IDataFlowProvider _dataFlowProvider;
        #endregion

        #region Constructor
        public S3EventHandler(IDataFlowProvider dataFlowProvider)
        {
            _dataFlowProvider = dataFlowProvider;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper() == "S3EVENT")
                {
                    S3Event s3Event = JsonConvert.DeserializeObject<S3Event>(msg);
                    Logger.Info("S3EventHandler processing S3EVENT message: " + JsonConvert.SerializeObject(s3Event));

                    switch (s3Event.PayLoad.eventName.ToUpper())
                    {
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_PUT:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_POST:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COPY:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COMPLETEMULTIPARTUPLOAD:
                            Logger.Info($"S3EventHandler processing AWS event - {JsonConvert.SerializeObject(s3Event)}");
                            Task.Factory.StartNew(() => _dataFlowProvider.ExecuteDependenciesAsync(s3Event.PayLoad),
                                                                    TaskCreationOptions.LongRunning).ContinueWith(TaskException,
                                                                    TaskContinuationOptions.OnlyOnFaulted);
                            break;
                        default:
                            Logger.Info($"S3EventHandler not configured to handle AWS event type ({s3Event.EventType}), skipping event.");
                            break;
                    }
                }
                else
                {
                    Logger.Info($"S3EventHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"S3EventHandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
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

        private void TaskException(Task t)
        {
            Logger.Fatal("Exception occurred on main Windows Service Task. Stopping Service immediately.", t.Exception);
        }
    }
}
