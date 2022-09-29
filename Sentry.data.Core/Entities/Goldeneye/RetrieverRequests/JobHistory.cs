using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class JobHistory
    {
        public JobHistory()
        {
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }
        public virtual int HistoryId { get; set; }
        public virtual RetrieverJob JobId { get; set; }
        public virtual Guid JobGuid { get; set; }
        public virtual int BatchId { get; set; }
        public virtual string State { get; set; }
        public virtual string LivyAppId { get; set; }
        public virtual string LivyDriverLogUrl { get; set; }
        public virtual string LivySparkUiUrl { get; set; }
        public virtual string LogInfo { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual Boolean Active { get; set; }
        public virtual Submission Submission { get; set; }
        public virtual string ClusterUrl { get; set; }
    }
}
