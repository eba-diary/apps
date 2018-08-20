using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class HiveTable
    {
        public HiveTable()
        {

        }

        public virtual int Hive_ID { get; set; }

        public virtual Schema Schema { get; set; }
        public virtual string Hive_NME { get; set; }
        public virtual string Hive_DSC { get; set; }
        public virtual string HiveDatabase_NME { get; set; }
        public virtual Boolean IsPrimary { get; set; }

        public virtual DateTime Created_DTM { get; set; }
        public virtual DateTime Changed_DTM { get; set; }
    }
}
