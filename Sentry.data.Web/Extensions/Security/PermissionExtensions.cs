
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public static class PermissionExtensions
    {

        public static PermissionModel ToModel(this Core.Permission core)
        {
            return new PermissionModel()
            {
                PermissionCode = core.PermissionCode,
                PermissionDescription = core.PermissionDescription,
                PermissionName = core.PermissionName,
                SecurableObject = core.SecurableObject
            };
        }


        public static List<PermissionModel> ToModel(this List<Core.Permission> coreList)
        {
            List<PermissionModel> permissions = new List<PermissionModel>();

            if (coreList == null) { return permissions; }

            coreList.ForEach(x => permissions.Add(x.ToModel()));

            return permissions;
        }

    }
}