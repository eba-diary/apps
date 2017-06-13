using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class FilterNameModel
    {
        public int id { get; set; }

        public string value { get; set; }
        
        public int count { get; set; }

        public Boolean isChecked { get; set; }

    }
}