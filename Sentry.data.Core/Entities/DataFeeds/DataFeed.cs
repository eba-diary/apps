using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFeed
    {

        public DataFeed() { }

        public virtual int Id { get; set; }
        public virtual int? Id2 { get; set; }           //depending on what kind of feed type, the Id2 may be used as a secondary part of the URL thats clicked in the feed 
        public virtual string Url { get; set; }
        public virtual string UrlType { get; set; }
        public virtual string Type { get; set; }
        public virtual string Name { get; set; }
        public virtual string Category { get; set; }
    }
}
