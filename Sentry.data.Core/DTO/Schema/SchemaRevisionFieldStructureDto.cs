using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaRevisionFieldStructureDto
    {
        public SchemaRevisionDto Revision { get; set; }
        public List<BaseFieldDto> FieldStructure { get; set; }
    }
}
