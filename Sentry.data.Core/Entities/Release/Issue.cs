using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Issue
    {
        public int id { get; set;  }
        public string key { get; set; }
        public string self { get; set; }
        public string expand { get; set; }

       public Fields fields { get; set; }
    }

    public class Fields
    {
        public string description { get; set; }
        public string summary { get; set; }
        public string customfield_10302 { get; set; } //Business Impact
        public string customfield_10303 { get; set; } //Resolution Description

        public List<FixVersion> fixVersions { get; set; }

        public Priority priority { get; set; }

        public Epic epic { get; set; }
    }

    public class FixVersion
    {
        public string self { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public bool archived { get; set; }
        public bool released { get; set; }

    }

    public class Priority
    {
        public string self { get; set; }
        public string id { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }

    }

    public class Epic
    {

        public string id { get; set; }
        public string key { get; set; }
        public string self { get; set; }
        public string name { get; set; }
        public string summary { get; set; }
        public bool done { get; set; }
    }

}
