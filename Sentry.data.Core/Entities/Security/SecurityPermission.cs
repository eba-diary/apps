using System;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class SecurityPermission : IValidatable
    {

        public SecurityPermission() { }


        public virtual Guid SecurityPermissionId { get; set; }
        public virtual bool IsEnabled { get; set; }

        public virtual DateTime AddedDate { get; set; }
        public virtual DateTime? EnabledDate { get; set; }
        public virtual DateTime? RemovedDate { get; set; }

        public virtual SecurityTicket AddedFromTicket { get; set; }
        public virtual SecurityTicket RemovedFromTicket { get; set; }

        public virtual Permission Permission { get; set; }




        public virtual ValidationResults ValidateForDelete()
        {
            throw new NotImplementedException();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (AddedDate < new DateTime(1800, 1, 1))
            {
                vr.Add("AddedDate", "The Added Date is required");
            }
           if(AddedFromTicket == null)
            {
                vr.Add("AddedFromTicket", "Ticket reference is required on the Security Permission");
            }
            if (Permission == null)
            {
                vr.Add("Permission", "Permission is required on the Security Permission");
            }

            return vr;
        }
    }
}
