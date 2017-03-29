
using Sentry.data.Core;
using System;

namespace Sentry.data.Web
{

    public class AuthorizeUseAppAttribute : AuthorizeAttribute
    {
        public override Boolean DoesUserHaveAccess(IApplicationUser appUser)
        {
            return appUser.CanUseApp;
        }
    }
}
