using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Livy
{
    public class HiveColumn
    {
        public string name { get; set; }
        public string datatype { get; set; }
        public Boolean nullable { get; set; }
        public Boolean found { get; set; }
    }
}
