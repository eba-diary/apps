
using System.Collections.Generic;
using System.Linq;
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
    }
}