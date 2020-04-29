using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web
{
    public class DaleSearchModel
    {
        public string Criteria { get; set; }

        public DaleDestination Destination { get; set; }

        public List<DaleResultModel> DaleResults { get; set; }
    }


}
