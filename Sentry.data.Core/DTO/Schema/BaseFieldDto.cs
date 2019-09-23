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
        public string Name { get; set; }
        public DateTime CreateDTM { get; set; }
        public string FieldType { get; set; }
        public List<BaseFieldDto> ChildFields { get; set; }
    }
}
