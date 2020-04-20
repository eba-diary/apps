using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DaleSearchModel
    {

        public DaleSearchModel() { }

        public bool Table { get; set; }
        public bool Column { get; set; }
        public string Criteria { get; set; }


    }
}