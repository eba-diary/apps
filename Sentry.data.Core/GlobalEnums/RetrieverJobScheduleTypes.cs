using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.GlobalEnums
{
    public enum RetrieverJobScheduleTypes
    {
        [Description("Hourly")]
        Hourly = 1,
        [Description("Daily")]
        Daily = 2,
        [Description("Weekly")]
        Weekly = 3,
        [Description("Monthly")]
        Monthly = 4,
        [Description("Yearly")]
        Yearly = 5
    }
}
