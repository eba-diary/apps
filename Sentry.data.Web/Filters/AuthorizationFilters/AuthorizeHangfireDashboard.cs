using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire.Dashboard;
using Sentry.data.Core;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class AuthorizeHangfireDashboard : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            UserService userService = DependencyResolver.Current.GetService<UserService>();
            IApplicationUser user = userService.GetCurrentUser();
            return user.CanUserSwitch;
        }
    }
}