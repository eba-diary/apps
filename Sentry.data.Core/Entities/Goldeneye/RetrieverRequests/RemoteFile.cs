using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RemoteFile
    {
        public string Name { get; set; }
        public Int64 Size { get; set; }
        public DateTime Modified { get; set; }
        public string Type { get; set; }
    }
}
