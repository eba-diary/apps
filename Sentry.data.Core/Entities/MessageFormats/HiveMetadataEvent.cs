using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class HiveMetadataEvent : BaseEventMessage
    {
        public SchemaModel Schema { get; set; }
    }
}
