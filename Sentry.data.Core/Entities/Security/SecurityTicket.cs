using System;
using System.Collections.Generic;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class SecurityTicket : IValidatable
    {

        public SecurityTicket() { }


        public virtual Guid SecurityTicketId{get;set;}
        public virtual string TicketId { get; set; }
        public virtual string RequestedById { get; set; }
        public virtual string ApprovedById { get; set; }
        public virtual string RejectedById { get; set; }
        public virtual DateTime RequestedDate { get; set; }
        public virtual DateTime? ApprovedDate { get; set; }
        public virtual DateTime? RejectedDate { get; set; }
        public virtual string RejectedReason { get; set; }
        public virtual string TicketStatus { get; set; }
        public virtual bool IsAddingPermission { get; set; }
        public virtual bool IsRemovingPermission { get; set; }
        public virtual string AdGroupName { get; set; }
        public virtual bool IsSecuredAtUserLevel { get; set; }
        public virtual string GrantPermissionToUserId { get; set; }
        public virtual string AwsArn { get; set; }

        public virtual Security ParentSecurity { get; set; }

        public virtual IList<SecurityPermission> Permissions { get; set; }

        public virtual string SaidKeyCode { get; set; }


        public virtual ValidationResults ValidateForDelete()
        {
            throw new NotImplementedException();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (string.IsNullOrWhiteSpace(TicketId))
            {
                vr.Add("TicketId", "TicketId is required");
            }
            if (string.IsNullOrWhiteSpace(RequestedById))
            {
                vr.Add("RequestedById", "Request by Id is required");
            }
            if (RequestedDate < new DateTime(1800, 1, 1))
            {
                vr.Add("RequestedDate", "The Requested Date is required");
            }
            if (string.IsNullOrWhiteSpace(TicketStatus))
            {
                vr.Add("TicketStatus", "Ticket Status is required");
            }
            if(Permissions == null || Permissions.Count == 0)
            {
                vr.Add("Permissions", "Permissions are required on the ticket");
            }

            return vr; 
        }
    }
}
