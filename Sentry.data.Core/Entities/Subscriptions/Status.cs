using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Status
    {
        public Status()
        {

        }

        public virtual int Status_ID { get; set; }

        public virtual string Description { get; set; }
    }
}
