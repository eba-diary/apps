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
    public class DatasetMetadata : IValidatable
    {
#pragma warning disable CS0649
        private int _datasetMetadataId;
#pragma warning restore CS0649
        private int _datasetId;
        private bool _isColumn;
        private string _name;
        private string _value;
        private Dataset _parent;

        protected DatasetMetadata()
        {
        }

        public DatasetMetadata(int datasetMetadataId,
                               int datasetId,
                               bool isColumn,
                               string name,
                               string value,
                               Dataset parent)

        {
            this._datasetMetadataId = datasetMetadataId;
            this._datasetId = datasetId;
            this._isColumn = isColumn;
            this._name = name;
            this._value = value;
            this._parent = parent;
        }

        public virtual int DatasetMetadataId
        {
            get
            {
                return _datasetMetadataId;
            }
            set
            {
                _datasetMetadataId = value;
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

        public virtual bool IsColumn
        {
            get
            {
                return _isColumn;
            }
            set
            {
                _isColumn = value;
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

        public virtual string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public virtual Dataset Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();
            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The name is required");
            }
            if (string.IsNullOrWhiteSpace(Value))
            {
                vr.Add(ValidationErrors.valueIsBlank, "The value is required");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string nameIsBlank = "nameIsBlank";
            public const string valueIsBlank = "valueIsBlank";
        }
    }
}
