using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataElementDto
    {
        public string SchemaName { get; set; }
        public string SchemaDescription { get; set; }
        public bool SchemaIsForceMatch { get; set; }
        public bool SchemaIsPrimary { get; set; }
        public string Delimiter { get; set; }
        public DateTime DataElementChange_DTM { get; set; }
        public bool HasHeader { get; set; }
        public int FileFormatId { get; set; }
    }
}
