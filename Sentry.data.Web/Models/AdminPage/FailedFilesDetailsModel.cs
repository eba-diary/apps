using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.AdminPage
{
    public class FailedFilesDetailsModel
    {
        public String Dataset { get; set; }

        public int FileCount { get; set; }

        public String Schema { get; set; }
    }
}