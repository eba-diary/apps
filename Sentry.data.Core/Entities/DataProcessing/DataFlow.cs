using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlow
    {
        public DataFlow()
        {
            //Assign new guild value
            Guid g = Guid.NewGuid();
            FlowGuid = g;
        }
        public virtual int Id { get; set; }
        public virtual Guid FlowGuid { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual IList<DataFlowStep> Steps { get; set; }
        public virtual IList<FlowExecution> Executions { get; set; }
    }
}
