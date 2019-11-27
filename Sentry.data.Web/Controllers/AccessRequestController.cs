using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    public class AccessRequestController : BaseController
    {

        private readonly IObsidianService _obsidianService;
        private readonly IDatasetService _datasetService;
        private readonly INotificationService _notificationService;

        public AccessRequestController(IObsidianService obsidianService, IDatasetService datasetService, INotificationService notificationService)
        {
            _obsidianService = obsidianService;
            _notificationService = notificationService;
            _datasetService = datasetService;
        }

       


    }
}