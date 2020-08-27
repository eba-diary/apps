using Sentry.Core;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class SchemaMap : IValidatable
    {
        public virtual int Id { get; set; }
        public virtual DataFlowStep DataFlowStepId { get; set; }
        public virtual FileSchema MappedSchema { get; set; }
        public virtual Dataset Dataset { get; set; }
        public virtual string SearchCriteria { get; set; }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults errors = new ValidationResults();
            if (Dataset == null)
            {
                errors.Add(ValidationErrors.schemamapMustContainDataset, "Schema Map must map to dataset");
            }
            if (MappedSchema == null)
            {
                errors.Add(ValidationErrors.schemamapMustContainSchema, "Schema Map must map to schema");
            }
            return errors;
        }

        public class ValidationErrors
        {
            public const string schemamapMustContainDataset = "schemamapMustContainDataset";
            public const string schemamapMustContainSchema = "schemamapMustContainSchema";
        }
    }
}
