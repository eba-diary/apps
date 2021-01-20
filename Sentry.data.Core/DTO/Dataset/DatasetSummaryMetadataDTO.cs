using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetSummaryMetadataDTO
    {
        public int DatasetId { get; set; }
        public long FileCount { get; set; }
        public DateTime Max_Created_DTM { get; set; }
    }
}
