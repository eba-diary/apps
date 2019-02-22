﻿
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class AccessRequest
    {
        public int SecurableObjectId { get; set; }
        public Guid SecurityId { get; set; }
        public string SecurableObjectName { get; set; }
        public string AdGroupName { get; set; }
        public string PermissionForUserId { get; set; }
        public string PermissionForUserName { get; set; }
        public string BusinessReason { get; set; }
        public string RequestorsId { get; set; }
        public string RequestorsName { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ApproverId { get; set; }
        public bool IsProd { get; set; }

        public List<Permission> Permissions { get; set; }
        
        public List<KeyValuePair<string,string>> ApproverList { get; set; }
        public List<DataAsset> DataAssest { get; set; }

        public List<string> SelectedPermissionCodes { get; set; }
        public string SelectedApprover { get; set; }
    }
}
