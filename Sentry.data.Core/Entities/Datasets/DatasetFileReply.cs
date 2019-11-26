using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetFileReply
    {
        public virtual int DatasetFileReplyId { get; set; }
        public virtual int DatasetFileId { get; set; }
        public virtual int SchemaID { get; set; }
        public virtual string FileLocation { get; set; }
        public virtual string ReplayStatus { get; set; }
        public virtual int DatasetId { get; set; }
    }
}
