﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web.Controllers
{
    public class DaleController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly IDataFeatures _featureFlags;
        private readonly IDaleService _daleService;

        public DaleController(IEventService eventService,IDataFeatures featureFlags, IDaleService daleService)
        {
            _eventService = eventService;
            _featureFlags = featureFlags;
            _daleService = daleService;
        }

        public ActionResult DaleSearch()
        {
            if ( _featureFlags.Expose_DaleSearch_CLA_1450.GetValue() || SharedContext.CurrentUser.IsAdmin)
            {
                DaleSearchModel searchModel = new DaleSearchModel();
                return View(searchModel);
            }
            else
            {
                return View("Forbidden");
            }
        }

        [HttpPost]
        public ActionResult GetSearchResults(DaleSearchModel searchModel)
        {

            if (_featureFlags.Expose_DaleSearch_CLA_1450.GetValue() || SharedContext.CurrentUser.IsAdmin)
            {
                searchModel.DaleResults = _daleService.GetSearchResults(searchModel.ToDto()).ToWeb();
                return View("DaleResult", searchModel);
            }
            else
            {
                return View("Forbidden");
            }
        }
    }
}