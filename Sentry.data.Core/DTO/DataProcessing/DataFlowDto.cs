using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowDto
    {
        public int Id { get; set; }
        public Guid FlowGuid { get; set; }
        public string Name { get; set; }
        public DateTime CreateDTM { get; set; }
        public string CreatedBy { get; set; }
        public string DFQuestionnaire { get; set; }
        public List<SchemaMapDto> SchemaMap { get; set; }
        public RetrieverJobDto RetrieverJob { get; set; }
    }
}
