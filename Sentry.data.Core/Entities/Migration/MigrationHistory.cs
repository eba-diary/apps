using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class MigrationHistory
    {
        public virtual int MigrationHistoryId { get; set; }
        public virtual DateTime CreateDateTime { get; set; }

        public virtual IList<MigrationHistoryDetail> MigrationHistoryDetails { get; set; }
    }
}
