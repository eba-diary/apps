using System;
using System.Collections.Generic;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class Security : IValidatable
    {

        public Security() {}
        public Security(string securableEntityName)
        {
            SecurableEntityName = securableEntityName;
            CreatedDate = DateTime.Now;
            EnabledDate = DateTime.Now;
        }

        public virtual Guid SecurityId { get; set; }
        public virtual string SecurableEntityName { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime? RemovedDate { get; set; }
        public virtual DateTime EnabledDate { get; set; }
        public virtual string UpdatedById { get; set; }
        public virtual string CreatedById { get; set; }
        public virtual ISet<SecurityTicket> Tickets { get; set; }





        public virtual ValidationResults ValidateForDelete()
        { //We should never delete this record.
            throw new NotImplementedException();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (CreatedDate < new DateTime(1800, 1, 1)) 
            {
                vr.Add("CreatedDate", "The Created Date is required");
            }
            if (CreatedDate < new DateTime(1800, 1, 1))
            {
                vr.Add("EnabledDate", "The Enabled Date is required");
            }
            if (string.IsNullOrWhiteSpace(SecurableEntityName))
            {
                vr.Add("ObjectType", "The Object Type is required");
            }

            return vr;
        }
    }
}
