using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class HiveTableCreateModel
    {
        public HiveTableCreateModel()
        {
            EventType = "HIVE-TABLE-CREATE";
        }
        public string EventType { get; set; }
        public SchemaModel Schema { get; set; }
    }
}
