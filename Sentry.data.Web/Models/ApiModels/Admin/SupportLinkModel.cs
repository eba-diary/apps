using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Admin
{
    public class SupportLinkModel
    {
        public int SupportLinkId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}