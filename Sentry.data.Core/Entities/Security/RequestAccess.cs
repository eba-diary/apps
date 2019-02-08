
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class AccessRequest
    {
        public int DatasetId { get; set; }
        public Guid SecurityId { get; set; }
        public string DatasetName { get; set; }
        public string AdGroupName { get; set; }
        public string BusinessReason { get; set; }
        public string RequestorsId { get; set; }
        public string RequestorsName { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ApproverId { get; set; }
        public bool IsProd { get; set; }

        public List<Permission> Permissions { get; set; }
        public List<KeyValuePair<string,string>> ApproverList { get; set; }

        public List<string> SelectedPermissionCodes { get; set; }
        public string SelectedApprover { get; set; }
    }
}
