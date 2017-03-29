using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Core;
using Sentry.Common;
using static Sentry.Common.SystemClock;

namespace Sentry.data.Core
{
    public class DatasetFileGroup : IValidatable
    {
        // This class represents a logical file; a file may have several physical versions of available in various types:
        // For example:
        //
        //     DatasetFileGroup:    ClaimData
        //
        //     DatasetFileVersion:  ClaimData_2016.csv         \  
        //     DatasetFileVersion:  ClaimData_2016.sas7bdat     \  these are all different versions of the same file
        //     DatasetFileVersion:  ClaimData_2015.csv          /
        //     DatasetFileVersion:  ClaimData_2015.sas7bdat    /
        private string _ETag;
        private string _key;
        private string _bucket;
        private string _name;
        private string _summaryDescription;
        private string _detailDescription;
        private string _fullPath;
        private Dictionary<string, string> _metadata;

        protected DatasetFileGroup()
        {
        }

        public DatasetFileGroup(string eTag,
                       string key,
                       string bucket,
                       string name,
                       string summaryDescription,
                       string detailDescription,
                       string fullPath,
                       Dictionary<string, string> metadata)
        {
            this._ETag = eTag;
            this._key = key;
            this._bucket = bucket;
            this._name = name;
            this._summaryDescription = summaryDescription;
            this._detailDescription = detailDescription;
            this._fullPath = fullPath;
            this._metadata = metadata;
        }

        public virtual string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        public virtual string Bucket
        {
            get
            {
                return _bucket;
            }
            set
            {
                _bucket = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual string SummaryDescription
        {
            get
            {
                return _summaryDescription;
            }
            set
            {
                _summaryDescription = value;
            }
        }

        public virtual string DetailDescription
        {
            get
            {
                return _detailDescription;
            }
            set
            {
                _detailDescription = value;
            }
        }

        public virtual string FullPath
        {
            get
            {
                return _fullPath;
            }
            set
            {
                _fullPath = value;
            }
        }

        public virtual string ETag
        {
            get
            {
                return _ETag;
            }
        }

        public virtual int Id { get; }

        public virtual int Version { get; }

        public virtual Dictionary<string, string> Metadata
        {
            get
            {
                return _metadata;
            }
            set
            {
                _metadata = value;
            }

        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();
            if (string.IsNullOrWhiteSpace(Key))
            {
                vr.Add(ValidationErrors.keyIsBlank, "The Data Set Key is required");
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The Data Set Name is required");
            }
            if (string.IsNullOrWhiteSpace(SummaryDescription))
            {
                vr.Add(ValidationErrors.summaryDescriptionIsBlank, "The Data Set Summary Description is required");
            }
            if (string.IsNullOrWhiteSpace(DetailDescription))
            {
                vr.Add(ValidationErrors.detailDescriptionIsBlank, "The Data Set Detail Description is required");
            }
            if (string.IsNullOrWhiteSpace(FullPath))
            {
                vr.Add(ValidationErrors.fullPathIsBlank, "The Data Set Full Path must exist");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string keyIsBlank = "keyIsBlank";
            public const string bucketIsBlank = "bucketIsBlank";
            public const string nameIsBlank = "nameIsBlank";
            public const string summaryDescriptionIsBlank = "summaryDescriptionIsBlank";
            public const string detailDescriptionIsBlank = "detailDescriptionIsBlank";
            public const string fullPathIsBlank = "fullPathIsBlank";
        }
    }
}
