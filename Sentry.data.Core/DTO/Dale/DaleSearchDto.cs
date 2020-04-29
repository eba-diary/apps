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
        public DaleDestination Destination { get; set; }
    }
}
