
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class RequestAccess
    {
        public int DatasetId { get; set; }
        public string AdGroupName { get; set; }
        public string BusinessReason { get; set; }
        public string RequestorsId { get; set; }
        public string RequestorsName { get; set; }
        public string PrimaryApproverId { get; set; }
        public string SecondaryApproverId { get; set; }
        public DateTime RequestedDate { get; set; }
        public bool IsProd { get; set; }

        public List<Permission> Permissions { get; set; }
    }
}
