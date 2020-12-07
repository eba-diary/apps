using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public enum ConsumptionLayerTableStatusEnum
    {
        Pending = 0,
        Requested = 1,
        Available = 2,
        NameReserved = 3,
        RequestFailed = 4,
        Deleted = 5,
        DeleteRequested = 6,
        DeleteFailed =7
    }
}
