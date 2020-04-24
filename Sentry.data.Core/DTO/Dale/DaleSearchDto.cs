using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DaleSearchDto
    {
        public bool Table { get; set; }
        public bool Column { get; set; }
        public bool View { get; set; }
        public string Criteria { get; set; }
    }
}
