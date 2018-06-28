using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataSourceType
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string DiscrimatorValue { get; set;}
    }
}
