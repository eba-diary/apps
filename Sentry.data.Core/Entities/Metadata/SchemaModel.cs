using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaModel
    {
        public SchemaModel()
        {          
        }

        public string SchemaID { get; set; }
        public string Format { get; set; }
        public string Header { get; set; }
        public string Delimiter { get; set; }
        public string HiveTable { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveStatus { get; set; }
        public IList<ColumnModel> Columns { get; set; }
    }   
}
