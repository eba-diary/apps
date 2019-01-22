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
            switch (baseEvent.EventType.ToUpper())
            {
                case "HIVE-TABLE-CREATED":
                    HiveTableCreateModel hiveEvent = JsonConvert.DeserializeObject<HiveTableCreateModel>(msg);
                    Logger.Debug("HiveMetadataHandler processing HIVE-TABLE-CREATED message: " + JsonConvert.SerializeObject(hiveEvent));

                    switch (hiveEvent.Schema.HiveStatus.ToUpper())
                    {
                        case "CREATED":
                        case "EXISTED":
                            de = _dsContext.GetById<DataElement>(hiveEvent);
                            de.HiveTableStatus = HiveTableStatusEnum.Available.ToString();
                            break;
                        case "FAILED":
                        default:
                            de = _dsContext.GetById<DataElement>(hiveEvent);
                            de.HiveTableStatus = HiveTableStatusEnum.Pending.ToString();
                            break;
                    }                    

                    _dsContext.Merge(de);
                    _dsContext.SaveChanges();

                    break;
                default:
                    Logger.Debug($"HiveMetadataHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                    break;
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
