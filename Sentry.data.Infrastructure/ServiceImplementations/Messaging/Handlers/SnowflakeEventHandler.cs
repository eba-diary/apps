using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SnowflakeEventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IDatasetContext _dsContext;
        private readonly IDataFeatures _dataFeatures;
        #endregion

        #region Constructor
        public SnowflakeEventHandler(IDatasetContext dsContext, IDataFeatures dataFeatures)
        {
            _dsContext = dsContext;
            _dataFeatures = dataFeatures;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            throw new NotImplementedException();
        }

        async Task IMessageHandler<string>.HandleAsync(string msg)
        {
            Logger.Info($"Start method <snowflakeeventhandler-handle>");
            BaseEventMessage baseEvent = null;
            FileSchema schema = null;

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
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowRequestedEvent)}");

                        schema = _dsContext.GetById<FileSchema>(snowRequestedEvent.SchemaID);
                        UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Requested);
                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case "SNOW-TABLE-CREATE-COMPLETED":
                        SnowTableCreateModel snowCompletedEvent = JsonConvert.DeserializeObject<SnowTableCreateModel>(msg);
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowCompletedEvent)}");
                        schema = _dsContext.GetById<FileSchema>(snowCompletedEvent.SchemaID);

                        switch (snowCompletedEvent.SnowStatus.ToUpper())
                        {
                            case "CREATED":
                            case "EXISTED":
                                UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Available);
                                break;
                            case "FAILED":
                                UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.RequestFailed);
                                break;
                            default:
                                UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Pending);
                                break;
                        }

                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case "SNOW-TABLE-DELETE-REQUESTED":
                        SnowTableDeleteModel snowDeleteEvent = JsonConvert.DeserializeObject<SnowTableDeleteModel>(msg);
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowDeleteEvent)}");
                        schema = _dsContext.GetById<FileSchema>(snowDeleteEvent.SchemaID);
                        UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.DeleteRequested);
                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case "SNOW-TABLE-DELETE-COMPLETED":
                        SnowTableDeleteModel deleteCompletedEvent = JsonConvert.DeserializeObject<SnowTableDeleteModel>(msg);
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(deleteCompletedEvent)}");
                        schema = _dsContext.GetById<FileSchema>(deleteCompletedEvent.SchemaID);

                        switch (deleteCompletedEvent.SnowStatus.ToUpper())
                        {
                            case "DELETED":
                            case "SKIPPED":
                                UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Deleted);
                                break;
                            case "FAILED":
                                UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.DeleteFailed);
                                break;
                            default:
                                UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Pending);
                                break;
                        }

                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case GlobalConstants.SnowConsumptionMessageTypes.CREATE_REQUEST:
                    case GlobalConstants.SnowConsumptionMessageTypes.CREATE_RESPONSE:
                    case GlobalConstants.SnowConsumptionMessageTypes.DELETE_REQUEST:
                    case GlobalConstants.SnowConsumptionMessageTypes.DELETE_RESPONSE:
                        if (_dataFeatures.CLA5211_SendNewSnowflakeEvents.GetValue()) //When we remove CLA5211 feature flag, the above switch statement can be removed, the following function handles it 
                        {
                            HandleSnowConsumptionMessage(msg);
                        }
                        break;
                    default:
                        Logger.Info($"snowflakeeventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.Error($"{nameof(SnowflakeEventHandler)} failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
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

        public void HandleSnowConsumptionMessage(string message)
        {
            SnowConsumptionMessageModel snowConsumptionMessage = GetSnowConsumptionMessageModelFromMessage(message);

            if (snowConsumptionMessage.SchemaID == 0)
            {
                return; //dataset snowflake schema, nothing to update
            }

            FileSchema schema = _dsContext.GetById<FileSchema>(snowConsumptionMessage.SchemaID);

            switch (snowConsumptionMessage.EventType)
            {
                case (GlobalConstants.SnowConsumptionMessageTypes.CREATE_REQUEST):
                    UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Requested);
                    break;                
                case (GlobalConstants.SnowConsumptionMessageTypes.CREATE_RESPONSE):
                    switch (snowConsumptionMessage.SnowStatus.ToUpper())
                    {
                        case "SUCCESS":
                            UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Available);
                            break;
                        case "FAILURE":
                            UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.RequestFailed);
                            break;
                        default:
                            UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Pending);
                            break;
                    }
                    break;                
                case (GlobalConstants.SnowConsumptionMessageTypes.DELETE_REQUEST):
                    UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.DeleteRequested);
                    break;                
                case (GlobalConstants.SnowConsumptionMessageTypes.DELETE_RESPONSE):
                    switch (snowConsumptionMessage.SnowStatus.ToUpper())
                    {
                        case "SUCCESS":
                            UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Deleted);
                            break;
                        case "FAILURE":
                            UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.DeleteFailed);
                            break;
                        default:
                            UpdateSchemaConsumptionDetailsStatus(schema, ConsumptionLayerTableStatusEnum.Pending);
                            break;
                    }
                    break;
                default:
                    break;
            }

            _dsContext.SaveChanges();

        }

        private SnowConsumptionMessageModel GetSnowConsumptionMessageModelFromMessage(string message)
        {
            return JsonConvert.DeserializeObject<SnowConsumptionMessageModel>(message);
        }

        private void UpdateSchemaConsumptionDetailsStatus(FileSchema schema, ConsumptionLayerTableStatusEnum status)
        {
            schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList().ForEach(c => c.SnowflakeStatus = status.ToString());
        }
    }
}
