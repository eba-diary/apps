﻿using Sentry.data.Core;
using Sentry.data.Web.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Web.Models.ApiModels.Admin;
using Sentry.data.Core.Interfaces;
using System.Web.Http;

namespace Sentry.data.Web.Controllers.WebApi
{
    public class AdminController : Controller
    {
        private readonly ISupportLink _supportLinkService;

        public AdminController(ISupportLink supportLinkService)
        {
            _supportLinkService = supportLinkService;
        }

        public IHttpActionResult AddSupportLink()

        public IHttpActionResult DeleteSupportLink(int id)
        {

        }

        public ActionResult Index()
        {
            return View();
        }
    }
}