using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.AdminPage
{
    public class SchemaProcessActivityModel
    {
        public string Schema { get; set; }

        public int SchemaId { get; set; }
        
        public int DatasetId { get; set; }

        public int FileCount { get; set; }

        public DateTime RecentRun { get; set; }
    }
}