using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataElement
    {
        public DataElement()
        {
            DataElementDetails = new List<DataElementDetail>();
            DataObjects = new List<DataObject>();
        }

        public virtual IList<DataElementDetail> DataElementDetails { get; set; }

        public virtual IList<DataObject> DataObjects { get; set; }

        public virtual MetadataAsset MetadataAsset { get; set; }

        public virtual int DataElement_ID { get; set; }
        public virtual int DataTag_ID { get; set; }
        public virtual string DataElement_NME { get; set; }
        public virtual string DataElement_DSC { get; set; }
        public virtual string DataElement_CDE { get; set; }
        public virtual string DataElementCode_DSC { get; set; }
        public virtual DateTime DataElementCreate_DTM { get; set; }
        public virtual DateTime DataElementChange_DTM { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual int DataAsset_ID{ get; set; }
        public virtual string BusElementKey { get; set; }
        public virtual DatasetFileConfig DatasetFileConfig { get; set; }
        public virtual string SchemaName
        {
            get
            {
                return GetElementDetail("Schema_NME").DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("Schema_NME", value);
            }
        }
        public virtual string SchemaDescription
        {
            get
            {
                return GetElementDetail("Schema_DSC").DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("Schema_DSC", value);
            }
        }
        public virtual string FileFormat
        {
            get
            {
                return GetElementDetail("FileFormat_TYP").DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("FileFormat_TYP", value);
            }
        }
        public virtual string Delimiter
        {
            get
            {
                DataElementDetail detail = GetElementDetail("Delimiter_TYP");
                return (detail == null) ? null : detail.DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("Delimiter_TYP", value);
            }
        }
        public virtual int SchemaRevision
        {
            get
            {
                return Int32.Parse(GetElementDetail("Revision_CDE").DataElementDetailType_VAL);
            }
            set
            {
                SetDataElementDetailValue("Revision_CDE", value.ToString());
            }
        }
        public virtual Boolean SchemaIsForceMatch
        {
            get
            {
                return Boolean.Parse(GetElementDetail("ForceMatch_IND").DataElementDetailType_VAL);
            }
            set
            {
                SetDataElementDetailValue("ForceMatch_IND", (value) ? "True" : "False");
            }
        }
        public virtual Boolean SchemaIsPrimary
        {
            get
            {
                string a = GetElementDetail("Primary_IND").DataElementDetailType_VAL;
                Boolean x = Boolean.Parse(a);
                //Boolean.Parse(GetElementDetail("Primary_IND").DataElementDetailType_VAL)
                return x;
            }
            set
            {
                SetDataElementDetailValue("Primary_IND", (value) ? "True" : "False");
            }
        }
        public virtual string HiveTable
        {
            get
            {
                DataElementDetail detail = GetElementDetail("HiveTable_NME");
                return (detail == null) ? null : detail.DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("HiveTable_NME", value);
            }
        }
        public virtual string HiveDatabase
        {
            get
            {
                DataElementDetail detail = GetElementDetail("HiveDatabase_NME");
                return (detail == null) ? null : detail.DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("HiveDatabase_NME", value);
            }
        }
        public virtual string HiveTableStatus
        {
            get
            {
                DataElementDetail detail = GetElementDetail("HiveTableStatus");
                return (detail == null) ? null : detail.DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("HiveTableStatus", value);
            }
        }
        public virtual string StorageCode
        {
            get
            {
                DataElementDetail detail = GetElementDetail("Storage_CDE");
                return detail?.DataElementDetailType_VAL;
            }
            set
            {
                SetDataElementDetailValue("Storage_CDE", value);
            }
        }
        public virtual Boolean HasHeader
        {
            get
            {
                DataElementDetail detail = GetElementDetail("HasHeader");
                return (detail == null || detail.DataElementDetailType_VAL.ToLower() == "false" ) ? false : true;
            }
            set
            {
                SetDataElementDetailValue("HasHeader", value.ToString());
            }
        }
        #region DataElementDetailHelpers
        private DataElementDetail GetElementDetail(string typeCDE)
        {
            return DataElementDetails?.Where(w => w.DataElementDetailType_CDE == typeCDE).SingleOrDefault();  
        }

        private void SetDataElementDetailValue(string typeCDE, string val)
        {
            DataElementDetail item = GetElementDetail(typeCDE);
            if (item != null)
            {
                //if we only want to set the tyepCDE value on initialization, then when tyepCDE exists
                // we do nothing with the value. Otherwise, we set it to the incoming value
                switch (typeCDE)
                {
                    case "Storage_CDE":
                        //only set value on initialization
                        break;
                    default:
                        item.DataElementDetailType_VAL = val;
                        break;
                }                
            }
            else
            {
                AddDataElementDetail(typeCDE, val);                
            }
        }

        private void AddDataElementDetail(string typeCDE, string val)
        {
            DataElementDetails.Add(new DataElementDetail()
            {
                DataElement = this,
                DataElementDetailCreate_DTM = DateTime.Now,
                DataElementDetailChange_DTM = DateTime.Now,
                DataElementDetailType_CDE = typeCDE,
                DataElementDetailType_VAL = val,
                LastUpdt_DTM = DateTime.Now
            });
        }
        #endregion 

    }

}
