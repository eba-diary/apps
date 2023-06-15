using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.GlobalEnums;
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
            DatasetNamedEnvironment = permissions.DatasetNamedEnvironment;
            DatasetSaidKeyCode = permissions.DatasetSaidKeyCode;

            foreach (var permission in permissions.Permissions)
            {
                IList<ManagePermissionModel> permissionCollection;
                switch (permission.SecurityPermission.Permission.PermissionCode)
                {
                    case PermissionCodes.SNOWFLAKE_ACCESS:
                        permissionCollection = SnowflakePermissions;
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
            InheritanceRequest = new RequestPermissionInheritanceModel(permissions);
            RemovePermissionRequest = new RemovePermissionModel(permissions);
            InheritanceTicket = permissions.InheritanceTicket != null ? permissions.InheritanceTicket : new SecurityTicket();
            InheritanceRequest.TicketId = InheritanceTicket != null ? InheritanceTicket.TicketId : "";
        }

        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetNamedEnvironment { get; set; }
        public string DatasetSaidKeyCode { get; set; }
        public IList<ManagePermissionModel> DscPermissions { get; set; } = new List<ManagePermissionModel>();
        public IList<ManagePermissionModel> AwsIamPermissions { get; set; } = new List<ManagePermissionModel>();
        public IList<ManagePermissionModel> SnowflakePermissions { get; set; } = new List<ManagePermissionModel>();
        public IList<ManagePermissionModel> AdPermissions { get; set; } = new List<ManagePermissionModel>();
        public RequestPermissionInheritanceModel InheritanceRequest { get; set; }
        public RemovePermissionModel RemovePermissionRequest { get; set; }
        public SecurityTicket InheritanceTicket { get; set; }

    }
}