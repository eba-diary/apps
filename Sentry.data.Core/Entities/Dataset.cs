using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Core;
using Sentry.Common;
using static Sentry.Common.SystemClock;
using System.Text.RegularExpressions;

namespace Sentry.data.Core
{
    public class Dataset : IValidatable
    {
#pragma warning disable CS0649
        private int _datasetId;
#pragma warning restore CS0649
        private string _category;
        private string _datasetName;
        private string _datasetDesc;
        //private string _creationUserId;
        private string _creationUserName;
        //private string _sentryOwnerId;
        private string _sentryOwnerName;
        private string _uploadUserName;
        private string _originationCode;
        //private string _fileExtension;
        private DateTime _datasetDtm;
        private DateTime _changedDtm;
        //private string _creationFreqCode;
        private string _creationFreqDesc;
        private string _s3key;
        private Boolean _IsSensitive;
        private Boolean _canDisplay;
        IList<DatasetMetadata> _rawMetadata;
        private Category _datasetCategory;
        private IList<DatasetFile> _datasetFile;
        private DatasetScopeType _datasetScopeType;
        private int _datafilesToKeep;
        private IList<DatasetFileConfig> _datasetFileConfigs;
        //private IDictionary<string, string> _columns;
        //private IDictionary<string, string> _metadata;
        private string _dropLocation;

        public Dataset()
        {
        }

        public Dataset(int datasetId,
                       string category, 
                       string datasetName,
                       string datasetDesc,
                       //string creationUserId,
                       string creationUserName,
                       //string sentryOwnerId,
                       string sentryOwnerName,
                       string uploadUserName,
                       string originationCode,
                       //string fileExtension,
                       DateTime datasetDtm,
                       DateTime changedDtm,
                       //string creationFreqCode,
                       string creationFreqDesc,
                       string s3key,
                       Boolean IsSensitive,
                       Boolean CanDisplay,
                       IList<DatasetMetadata> rawMetadata,
                       Category cat,
                       IList<DatasetFile> datasetFile,
                       DatasetScopeType datasetscopetype,
                       int datafilesToKeep,
                       IList<DatasetFileConfig> datasetFileConfigs,
                       string dropLocation)
        {
            this._datasetId = datasetId;
            this._category = category;
            this._datasetName = datasetName;
            this._datasetDesc = datasetDesc;
            //this._creationUserId = creationUserId;
            this._creationUserName = creationUserName;
            //this._sentryOwnerId = sentryOwnerId;
            this._sentryOwnerName = sentryOwnerName;
            this._uploadUserName = uploadUserName;
            this._originationCode = originationCode;
            //this._fileExtension = fileExtension;
            this._datasetDtm = datasetDtm;
            this._changedDtm = changedDtm;
            //this._creationFreqCode = creationFreqCode;
            this._creationFreqDesc = creationFreqDesc;
            this._s3key = s3key;
            this._IsSensitive = IsSensitive;
            this._canDisplay = CanDisplay;
            this._rawMetadata = rawMetadata;
            this._datasetCategory = cat;
            this._datasetFile = datasetFile;
            this._datasetScopeType = datasetscopetype;
            this._datafilesToKeep = datafilesToKeep;
            this._datasetFileConfigs = datasetFileConfigs;
            this._dropLocation = dropLocation;
        }

        public virtual Boolean IsSensitive
        {
            get
            {
                return _IsSensitive;
            }
            set
            {
                _IsSensitive = value;
            }
        }

        public virtual int DatasetId
        {
            get
            {
                return _datasetId;
            }
            set
            {
                _datasetId = value;
            }
        }

