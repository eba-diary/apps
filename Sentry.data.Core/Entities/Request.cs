using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Request
    {
        public string TaskName { get; set; }

        public DateTime RequestTime { get; set; }

        /// <summary>
        /// Email of orignial requestor
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Target location for request response.  Typically, the dataset bundle drop location for dataset loader.
        /// </summary>
        public string DatasetDropLocation { get; set; }

        /// <summary>
        /// Associate ID of request initiator
        /// </summary>
        public string RequestInitiatorId { get; set; }

        public string EventId { get; set; }

        public string Data { get; set; }

    }
}
