using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SnowflakeEventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IDatasetContext _dsContext;
        private readonly ISchemaService _schemaService;
        #endregion

        #region Constructor
        public SnowflakeEventHandler(IDatasetContext dsContext, ISchemaService schemaService)
        {
            _dsContext = dsContext;
            _schemaService = schemaService;
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
            FileSchema de = null;
            List<SchemaConsumptionSnowflake> consumptionDetails = null;

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

                        de = _dsContext.GetById<FileSchema>(snowRequestedEvent.SchemaID);
                        consumptionDetails = de.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList();

                        foreach(var consumption in consumptionDetails)
                        {
                            consumption.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Requested.ToString();
                            consumption.LastChanged = DateTime.Now;
                        }

                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case "SNOW-TABLE-CREATE-COMPLETED":
                        SnowTableCreateModel snowCompletedEvent = JsonConvert.DeserializeObject<SnowTableCreateModel>(msg);
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowCompletedEvent)}");
                        de = _dsContext.GetById<FileSchema>(snowCompletedEvent.SchemaID);
                        string newSnowStatus;

                        switch (snowCompletedEvent.SnowStatus.ToUpper())
                        {
                            case "CREATED":
                            case "EXISTED":
                                newSnowStatus = ConsumptionLayerTableStatusEnum.Available.ToString();
                                break;
                            case "FAILED":
                                newSnowStatus = ConsumptionLayerTableStatusEnum.RequestFailed.ToString();
                                break;
                            default:
                                newSnowStatus = ConsumptionLayerTableStatusEnum.Pending.ToString();
                                break;
                        }

                        consumptionDetails = de.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList();

                        foreach (var consumption in consumptionDetails)
                        {
                            consumption.SnowflakeStatus = newSnowStatus;
                            consumption.LastChanged = DateTime.Now;
                        }

                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case "SNOW-TABLE-DELETE-REQUESTED":
                        SnowTableDeleteModel snowDeleteEvent = JsonConvert.DeserializeObject<SnowTableDeleteModel>(msg);
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(snowDeleteEvent)}");
                        de = _dsContext.GetById<FileSchema>(snowDeleteEvent.SchemaID);
                        de.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList().ForEach(c => c.SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteRequested.ToString());
                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    case "SNOW-TABLE-DELETE-COMPLETED":
                        SnowTableDeleteModel deleteCompletedEvent = JsonConvert.DeserializeObject<SnowTableDeleteModel>(msg);
                        Logger.Info($"snowflakeeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(deleteCompletedEvent)}");
                        de = _dsContext.GetById<FileSchema>(deleteCompletedEvent.SchemaID);

                        switch (deleteCompletedEvent.SnowStatus.ToUpper())
                        {
                            case "DELETED":
                            case "SKIPPED":
                                de.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList().ForEach(c => c.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Deleted.ToString());
                                break;
                            case "FAILED":
                                de.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList().ForEach(c => c.SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteFailed.ToString());
                                break;
                            default:
                                de.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().ToList().ForEach(c => c.SnowflakeStatus = ConsumptionLayerTableStatusEnum.Pending.ToString());
                                break;
                        }

                        _dsContext.SaveChanges();
                        Logger.Info($"snowflakeeventhandler processed {baseEvent.EventType.ToUpper()} message");
                        break;
                    default:
                        Logger.Info($"snowflakeeventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
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
