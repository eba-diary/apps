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
#pragma warning disable CS0649
        private int _scopeTypeId;
#pragma warning restore CS0649
        private string _name;
        private string _typeDescription;
        private Boolean _isEnabled;

        protected DatasetScopeType() { }

        public DatasetScopeType(
                string name,
                string typeDescription,
                Boolean isEnabled)
        {
            this._scopeTypeId = 0; //defaulting to zero for new
            this._name = name;
            this._typeDescription = typeDescription;
            this._isEnabled = isEnabled;
        }

        public virtual int ScopeTypeId
        {
            get
            {
                return _scopeTypeId;
            }
            set
            {
                _scopeTypeId = value;
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
        public virtual string Description
        {
            get
            {
                return _typeDescription;
            }
            set
            {
                _typeDescription = value;
            }
        }
        public virtual Boolean IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
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