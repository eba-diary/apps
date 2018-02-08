using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RTRequest
    {       
        public RTRequest()
        {
            Parameters = new List<RTRequestParameters>();
        }

        public virtual int Id { get; set; }
        public virtual int SourceTypeId { get; set; }
        public virtual RTSourceTypes SourceType { get; set; }
        public virtual int EndpointId { get; set; }
        public virtual RTAPIEndpoints Endpoint { get; set; }
        public virtual int RequestorId { get; set; }
        public virtual IList<RTRequestParameters> Parameters { get; set; }
        public virtual Boolean IsEnabled { get; set; }
        public virtual string SystemFolder { get; set; }
        public virtual string RequestName { get; set; }
        public virtual int DatasetFileConfigId { get; set; }
        public virtual string Options { get; set; }
    }
}
