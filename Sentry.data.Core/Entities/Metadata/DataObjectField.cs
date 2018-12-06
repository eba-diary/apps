using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class DataObjectField
    {
        public DataObjectField()
        {
            DataObjectFieldDetails = new List<DataObjectFieldDetail>();
        }

        public virtual IList<DataObjectFieldDetail> DataObjectFieldDetails { get; set; }

        public virtual int DataObjectField_ID { get; set; }
        public virtual int DataObject_ID { get; set; }
        public virtual DataObject DataObject { get; set; }

        public virtual int DataTag_ID { get; set; }
        public virtual string DataObjectField_NME { get; set; }
        public virtual string DataObjectField_DSC { get; set; }
        public virtual DateTime DataObjectFieldCreate_DTM { get; set; }
        public virtual DateTime DataObjectFieldChange_DTM { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusObjectKey { get; set; }
        public virtual string BusFieldKey { get; set; }
        public virtual string DataType
        {
            get
            {
                DataObjectFieldDetail detail = GetFieldDetail("Datatype_TYP");
                return (detail == null) ? null : detail.DataObjectFieldDetailType_VAL;
            }
            set
            {
                SetDataGetFieldDetailValue("Datatype_TYP", value);
            }
        }
        public virtual string Length
        {
            get
            {
                DataObjectFieldDetail detail = GetFieldDetail("Length_AMT");
                return (detail == null) ? null : detail.DataObjectFieldDetailType_VAL;
            }
            set
            {
                SetDataGetFieldDetailValue("Length_AMT", value);
            }
        }
        public virtual string OrdinalPosition
        {
            get
            {
                DataObjectFieldDetail detail = GetFieldDetail("OrdinalPosition_CDE");
                return (detail == null) ? null : detail.DataObjectFieldDetailType_VAL;
            }
            set
            {
                SetDataGetFieldDetailValue("OrdinalPosition_CDE", value);
            }
        }        
        public virtual string Precision
        {
            get
            {
                DataObjectFieldDetail detail = GetFieldDetail("Precision_AMT");
                return (detail == null) ? null : detail.DataObjectFieldDetailType_VAL;
            }
            set
            {
                SetDataGetFieldDetailValue("Precision_AMT", value);
            }
        }        
        public virtual string Scale
        {
            get
            {
                DataObjectFieldDetail detail = GetFieldDetail("Scale_AMT");
                return (detail == null) ? null : detail.DataObjectFieldDetailType_VAL;
            }
            set
            {
                SetDataGetFieldDetailValue("Scale_AMT", value);
            }
        }        
        public Boolean? Nullable
        {
            get
            {
                DataObjectFieldDetail detail = GetFieldDetail("Nullable_IND");
                if (detail == null || detail.DataObjectFieldDetailType_VAL == null)
                {
                    return null;
                }
                else
                {
                    return Boolean.Parse(GetFieldDetail("Nullable_IND").DataObjectFieldDetailType_VAL);
                }                
            }
            set
            {
                if (value == null)
                {
                    SetDataGetFieldDetailValue("Nullable_IND", null);
                }
                else
                {
                    SetDataGetFieldDetailValue("Nullable_IND", ((bool)value) ? "True" : "False");
                }                
            }
        }
        
        #region DataElementDetailHelpers
        private DataObjectFieldDetail GetFieldDetail(string typeCDE)
        {
            DataObjectFieldDetail result = DataObjectFieldDetails.Where(w => w.DataObjectFieldDetailType_CDE == typeCDE).SingleOrDefault();
            return result;
        }

        private void SetDataGetFieldDetailValue(string typeCDE, string val)
        {
            DataObjectFieldDetail item = GetFieldDetail(typeCDE);
            if (item != null)
            {
                item.DataObjectFieldDetailType_VAL = val;
            }
            else
            {
                AddDataFieldDetail(typeCDE, val);
            }
        }

        private void AddDataFieldDetail(string typeCDE, string val)
        {
            DataObjectFieldDetails.Add(new DataObjectFieldDetail()
            {
                DataObjectField = this,
                DataObjectFieldDetailCreate_DTM = DateTime.Now,
                DataObjectFieldDetailChange_DTM = DateTime.Now,
                DataObjectFieldDetailType_CDE = typeCDE,
                DataObjectFieldDetailType_VAL = val,
                LastUpdt_DTM = DateTime.Now
            });
        }
        #endregion 
    }
}
