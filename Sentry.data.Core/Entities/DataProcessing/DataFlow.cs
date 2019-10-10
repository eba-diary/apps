using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlow
    {
        public int Id { get; set; }
        public Guid FlowGuid { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDTM { get; set; }
        public string CreatedBy { get; set; }
        public IList<DataFlowStep> Steps { get; set; }
    }
}
