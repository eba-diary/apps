using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public static class HiveExtensions
    {
        //public static HiveTableCreateModel ToHiveTableCreateMsg(this DataElement dataElement)
        //{
        //    HiveTableCreateModel hiveCreate = new HiveTableCreateModel();

        //    Sentry.data.Core.SchemaModel sm = new Sentry.data.Core.SchemaModel();
        //    sm.SchemaID = dataElement.StorageCode;
        //    sm.Format = dataElement.FileFormat;
        //    sm.Header = "true";
        //    sm.Delimiter = dataElement.Delimiter;
        //    sm.HiveDatabase = dataElement.HiveDatabase;
        //    sm.HiveTable = dataElement.HiveTable;

        //    DataObject dObj = dataElement.DataObjects.FirstOrDefault();

        //    List<ColumnModel> ColumnModelList = new List<ColumnModel>();

        //    if (dObj != null)
        //    {
        //        foreach(DataObjectField dof in dObj.DataObjectFields)
        //        {
        //            ColumnModel cm = new ColumnModel();
        //            cm.Name = dof.DataObjectField_NME;
        //            cm.DataType = dof.DataType;
        //            cm.Nullable = dof.Nullable;
        //            cm.Length = dof.Length;
        //            cm.Precision = dof.Precision;
        //            cm.Scale = dof.Scale;

        //            ColumnModelList.Add(cm);
        //        }
        //    }

        //    sm.Columns = ColumnModelList;

        //    hiveCreate.Schema = sm;

        //    return hiveCreate;
        //}
    }
}