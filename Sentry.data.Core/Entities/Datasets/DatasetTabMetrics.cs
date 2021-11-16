using Sentry.Core;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class DatasetTabMetrics : IValidatable
    {
        public DatasetTabMetrics() { }

        public virtual int UserId { get; set; }

        public virtual string DatasetId { get; set; }

        public virtual string SchemaAboutClicks { get; set; }

        public virtual string SchemaColumnsClicks { get; set; }

        public virtual string DataFilesClicks { get; set; }

        public virtual string DataPreviewClicks { get; set; }

        public virtual ValidationResults ValidateForDelete()
        {
            ValidationResults results = new ValidationResults();
            return results;
        }

        public virtual ValidationResults ValidateForSave()
        {
            
            ValidationResults vr = new ValidationResults();
            /*
            if (DatasetCategories == null || DatasetCategories.Count == 0)
            {
                vr.Add(ValidationErrors.datasetCategoryRequired, "The Dataset Category is required");
            }
            if (string.IsNullOrWhiteSpace(SAIDAssetKeyCode))
            {
                vr.Add(GlobalConstants.ValidationErrors.SAID_ASSET_REQUIRED, "The SAID Asset is required");
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
            if (!Regex.IsMatch(PrimaryOwnerId, "(^[0-9]{6,6}$)"))
            {
                vr.Add(ValidationErrors.datasetOwnerInvalid, "The Sentry Owner ID should contain owners Sentry ID");
            }
            if (DatasetDtm < new DateTime(1800, 1, 1)) // null dates are ancient; this suffices to check for null dates
            {
                vr.Add(ValidationErrors.datasetDateRequired, "The Dataset Date is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetDesc))
            {
                vr.Add(ValidationErrors.datasetDescriptionRequired, "The Dataset description is required");
            }

            //Report specific checks
            if (DatasetType == GlobalConstants.DataEntityCodes.REPORT && string.IsNullOrWhiteSpace(Metadata.ReportMetadata.Location))
            {
                vr.Add(ValidationErrors.datasetLocationRequired, "Report Location is required");
            }
            */
            return vr;
        }

        public static class ValidationErrors
        {

        }
    }
}
