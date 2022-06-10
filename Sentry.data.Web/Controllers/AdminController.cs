using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Data;
using Sentry.data.Core;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.Shared;
using DoddleReport.Web;
using DoddleReport;
using Sentry.Core;
using System.Threading.Tasks;

namespace Sentry.data.Web.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
    }
}