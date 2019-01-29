using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SecurityTicket
    {

        public SecurityTicket() { }


        public Guid Id{get;set;}
        public string TicketId { get; set; }
        public string RequestedById { get; set; }
        public string ApprovedById { get; set; }
        public string RejectedById { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime RejectedDate { get; set; }
        public string TicketStatus { get; set; }
        public bool IsAddingPermission { get; set; }
        public bool IsRemovingPermission { get; set; }
        public string AdGroupName { get; set; }

        public List<SecurityPermission> Permissions { get; set; }

    }
}
