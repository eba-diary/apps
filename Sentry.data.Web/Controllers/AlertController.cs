using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class AlertController : BaseController
    {
        // GET: Alert
        public ActionResult Index()
        {
            return View();
        }
    }
}