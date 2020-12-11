using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;

namespace Sentry.data.Infrastructure
{
    public class SnowflakeEventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IDatasetContext _dsContext;
        #endregion

        #region Constructor
        public SnowflakeEventHandler(IDatasetContext dsContext)
        {
            _dsContext = dsContext;
        }
        #endregion
               
        void IMessageHandler<string>.Handle(string msg)
        {
            Logger.Info($"Start method <snowflakeeventhandler-handle>");
            BaseEventMessage baseEvent = null;
            FileSchema de = null;

            try
            {
                baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            }
            catch (Exception ex)
            {
                Logger.Error($"snowflakeeventhandler failed to convert incoming event", ex);
            }

            try
            {
                switch (baseEvent.EventType.ToUpper())
                {
                    case "SNOW-TABLE-CREATE-REQUESTED":
                        SnowTableCreateModel snowRequestedEvent = JsonConvert.DeserializeObject<SnowTableCreateModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowRequestedEvent)}");

                        de = _dsContext.GetById<FileSchema>(snowRequestedEvent.SchemaID);
                        de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Requested.ToString();
                        _dsContext.SaveChanges();
                        break;
                    case "SNOW-TABLE-CREATE-COMPLETED":
                        SnowTableCreateModel snowCompletedEvent = JsonConvert.DeserializeObject<SnowTableCreateModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowCompletedEvent)}");
                        de = _dsContext.GetById<FileSchema>(snowCompletedEvent.SchemaID);

                        switch (snowCompletedEvent.SnowStatus.ToUpper())
                        {
                            case "CREATED":
                            case "EXISTED":
                                de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Available.ToString();
                                //Add SAS notification logic here if needed for snowflake
                                break;
                            case "FAILED":
                                de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.RequestFailed.ToString();
                                break;
                            default:
                                de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Pending.ToString();
                                break;
                        }

                        _dsContext.SaveChanges();
                        break;
                    case "SNOW-TABLE-DELETE-REQUESTED":
                        SnowTableDeleteModel snowDeleteEvent = JsonConvert.DeserializeObject<SnowTableDeleteModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowDeleteEvent)}");
                        de = _dsContext.GetById<FileSchema>(snowDeleteEvent.SchemaID);
                        de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteRequested.ToString();
                        _dsContext.SaveChanges();
                        break;
                    case "SNOW-TABLE-DELETE-COMPLETED":
                        SnowTableDeleteModel deleteCompletedEvent = JsonConvert.DeserializeObject<SnowTableDeleteModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(deleteCompletedEvent)}");
                        de = _dsContext.GetById<FileSchema>(deleteCompletedEvent.SchemaID);

                        switch (deleteCompletedEvent.SnowStatus.ToString())
                        {
                            case "DELETED":
                            case "SKIPPED":
                                de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Deleted.ToString();
                                break;
                            case "FAILED":
                                de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteFailed.ToString();
                                break;
                            default:
                                de.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Pending.ToString();
                                break;
                        }

                        _dsContext.SaveChanges();
                        break;
                    default:
                        Logger.Info($"SnowflakeHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.Error($"snowflakeeventhandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }
            Logger.Info($"End method <snowflakeeventhandler-handle>");
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("DfsEventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("DfsEventHandlerInitialized");
        }
    }
}
