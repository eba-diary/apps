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
        public virtual string Url { get; set; }
        public virtual string UrlType { get; set; }
        public virtual string Type { get; set; }
        public virtual string Name { get; set; }
    }
}
