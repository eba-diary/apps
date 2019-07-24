﻿
using System.Collections.Generic;
using System.Linq;
using Sentry.data.Web.Helpers;

namespace Sentry.data.Web
{
    public static class RequestAccessExtension
    {

        public static Core.AccessRequest ToCore(this AccessRequestModel model)
        {
            return new Core.AccessRequest()
            {
                ObjectId = model.ObjectId,
                AdGroupName = model.AdGroupName,
                BusinessReason = model.BusinessReason,
                SelectedPermissionCodes = model.SelectedPermissions != null ? model.SelectedPermissions.Split(',').ToList() : new List<string>(),
                SelectedApprover = model.SelectedApprover
            };
        }


        public static AccessRequestModel ToModel(this Core.AccessRequest core)
        {

            return new AccessRequestModel()
            {
                ObjectId = core.ObjectId,
                ObjectName = core.ObjectName,
                AllPermissions = core.Permissions.ToModel(),
                AllApprovers = Utility.BuildSelectListitem(core.ApproverList, "Select an approver")
            };
        }

    }
}