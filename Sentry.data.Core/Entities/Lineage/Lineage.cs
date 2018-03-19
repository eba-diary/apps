using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Lineage : IValidatable
    {
        public Lineage()
        {

        }

        private int _id;
        private int _dataAsset_ID;

        private String _dataElement_NME;
        private String _dataObject_NME;
        private String _dataObjectCode_DSC;
        private String _dataObjectDetailType_VAL;
        private String _dataObjectField_NME;

        private String _sourceElement_NME;
        private String _sourceObject_NME;
        private String _sourceObjectField_NME;
        private String _source_TXT;
        private String _transformation_TXT;
        private String _display_IND;

        public virtual int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }


        public virtual int DataAsset_ID
        {
            get
            {
                return _dataAsset_ID;
            }
            set
            {
                _dataAsset_ID = value;
            }
        }


        public virtual String DataElement_NME
        {
            get
            {
                return _dataElement_NME;
            }
            set
            {
                _dataElement_NME = value;
            }
        }

        public virtual String DataObject_NME
        {
            get
            {
                return _dataObject_NME;
            }
            set
            {
                _dataObject_NME = value;
            }
        }

        public virtual String DataObjectCode_DSC
        {
            get
            {
                return _dataObjectCode_DSC;
            }
            set
            {
                _dataObjectCode_DSC = value;
            }
        }

        public virtual String DataObjectDetailType_VAL
        {
            get
            {
                return _dataObjectDetailType_VAL;
            }
            set
            {
                _dataObjectDetailType_VAL = value;
            }
        }

        public virtual String DataObjectField_NME
        {
            get
            {
                return _dataObjectField_NME;
            }
            set
            {
                _dataObjectField_NME = value;
            }
        }

        public virtual String SourceElement_NME
        {
            get
            {
                return _sourceElement_NME;
            }
            set
            {
                _sourceElement_NME = value;
            }
        }


        public virtual String SourceObject_NME
        {
            get
            {
                return _sourceObject_NME;
            }
            set
            {
                _sourceObject_NME = value;
            }
        }

        public virtual String SourceObjectField_NME
        {
            get
            {
                return _sourceObjectField_NME;
            }
            set
            {
                _sourceObjectField_NME = value;
            }
        }

        public virtual String Source_TXT
        {
            get
            {
                return _source_TXT;
            }
            set
            {
                _source_TXT = value;
            }
        }

        public virtual String Transformation_TXT
        {
            get
            {
                return _transformation_TXT;
            }
            set
            {
                _transformation_TXT = value;
            }
        }

        public virtual String Display_IND
        {
            get
            {
                return _display_IND;
            }
            set
            {
                _display_IND = value;
            }
        }


        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            return new ValidationResults();
        }
    }
}
