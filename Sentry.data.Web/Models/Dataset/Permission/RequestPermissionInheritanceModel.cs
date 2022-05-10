using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class RequestPermissionInheritanceModel : AccessRequestModel
    {
        public RequestPermissionInheritanceModel() { }
        public RequestPermissionInheritanceModel(DatasetPermissionsDto permissions)
        {
            SecurableObjectId = permissions.DatasetId;
            DatasetName = permissions.DatasetName;
            List<KeyValuePair<string, string>> approverIdsNames = new List<KeyValuePair<string, string>>();
            foreach (SAIDRole prodCust in permissions.Approvers)
            {
                approverIdsNames.Add(new KeyValuePair<string, string>(prodCust.AssociateId, prodCust.Name));
            }
            AllApprovers = Utility.BuildSelectListitem(approverIdsNames, "Select an approver");
            SaidKeyCode = permissions.DatasetSaidKeyCode; 
        }
    }
}