using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class RawFileAddModel : BaseEventMessage
    {
        public RawFileAddModel()
        {
            EventType = "SCHEMA-RAWFILE-ADD";
        }
        public string SourceBucket { get; set; } 
        public string SourceKey { get; set; }
        public string SourceVersionId { get; set; }
        public int SchemaID { get; set; }
    }
}
