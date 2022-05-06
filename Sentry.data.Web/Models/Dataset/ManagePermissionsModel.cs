using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web
{
    public class ManagePermissionsModel
    {
        public ManagePermissionsModel(DatasetPermissionsDto permissions)
        {
            DatasetId = permissions.DatasetId;
            DatasetName = permissions.DatasetName;
            DatasetSaidKeyCode = permissions.DatasetSaidKeyCode;
            foreach (var permission in permissions.Permissions)
            {
                IList<ManagePermissionModel> permissionCollection;
                switch (permission.SecurityPermission.Permission.PermissionCode)
                {
                    case PermissionCodes.SNOWFLAKE_ACCESS:
                        permissionCollection = AdPermissions;
                        break;
                    case PermissionCodes.S3_ACCESS:
                        permissionCollection = AwsIamPermissions;
                        break;
                    default:
                        permissionCollection = DscPermissions;
                        break;
                }
                permissionCollection.Add(new ManagePermissionModel(permission, DatasetName, DatasetSaidKeyCode));
            }
        }

        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetSaidKeyCode { get; set; }
        public IList<ManagePermissionModel> DscPermissions { get; set; } = new List<ManagePermissionModel>();
        public IList<ManagePermissionModel> AwsIamPermissions { get; set; } = new List<ManagePermissionModel>();
        public IList<ManagePermissionModel> AdPermissions { get; set; } = new List<ManagePermissionModel>();

    }
}