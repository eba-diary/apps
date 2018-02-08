using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{




    public class Sprint
    {
        public int id { get; set; }
        public int sequence { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public int linkedPagesCount { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string completeDate { get; set; }

        public List<Issue> issues { get; set; }
    }
}
