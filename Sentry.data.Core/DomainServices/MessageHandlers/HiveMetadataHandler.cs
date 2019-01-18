using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.Common.Logging;

namespace Sentry.data.Core
{
    public class HiveMetadataHandler : IMessageHandler<HiveMetadataEvent>
    {
        public void HiveMetadataEvent()
        {
            // Method intentionally left empty.
        }
        void IMessageHandler<HiveMetadataEvent>.Handle(HiveMetadataEvent msg)
        {
            Logger.Debug("HiveMetadataHandler processing message: " + msg.ToString());
        }

        bool IMessageHandler<HiveMetadataEvent>.HandleComplete()
        {
            return true;
        }

        void IMessageHandler<HiveMetadataEvent>.Init()
        {
            throw new NotImplementedException();
        }

        
    }
}
