using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.WebApi
{
    /// <summary>
    /// This Web API attribute can be added to methods to restrict them to users with at least one of the given permissions
    /// </summary>
    public class WebApiAuthorizeByPermissionAttribute : WebApiAuthorizeAttribute
    {
        private List<string> _permissions = new List<string>();

        public WebApiAuthorizeByPermissionAttribute(string permission)
        {
            _permissions.Add(permission);
        }

        public WebApiAuthorizeByPermissionAttribute(params string[] permissions)
        {
            _permissions.AddRange(permissions);
        }

        public override bool DoesUserHaveAccess(IApplicationUser appUser)
        {
            return appUser.Permissions.Any(p => _permissions.Contains(p));
        }
    }

}