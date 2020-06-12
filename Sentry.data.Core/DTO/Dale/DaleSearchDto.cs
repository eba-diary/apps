using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class DaleSearchDto
    {
        public string Criteria { get; set; }
        public DaleDestiny Destiny { get; set; }
        public DaleSensitive Sensitive { get; set; }
    }
}
