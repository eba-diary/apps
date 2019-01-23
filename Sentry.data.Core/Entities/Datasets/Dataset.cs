using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public class Dataset : IValidatable
    {
        private string _metadata;

        public Dataset(){ }

        public virtual int DatasetId { get; set; }

        public virtual Boolean IsSensitive { get; set; }

        public virtual string S3Key { get; set; }

        public virtual string DatasetName { get; set; }

        public virtual string DatasetDesc { get; set; }

        public virtual string DatasetInformation { get; set; }

        public virtual string CreationUserName { get; set; }

        public virtual string SentryOwnerName { get; set; }

        public virtual string UploadUserName { get; set; }

        public virtual string OriginationCode { get; set; }

        public virtual DateTime DatasetDtm { get; set; }

        public virtual DateTime ChangedDtm { get; set; }

        public virtual Boolean CanDisplay { get; set; }

        public virtual IList<Category> DatasetCategories { get; set; }
        public virtual string DatasetType { get; set; }

        public virtual IList<DatasetFile> DatasetFiles { get; set; }

        public virtual IList<DatasetFileConfig> DatasetFileConfigs { get; set; }
        public virtual IList<MetadataTag> Tags { get; set; }
        public virtual DatasetMetadata Metadata
        {
            get
            {
                if (String.IsNullOrEmpty(_metadata))
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

        public virtual List<DatasetScopeType> DatasetScopeType
        {
            get
            {
                return DatasetFileConfigs.Select(x => x.DatasetScopeType).GroupBy(x => x.Name).Select(x => x.First()).ToList();
            }
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (DatasetCategories == null || DatasetCategories.Count == 0)
            {
                vr.Add(GlobalConstants.ValidationErrors.CATEGORY_IS_BLANK, "The Dataset Category is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetName))
            {
                vr.Add(GlobalConstants.ValidationErrors.NAME_IS_BLANK, "The Dataset Name is required");
            }
            if (string.IsNullOrWhiteSpace(CreationUserName))
            {
                vr.Add(GlobalConstants.ValidationErrors.CREATION_USER_NAME_IS_BLANK, "The Dataset Creation User Name is required");
            }
            if (string.IsNullOrWhiteSpace(UploadUserName))
            {
                vr.Add(GlobalConstants.ValidationErrors.UPLOAD_USER_NAME_IS_BLANK, "The Dataset UPload User Name is required");
            }
            if (!Regex.IsMatch(SentryOwnerName, "(^[0-9]{6,6}$)"))
            {
                vr.Add(GlobalConstants.ValidationErrors.SENTRY_OWNER_IS_NOT_NUMERIC, "The Sentry Owner ID should contain owners Sentry ID");
            }
            if (DatasetDtm < new DateTime(1800, 1, 1)) // null dates are ancient; this suffices to check for null dates
            {
                vr.Add(GlobalConstants.ValidationErrors.DATASET_DATE_IS_OLD, "The Dataset Date is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetDesc))
            {
                vr.Add(GlobalConstants.ValidationErrors.DATASET_DESC_IS_BLANK, "The Dataset description is required");
            }

            //Report specific checks
            if (DatasetType == GlobalConstants.DataEntityTypes.REPORT && string.IsNullOrWhiteSpace(Metadata.ReportMetadata.Location))
            {
                vr.Add(GlobalConstants.ValidationErrors.LOCATION_IS_BLANK, "Report Location is required");
            }

            return vr;
        }


    }
}
