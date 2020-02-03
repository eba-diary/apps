using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.S3
{
    public class S3LamdaEvent
    {
        public S3LamdaEvent()
        {
            Records = new List<S3ObjectEvent>();
        }

        public List<S3ObjectEvent> Records { get; set; }
    }
}
