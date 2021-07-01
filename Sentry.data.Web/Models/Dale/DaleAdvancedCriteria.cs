using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DaleAdvancedCriteria
    {
        public string Asset { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Object { get; set; }
        public string ObjectType { get; set; }
        public string Column { get; set; }
        public string SourceType { get; set; }
    }
}