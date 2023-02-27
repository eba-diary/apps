using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class AddSchemaDto
    {
        public FileSchemaDto SchemaDto { get; set; }
        public DataFlowDto DataFlowDto { get; set; }
        public DatasetFileConfigDto DatasetFileConfigDto { get; set; }
    }
}
