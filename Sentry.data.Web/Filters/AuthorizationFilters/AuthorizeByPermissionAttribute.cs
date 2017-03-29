using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class AuthorizeByPermissionAttribute : AuthorizeAttribute
    {
        List<string> _permissions = new List<string>();

        public AuthorizeByPermissionAttribute(string permission)
        {
            _permissions.Add(permission);
        }

        public AuthorizeByPermissionAttribute(params string[] permissions)
        {
            _permissions.AddRange(permissions);
        }
        
        public override Boolean DoesUserHaveAccess(IApplicationUser appUser)
        {
            return appUser.Permissions.Any(((p) => _permissions.Contains(p)));
        }
    }
}
