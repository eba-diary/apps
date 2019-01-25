using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;

namespace Sentry.data.Web.Extensions
{
    public static class ExtensionMethods
    {
        //public static HiveTableCreateModel ToHiveTableCreateMsg(this DataElement dataElement)
        //{
        //    HiveTableCreateModel hiveCreate = new HiveTableCreateModel();

        //    SchemaModel sm = new SchemaModel();
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
        //            cm.Nullable = dof.Nullable.ToString();
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