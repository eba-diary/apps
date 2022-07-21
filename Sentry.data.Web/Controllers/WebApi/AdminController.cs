using Sentry.data.Core;
using Sentry.data.Web.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers.WebApi
{
    public class AdminController : BaseWebApiController
    {
        private readonly IDatasetContext _datasetContext;

        public AdminController(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }
    }
}