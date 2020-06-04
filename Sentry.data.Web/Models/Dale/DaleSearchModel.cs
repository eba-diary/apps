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

        public DaleDestiny Destiny { get; set; }

        public bool Sensitive { get; set; }

        public bool CanDaleSensitiveView { get; set; }

        public List<DaleResultModel> DaleResults { get; set; }
        
    }
}
