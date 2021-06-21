using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataObject
    {
        public DataObject()
        {
            DataObjectDetails = new List<DataObjectDetail>();
            DataObjectFields = new List<DataObjectField>();
        }

        public virtual IList<DataObjectDetail> DataObjectDetails { get; set; }

        public virtual IList<DataObjectField> DataObjectFields { get; set; }

        public virtual int DataObject_ID { get; set; }

        //public virtual DataElement DataElement { get; set; }

        public virtual int DataElement_ID { get; set; }
        public virtual int DataTag_ID { get; set; }
        public virtual int Reviewer_ID { get; set; }
        public virtual string DataObject_NME { get; set; }
        public virtual string DataObject_DSC { get; set; }
        public virtual int DataObjectParent_ID { get; set; }
        public virtual string DataObject_CDE { get; set; }
        public virtual string DataObjectCode_DSC { get; set; }
        public virtual DateTime DataObjectCreate_DTM { get; set; }
        public virtual DateTime DataObjectChange_DTM { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusElementKey { get; set; }
        public virtual string BusObjectKey { get; set; }
        public virtual Int64 RowCount
        {
            get
            {
                DataObjectDetail detail = GetObjectDetail("Row_CNT");
                return (detail == null) ? 0 : Int64.Parse(detail.DataObjectDetailType_VAL);
            }
            set
            {
                SetDataGetObjectDetailValue("Row_CNT", value.ToString());
            }
        }

        #region DataElementDetailHelpers
        private DataObjectDetail GetObjectDetail(string typeCDE)
        {
            return DataObjectDetails.Where(w => w.DataObjectDetailType_CDE == typeCDE).SingleOrDefault();
        }

        private void SetDataGetObjectDetailValue(string typeCDE, string val)
        {
            DataObjectDetail item = GetObjectDetail(typeCDE);
            if (item != null)
            {
                item.DataObjectDetailType_VAL = val;
            }
            else
            {
                AddDataGetObjectDetail(typeCDE, val);
            }
        }

        private void AddDataGetObjectDetail(string typeCDE, string val)
        {
            DataObjectDetails.Add(new DataObjectDetail()
            {
                DataObject = this,
                DataObjectDetailCreate_DTM = DateTime.Now,
                DataObjectDetailChange_DTM = DateTime.Now,
                DataObjectDetailType_CDE = typeCDE,
                DataObjectDetailType_VAL = val,
                LastUpdt_DTM = DateTime.Now
            });
        }
        #endregion 
    }
}
