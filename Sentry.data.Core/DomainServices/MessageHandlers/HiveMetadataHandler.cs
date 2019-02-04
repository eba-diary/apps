using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.Common.Logging;
using Newtonsoft.Json;
using Sentry.data.Core.Entities.Metadata;
using StructureMap;

namespace Sentry.data.Core
{
    public class HiveMetadataHandler : IMessageHandler<string>
    {
        #region Declarations
        private IDatasetContext _dsContext;
        #endregion

        #region Constructor
        public HiveMetadataHandler(IDatasetContext dsContext)
        {
            _dsContext = dsContext;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            DataElement de = null;
            try
            {
                switch (baseEvent.EventType.ToUpper())
                {
                    case "HIVE-TABLE-CREATE-COMPLETED":
                        HiveTableCreateModel hiveCreatedEvent = JsonConvert.DeserializeObject<HiveTableCreateModel>(msg);
                        Logger.Debug("HiveMetadataHandler processing HIVE-TABLE-CREATE-COMPLETED message: " + JsonConvert.SerializeObject(hiveCreatedEvent));

                        switch (hiveCreatedEvent.Schema.HiveStatus.ToUpper())
                        {
                            case "CREATED":
                            case "EXISTED":
                                de = _dsContext.GetById<DataElement>(hiveCreatedEvent.Schema.SchemaID);
                                de.HiveTableStatus = HiveTableStatusEnum.Available.ToString();
                                break;
                            case "FAILED":
                                de = _dsContext.GetById<DataElement>(hiveCreatedEvent.Schema.SchemaID);
                                de.HiveTableStatus = HiveTableStatusEnum.RequestFailed.ToString();
                                break;
                            default:
                                de = _dsContext.GetById<DataElement>(hiveCreatedEvent.Schema.SchemaID);
                                de.HiveTableStatus = HiveTableStatusEnum.Pending.ToString();
                                break;
                        }

                        _dsContext.Merge(de);
                        _dsContext.SaveChanges();
                        break;
                    case "HIVE-TABLE-CREATE-REQUESTED":
                        HiveTableCreateModel hiveReqeustedEvent = JsonConvert.DeserializeObject<HiveTableCreateModel>(msg);
                        Logger.Debug("HiveMetadataHandler processing HIVE-TABLE-CREATE-REQUESTED message: " + JsonConvert.SerializeObject(hiveReqeustedEvent));

                        de = _dsContext.GetById<DataElement>(hiveReqeustedEvent.Schema.SchemaID);
                        de.HiveTableStatus = HiveTableStatusEnum.Requested.ToString();

                        _dsContext.Merge(de);
                        _dsContext.SaveChanges();
                        break;
                    default:
                        Logger.Debug($"HiveMetadataHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"HiveMetadataHandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }                        
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            //do nothing
        }

        
    }
}
