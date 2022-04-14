using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ManagePermissionModel
    {
        public ManagePermissionModel(SecurablePermission permission)
        {
            Scope = permission.Scope.GetDescription();
            Identity = permission.Identity;
            PermissionDescription = permission.Permission.PermissionDescription;
        }

        public string Scope { get; set; }
        public string Identity { get; set; }
        public string PermissionDescription { get; set; }
    }
}