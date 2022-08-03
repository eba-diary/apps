using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
namespace Sentry.data.Web
{
    public class AuditSelectionModel
    {
        public List<SelectListItem> AllDatasets { get; set; }
        public List<SelectListItem> AllAuditTypes{ get; set; }
        public List<SelectListItem> AllAuditSearchTypes { get; set; }
        public int[] AuditAddedSearchKey { get; set; }
        public List<string> Schema { get; set; }
    }
}