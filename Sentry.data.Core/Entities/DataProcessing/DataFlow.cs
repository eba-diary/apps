using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlow : IValidatable, ISecurable
    {
        public DataFlow()
        {
            //Assign new guild value
            Guid g = Guid.NewGuid();
            FlowGuid = g;
        }
        public virtual int Id { get; set; }
        public virtual Guid FlowGuid { get; set; }
        public virtual int IngestionType { get; set; }
        public virtual string FlowStorageCode { get; set; }
        public virtual string SaidKeyCode { get; set; }
        public virtual int DatasetId { get; set; }
        public virtual int SchemaId { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual DateTime ModifiedDTM { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string Questionnaire { get; set; }
        public virtual IList<DataFlowStep> Steps { get; set; }
        public virtual IList<EventMetric> Logs { get; set; }

        //Delete implementation
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }
        public virtual bool IsDecompressionRequired { get; set; }
        public virtual int? CompressionType { get; set; }
        public virtual bool IsPreProcessingRequired { get; set; }
        public virtual int? PreProcessingOption { get; set; }

        public virtual string UserDropLocationBucket { get; set; }
        public virtual string UserDropLocationPrefix { get; set; }

        public virtual string NamedEnvironment { get; set; }
        public virtual NamedEnvironmentType NamedEnvironmentType { get; set; }

        #region ISecurableImplementation
        public virtual bool IsSecured { get; set; } = true;
        public virtual Core.Security Security { get; set; }
        public virtual string PrimaryContactId { get; set; }

        public virtual bool AdminDataPermissionsAreExplicit { get; set; }

        public virtual ISecurable Parent { get; set; }
        #endregion

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();
            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The Name of the data flow is required");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string nameIsBlank = "nameIsBlank";
            public const string nameContainsReservedWords = "nameContainsReservedWords";
            public const string nameMustBeUnique = "nameMustBeUnique";
            public const string stepsContainsAtLeastOneSchemaMap = "stepsContainsAtLeastOneSchemaMap";
        }
    }
}
