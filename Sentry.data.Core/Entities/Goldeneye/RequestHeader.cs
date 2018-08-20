using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    [Serializable]
    //This approach is described at the site http://ivanz.com/2011/06/16/editing-variable-length-reorderable-collections-in-asp-net-mvc-part-1/
    public class RequestHeader
    {
        public virtual string Index { get; set; }
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }
    }
}
