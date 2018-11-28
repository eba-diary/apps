using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class MetadataTag : IValidatable
    {
        public virtual int TagId { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<Dataset> Datasets { get; set; }
        public SearchableTag GetSearchableTag()
        {
            return new SearchableTag(this);
        }

        public ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "Name is required");
            }           
            if (string.IsNullOrWhiteSpace(CreatedBy))
            {
                vr.Add(ValidationErrors.nameIsBlank, "CreatedBy is required");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string nameIsBlank = "nameIsBlank";
            public const string createdByIsBlank = "createdByIsBlank";
        }

        
    }
}
