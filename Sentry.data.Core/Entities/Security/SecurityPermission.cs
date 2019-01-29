using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SecurityPermission
    {

        public SecurityPermission() { }

        public SecurityPermission(Permission permission)
        {
            IsEnabled = false;
            AddedDate = DateTime.Now;
        }


        public Guid SecurityPermissionId { get; set; }
        public bool IsEnabled { get; set; }

        public DateTime AddedDate { get; set; }
        public DateTime EnabledDate { get; set; }
        public DateTime RemovedDate { get; set; }

        public Guid AddedFromTicket { get; set; }
        public Guid RemovedFromTicket { get; set; }

        public Permission Permission { get; set; }
    }
}
