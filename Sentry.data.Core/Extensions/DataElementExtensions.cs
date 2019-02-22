using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class DataElementExtensions
    {

        public static void ToSchemaModel(this DataElement de, SchemaModel model)
        {
            model.SchemaID = de.DataElement_ID;
            model.Format = de.FileFormat;
            model.Header = "true";
            model.Delimiter = de.Delimiter;
            model.HiveDatabase = de.HiveDatabase;
            model.HiveTable = de.HiveTable;
            model.HiveLocation = de.HiveLocation;

            DataObject dObj = de.DataObjects.FirstOrDefault();

            List<ColumnModel> ColumnModelList = new List<ColumnModel>();

            if (dObj != null)
            {
                foreach (DataObjectField dof in dObj.DataObjectFields)
                {
                    ColumnModel cm = new ColumnModel();
                    cm.Name = dof.DataObjectField_NME;
                    cm.DataType = dof.DataType;
                    cm.Nullable = dof.Nullable.ToString();
                    cm.Length = dof.Length;
                    cm.Precision = dof.Precision;
                    cm.Scale = dof.Scale;

                    ColumnModelList.Add(cm);
                }
            }

            model.Columns = ColumnModelList;
        }

        public static void ToHiveCreateModel(this DataElement de, HiveTableCreateModel model)
        {
            SchemaModel schemaModel = new SchemaModel();
            de.ToSchemaModel(schemaModel);
            model.Schema = schemaModel;
        }
    }
}
