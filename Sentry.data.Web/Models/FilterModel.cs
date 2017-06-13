using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class FilterModel
    {
        public string FilterType { get; set; }

        public IList<FilterNameModel> FilterNameList { get; set; }

    }
}