using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Sentry.data.Web.Hubs
{
    public class ProgressHub : Hub
    {
        public void Send(string userId, string message)
        {
            Clients.User(userId).send(message);
        }
    }
}