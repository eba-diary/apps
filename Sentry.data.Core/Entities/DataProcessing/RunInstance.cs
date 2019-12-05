using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class RunInstance
    {
        public virtual int Id { get; set; }
        public virtual Guid InstanceGuid { get; set; }
        public virtual IList<DataFlowStep> Steps { get; set; }
    }
}
