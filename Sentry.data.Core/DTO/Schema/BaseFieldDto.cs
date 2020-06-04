using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BaseFieldDto
    {
        public int FieldId { get; set; }
        public Guid FieldGuid { get; set; }
        public string Name { get; set; }
        public DateTime CreateDTM { get; set; }
        public DateTime LastUpdatedDTM { get; set; }
        public string FieldType { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public List<BaseFieldDto> ChildFields { get; set; }
        public string SourceFormat { get; set; }
        public bool IsArray { get; set; }
        public bool Nullable { get; set; }
        public int OrdinalPosition { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
    }
}
