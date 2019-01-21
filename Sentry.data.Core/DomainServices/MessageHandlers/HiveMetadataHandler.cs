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
        IContainer _container;

        public HiveMetadataHandler()
        {

        }
        void IMessageHandler<string>.Handle(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            switch (baseEvent.EventType)
            {
                case "HIVE-TABLE-CREATE":
                    HiveMetadataEvent hiveEvent = JsonConvert.DeserializeObject<HiveMetadataEvent>(msg);
                    Logger.Debug("HiveMetadataHandler processing HIVE-TABLE-CREATE message: " + JsonConvert.SerializeObject(hiveEvent));

                    using (_container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                    {
                    }
                        DataElement de = _dsContext.GetById<DataElement>(hiveEvent);

                    break;
                default:
                    //do nothing
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
