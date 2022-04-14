using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ManagePermissionsModel
    {
        public ManagePermissionsModel(DatasetPermissionsDto permissions)
        {
            DatasetId = permissions.DatasetId;
            DatasetName = permissions.DatasetName;
            foreach (var permission in permissions.Permissions)
            {
                Permissions.Add(new ManagePermissionModel(permission));
            }
        }
        public int DatasetId { get; set; }
        public string DatasetName { get; set; } 
        public IList<ManagePermissionModel> Permissions { get; set; } = new List<ManagePermissionModel>();

    }
}