using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.S3;

namespace Sentry.data.Core
{
    public class S3Event : BaseEventMessage
    {
        public S3Event()
        {
            EventType = "S3EVENT";
        }
        public S3ObjectEvent PayLoad { get; set; }
    }
}
