﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Sentry.data.Web.Hubs;
using Sentry.data.Core;

namespace Sentry.data.Web.Helpers
{
    public class ProgressUpdater
    {
        private static UserService _userService;

        public static void SendProgress(string progressMessage, int percentDone)
        {
            //IN ORDER TO INVOKE SIGNALR FUNCTIONALITY DIRECTLY FROM SERVER SIDE WE MUST USE THIS
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();

            //CALCULATING PERCENTAGE BASED ON THE PARAMETERS SENT
            var percentage = percentDone;

            //PUSHING DATA TO ALL CLIENTS
            hubContext.Clients.All.AddProgress(progressMessage, percentage + "%");
            
            //hubContext.Clients.User(_userService.GetCurrentUser().AssociateId).AddPrgress(progressMessage, percentage + "%");
        }
    }
}