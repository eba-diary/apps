using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetScopeType : IValidatable
    {
        public DatasetScopeType() { }

        public virtual int ScopeTypeId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Boolean IsEnabled { get; set; }


        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "Name of Scope is Required");
            }
            if (string.IsNullOrWhiteSpace(Description))
            {
                vr.Add(ValidationErrors.descriptionIsBlank, "Description of Scope is Required");
            }

            return vr;
        }

        public class ValidationErrors
        {
            public const string nameIsBlank = "nameIsBlank";
            public const string descriptionIsBlank = "descriptionIsBlank";
        }
    }
}