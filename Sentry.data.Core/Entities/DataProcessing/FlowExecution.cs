using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class FlowExecution
    {
        public virtual int Id { get; set; }
        public virtual Guid ExecutionGuid { get; set; }
        public virtual DataFlow DataFlow { get; set; }
        public virtual IList<RunInstance> RunInstances { get; set; }
    }
}
