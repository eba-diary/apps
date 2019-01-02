using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Core;
using Sentry.Common;
using static Sentry.Common.SystemClock;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public class Dataset : IValidatable
    {
        private string _metadata;

        public Dataset(){ }

        public virtual int DatasetId { get; set; }
        public virtual string Category { get; set; }

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

        public virtual Category DatasetCategory { get; set; }
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

            if (string.IsNullOrWhiteSpace(Category))
            {
                vr.Add(ValidationErrors.categoryIsBlank, "The Dataset Category is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetName))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The Dataset Name is required");
            }
            if (string.IsNullOrWhiteSpace(CreationUserName))
            {
                vr.Add(ValidationErrors.creationUserNameIsBlank, "The Dataset Creation User Name is required");
            }
            if (string.IsNullOrWhiteSpace(UploadUserName))
            {
                vr.Add(ValidationErrors.uploadUserNameIsBlank, "The Dataset UPload User Name is required");
            }
            if (!Regex.IsMatch(SentryOwnerName, "(^[0-9]{6,6}$)"))
            {
                vr.Add(ValidationErrors.sentryOwnerIsNotNumeric, "The Sentry Owner ID should contain owners Sentry ID");
            }
            if (DatasetDtm < new DateTime(1800, 1, 1)) // null dates are ancient; this suffices to check for null dates
            {
                vr.Add(ValidationErrors.datasetDateIsOld, "The Dataset Date is required");
            }
            if (string.IsNullOrWhiteSpace(DatasetDesc))
            {
                vr.Add(ValidationErrors.datasetDescIsBlank, "The Dataset description is required");
            }

            //Report specific checks
            if(!string.IsNullOrEmpty(DatasetType) && DatasetType == "RPT")
            {
                if (string.IsNullOrWhiteSpace(Metadata.ReportMetadata.Location))
                {
                    vr.Add(ValidationErrors.locationIsBlank, "Report Location is required");
                }
            }
            
            return vr;
        }

        public class ValidationErrors
        {
            public const string s3keyIsBlank = "keyIsBlank";
            public const string categoryIsBlank = "categoryIsBlank";
            public const string nameIsBlank = "nameIsBlank";
            public const string creationUserNameIsBlank = "creationUserNameIsBlank";
            public const string uploadUserNameIsBlank = "uploadUserNameIsBlank";
            public const string datasetDateIsOld = "datasetDateIsOld";
            public const string datasetDescIsBlank = "descIsBlank";
            public const string sentryOwnerIsNotNumeric = "sentryOwnerIsNotNumeric";
            public const string numberOfFilesIsNegative = "numberOfFilesIsNegative";

            //Report specific validation errors
            public const string locationIsBlank = "locationIsBlank";
            public const string locationIsInvalid = "locationIsInvalid";
        }
    }
}
