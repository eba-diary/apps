using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web.Filters.AuthorizationFilters
{
    public class WebApiAuthorizeByPermissionAttribute : WebApiAuthorizeAttribute
    {
        List<string> _permissions = new List<string>();

        public WebApiAuthorizeByPermissionAttribute(string permission)
        {
            _permissions.Add(permission);
        }

        public WebApiAuthorizeByPermissionAttribute(string[] permissions)
        {
            _permissions.AddRange(permissions);
        }

        public override bool DoesUserHaveAccess(IApplicationUser appUser)
        {
            return appUser.Permissions.Any(p => _permissions.Contains(p));
        }
    }
}