﻿
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Web.Helpers;

namespace Sentry.data.Web
{
    public static class RequestAccessExtension
    {

        public static Core.AccessRequest ToCore(this DatasetAccessRequestModel model)
        {
            return new Core.AccessRequest()
            {
                SecurableObjectId = model.SecurableObjectId,
                AdGroupName = model.AdGroupName,
                BusinessReason = model.BusinessReason,
                SelectedPermissionCodes = model.SelectedPermissions != null ? model.SelectedPermissions.Split(',').ToList() : new List<string>(),
                SelectedApprover = model.SelectedApprover
            };
        }
        public static Core.AccessRequest ToCore(this NotificationAccessRequestModel model)
        {
            return new Core.AccessRequest()
            {
                SecurableObjectId = model.SecurableObjectId,
                PermissionForUserId = model.PermssionForUserId,
                BusinessReason = model.BusinessReason,
                SelectedPermissionCodes = model.SelectedPermissions != null ? model.SelectedPermissions.Split(',').ToList() : new List<string>(),
                SelectedApprover = model.SelectedApprover
            };
        }

        public static Core.AccessRequest ToCore(this DataSourceAccessRequestModel model)
        {
            return new Core.AccessRequest()
            {
                SecurableObjectId = model.SecurableObjectId,
                AdGroupName = model.AdGroupName,
                BusinessReason = model.BusinessReason,
                SelectedPermissionCodes = model.SelectedPermissions != null ? model.SelectedPermissions.Split(',').ToList() : new List<string>(),
                SelectedApprover = model.SelectedApprover
            };
        }

        public static DatasetAccessRequestModel ToDatasetModel(this Core.AccessRequest core)
        {
            return new DatasetAccessRequestModel()
            {
                SecurableObjectId = core.SecurableObjectId,
                SecurableObjectName = core.SecurableObjectName,
                AllPermissions = core.Permissions.ToModel(),
                AllApprovers = Utility.BuildSelectListitem(core.ApproverList, "Select an approver")
            };
        }
        public static DataSourceAccessRequestModel ToDataSourceModel(this Core.AccessRequest core)
        {
            return new DataSourceAccessRequestModel()
            {
                SecurableObjectId = core.SecurableObjectId,
                SecurableObjectName = core.SecurableObjectName,
                AllPermissions = core.Permissions.ToModel(),
                AllApprovers = Utility.BuildSelectListitem(core.ApproverList, "Select an approver")
            };
        }

        public static NotificationAccessRequestModel ToNotificationModel(this Core.AccessRequest core)
        {
            return new NotificationAccessRequestModel()
            {
                SecurableObjectId = core.SecurableObjectId,
                SecurableObjectName = core.SecurableObjectName,
                AllPermissions = core.Permissions.ToModel(),
                AllSecurableObjects = core.DataAssest.Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.Id.ToString() }).ToList(),
                AllApprovers = Utility.BuildSelectListitem(core.ApproverList, "Select an approver")
            };
        }
    }
}