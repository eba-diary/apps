using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;
using Newtonsoft.Json;
using Sentry.data.Core.GlobalEnums;
using System.Text.RegularExpressions;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class Dataset : IValidatable, ISecurable
    {
        private string _metadata;

        public Dataset() { }

        public virtual int DatasetId { get; set; }

        public virtual string S3Key { get; set; }

        public virtual string DatasetName { get; set; }

        public virtual string ShortName { get; set; }

        public virtual string DatasetDesc { get; set; }

        public virtual string DatasetInformation { get; set; }

        public virtual string CreationUserName { get; set; }

        public virtual string UploadUserName { get; set; }

        public virtual string OriginationCode { get; set; }

        public virtual DateTime DatasetDtm { get; set; }

        public virtual DateTime ChangedDtm { get; set; }

        public virtual bool CanDisplay { get; set; }

        public virtual IList<Category> DatasetCategories { get; set; }
        public virtual IList<BusinessUnit> BusinessUnits { get; set; }
        public virtual IList<DatasetFunction> DatasetFunctions { get; set; }
        public virtual string DatasetType { get; set; }
        public virtual DataClassificationType DataClassification { get; set; }

        public virtual IList<DatasetFile> DatasetFiles { get; set; }

        public virtual IList<DatasetFileConfig> DatasetFileConfigs { get; set; }
        public virtual IList<MetadataTag> Tags { get; set; }
        public virtual DatasetMetadata Metadata
        {
            get
            {
                if (string.IsNullOrEmpty(_metadata))
                {
                    return null;
                }
                else
                {
                    DatasetMetadata a = JsonConvert.DeserializeObject<DatasetMetadata>(_metadata);
                    return a;
                }
            }
            set
            {
                _metadata = JsonConvert.SerializeObject(value);
            }
        }
        public virtual IList<Favorite> Favorities { get; set; }

        public virtual List<DatasetScopeType> DatasetScopeType()
        {
            return DatasetFileConfigs.Select(x => x.DatasetScopeType).GroupBy(x => x.Name).Select(x => x.First()).ToList();
        }

        public virtual IList<Image> Images { get; set; }
        public virtual string NamedEnvironment { get; set; }
        public virtual NamedEnvironmentType NamedEnvironmentType { get; set; }
        public virtual Asset Asset { get; set; }

         #region Security

        public virtual string PrimaryContactId { get; set; }
        public virtual bool IsSecured { get; set; }
        public virtual Security Security { get; set; }

        /// <summary>
        /// AdminDataPermissionsAreExplicit is true for Datasets within 
        /// Human Resource category
        /// </summary>
        public virtual bool AdminDataPermissionsAreExplicit
        {
            get
            {
                return DatasetType == GlobalConstants.DataEntityCodes.DATASET && DatasetCategories.Any(z => z.Name == "Human Resources");
            }
        }
        public virtual ISecurable Parent { get => Asset; }

        #endregion

        //Delete Implementation
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual bool DeleteInd { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }

        public virtual string AlternateContactEmail { get; set; }

        public virtual bool IsHumanResources { 
            get
            {
                return DatasetCategories.Any(w => w.AbbreviatedName == "HR");
            }
        }

        public virtual ValidationResults ValidateForDelete()
        {
            ValidationResults results = new ValidationResults();
            return results;
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (DatasetCategories == null || DatasetCategories.Count == 0)
            {
                vr.Add(ValidationErrors.datasetCategoryRequired, "The Dataset Category is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetName))
            {
                vr.Add(GlobalConstants.ValidationErrors.NAME_IS_BLANK, "The Dataset Name is required");
            }
            if (string.IsNullOrWhiteSpace(CreationUserName))
            {
                vr.Add(ValidationErrors.datasetCreatedByRequired, "The Dataset Creation User Name is required");
            }
            if (string.IsNullOrWhiteSpace(UploadUserName))
            {
                vr.Add(ValidationErrors.datasetUploadedByRequired, "The Dataset Upload User Name is required");
            }
            if (DatasetDtm < new DateTime(1800, 1, 1)) // null dates are ancient; this suffices to check for null dates
            {
                vr.Add(ValidationErrors.datasetDateRequired, "The Dataset Date is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetDesc))
            {
                vr.Add(ValidationErrors.datasetDescriptionRequired, "The Dataset description is required");
            }

            // Validations that only apply to Datasets
            if (DatasetType == GlobalConstants.DataEntityCodes.DATASET)
            {
                if (Asset == null)
                {
                    vr.Add(GlobalConstants.ValidationErrors.SAID_ASSET_REQUIRED, "The SAID Asset is required");
                }
                if (string.IsNullOrWhiteSpace(ShortName))
                {
                    vr.Add(ValidationErrors.datasetShortNameRequired, "Short Name is required");
                }
                if (ShortName != null && new Regex(@"[^0-9a-zA-Z]").Match(ShortName).Success)
                {
                    vr.Add(ValidationErrors.datasetShortNameInvalid, "Short Name can only contain alphanumeric characters");
                }
                if (ShortName != null && ShortName.Length > 12)
                {
                    vr.Add(ValidationErrors.datasetShortNameInvalid, "Short Name must be 12 characters or less");
                }
                if (ShortName == SecurityConstants.ASSET_LEVEL_GROUP_NAME)
                {
                    vr.Add(ValidationErrors.datasetShortNameInvalid, $"Short Name cannot be \"{SecurityConstants.ASSET_LEVEL_GROUP_NAME}\"");
                }
            }

            // Validations that only apply to Reports
            if (DatasetType == GlobalConstants.DataEntityCodes.REPORT && string.IsNullOrWhiteSpace(Metadata.ReportMetadata.Location))
            {
                vr.Add(ValidationErrors.datasetLocationRequired, "Report Location is required");
            }

            return vr;
        }

        public static class ValidationErrors
        {
            public const string datasetNameDuplicate = "datasetNameDuplicate";
            public const string datasetNameRequired = "datasetNameRequired";
            public const string datasetNameIdempotent = "datasetNameIdempotent";
            public const string datasetShortNameRequired = "datasetShortNameIsBlank";
            public const string datasetShortNameInvalid = "datasetShortNameIsInvalid";
            public const string datasetShortNameDuplicate = "datasetShortNameIsDuplicate";
            public const string datasetShortNameIdempotent = "datasetShortNameIsIdempotent";
            public const string datasetDescriptionRequired = "datasetDescriptionRequired";
            public const string datasetOwnerRequired = "datasetOwnerRequired";
            public const string datasetOwnerInvalid = "datasetOwnerInvalid";
            public const string datasetCreatedByRequired = "datasetCreatedByRequired";
            public const string datasetUploadedByRequired = "datasetUploadedByRequired";
            public const string datasetContactRequired = "datasetContactRequired";
            public const string datasetCategoryRequired = "datasetCategoryRequired";
            public const string datasetScopeRequired = "datasetScopeRequired";
            public const string datasetDateRequired = "datasetDateRequired";
            public const string datasetLocationRequired = "datasetLocationRequired";
            public const string datasetOriginationRequired = "datasetOriginationRequired";
            public const string datasetAlternateContactEmailFormatInvalid = "datasetAlternateContactEmailFormatInvalid";
        }
    }
}