        public virtual string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
            }
        }

        public virtual string S3Key
        {
            get
            {
                return _s3key;
            }
            set
            {
                _s3key = value;
            }
        }

        public virtual string DatasetName
        {
            get
            {
                return _datasetName;
            }
            set
            {
                _datasetName = value;
            }
        }

        public virtual string DatasetDesc
        {
            get
            {
                return _datasetDesc;
            }
            set
            {
                _datasetDesc = value;
            }
        }

        public virtual string CreationUserName
        {
            get
            {
                return _creationUserName;
            }
            set
            {
                _creationUserName = value;
            }
        }

        public virtual string SentryOwnerName
        {
            get
            {
                return _sentryOwnerName;
            }
            set
            {
                _sentryOwnerName = value;
            }
        }

        public virtual string UploadUserName
        {
            get
            {
                return _uploadUserName;
            }
            set
            {
                _uploadUserName = value;
            }
        }

        public virtual string OriginationCode
        {
            get
            {
                return _originationCode;
            }
            set
            {
                _originationCode = value;
            }
        }

        public virtual string FileExtension
        {
            get
            {
                var extension = System.IO.Path.GetExtension(this.S3Key);

                return extension;
            }

        }

        public virtual DateTime DatasetDtm
        {
            get
            {
                return _datasetDtm;
            }
            set
            {
                _datasetDtm = value;
            }
        }

        public virtual DateTime ChangedDtm
        {
            get
            {
                return _changedDtm;
            }
            set
            {
                _changedDtm = value;
            }
        }

        public virtual string CreationFreqDesc
        {
            get
            {
                return _creationFreqDesc;
            }
            set
            {
                _creationFreqDesc = value;
            }
        }

        public virtual Boolean CanDisplay
        {
            get
            {
                return _canDisplay;
            }
            set
            {
                _canDisplay = value;
            }
        }

        public virtual IList<DatasetMetadata> RawMetadata
        {
            get
            {
                return _rawMetadata;
            }
            set
            {
                _rawMetadata = value;
            }
        }

        public virtual IList<DatasetMetadata> Metadata
        {
            get
            {
                return _rawMetadata.Where((x) => x.IsColumn == false).ToList();
            }
        }

        public virtual IList<DatasetMetadata> Columns
        {
            get
            {
                return _rawMetadata.Where((x) => x.IsColumn == true).ToList();
            }
        }

        public virtual Category DatasetCategory
        {
            get
            {
                return _datasetCategory;
            }

            set
            {
                _datasetCategory = value;
            }
        }

        public virtual IList<DatasetFile> DatasetFiles
        {
            get
            {
                return _datasetFile;
            }
            set
            {
                _datasetFile = value;
            }
        }
        public virtual DatasetScopeType DatasetScopeType
        {
            get
            {
                return _datasetScopeType;
            }
            set
            {
                _datasetScopeType = value;
            }
        }
        public virtual int DatafilesToKeep
        {
            get
            {
                return _datafilesToKeep;
            }
            set
            {
                _datafilesToKeep = value;
            }
        }

        public virtual IList<DatasetFileConfig> DatasetFileConfigs
        {
            get
            {
                return _datasetFileConfigs;
            }
            set
            {
                _datasetFileConfigs = value;
            }
        }

        public virtual string DropLocation
        {
            get { return _dropLocation; }
            set { _dropLocation = value; }
        }


        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (string.IsNullOrWhiteSpace(S3Key))
            {
                vr.Add(ValidationErrors.s3keyIsBlank, "The Dataset S3 Key is required");
            }
            if (string.IsNullOrWhiteSpace(Category))
            {
                vr.Add(ValidationErrors.categoryIsBlank, "The Dataset Category is required");
            }
            //if (!string.IsNullOrWhiteSpace(S3Key) && !string.IsNullOrWhiteSpace(Category) && Category != S3Key.Substring(0,S3Key.IndexOf("/")).Replace("/",""))
            //{
            //    vr.Add(ValidationErrors.s3KeyCategoryMismatch, "The Dataset Category does not match first portion of S3 Key");
            //}
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
            if (DatafilesToKeep < 0)
            {
                vr.Add(ValidationErrors.numberOfFilesIsNegative, "Number of Files to Keep cannot be negative");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string s3keyIsBlank = "keyIsBlank";
            public const string categoryIsBlank = "categoryIsBlank";
            //public const string s3KeyCategoryMismatch = "s3KeyCategoryMismatch";
            public const string nameIsBlank = "nameIsBlank";
            public const string creationUserNameIsBlank = "creationUserNameIsBlank";
            public const string uploadUserNameIsBlank = "uploadUserNameIsBlank";
            public const string datasetDateIsOld = "datasetDateIsOld";
            public const string datasetDescIsBlank = "descIsBlank";
            public const string sentryOwnerIsNotNumeric = "sentryOwnerIsNotNumeric";
            public const string numberOfFilesIsNegative = "numberOfFilesIsNegative";
        }
    }
}
