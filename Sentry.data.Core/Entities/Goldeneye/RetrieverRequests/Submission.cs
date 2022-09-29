using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Submission
    {
        public Submission() { }

        public virtual int SubmissionId { get; set; }
        public virtual RetrieverJob JobId { get; set; }
        public virtual Guid JobGuid { get; set; }
        public virtual string Serialized_Job_Options { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual string FlowExecutionGuid { get; set; }
        public virtual string RunInstanceGuid { get; set; }
        public virtual string ClusterUrl { get; set; }
    }
}
