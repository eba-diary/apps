using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaRow
    {
        public SchemaRow()
        {

        }

        public int DataObjectField_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string ArrayType { get; set; }
        public string Length { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
        public int Position { get; set; }
        public string Format { get; set; }
        public Boolean? Nullable { get; set; }
        public List<SchemaRow> ChildRows { get; set; }
        public double LastUpdated { get; set; }
    }
}
