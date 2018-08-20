using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.Livy;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Sentry.data.Web.Helpers
{
    public class LivyHelper
    {
        public IDatasetContext _datasetContext;
        public int SessionID;
        public static string _livyUrl;

        public LivyHelper(IDatasetContext dsCtxt)
        {
            _datasetContext = dsCtxt;
            _livyUrl = Sentry.Configuration.Config.GetHostSetting("ApacheLivy");
        }

        public static string SQLServerDataTypeToHive(string sqlServerDataType)
        {
            switch (sqlServerDataType.ToUpper())
            {
                case "INTEGER":
                case "SMALLINT":
                    return "INTEGER";
                case "VARCHAR":
                case "CHAR":
                case "LONGVARCHAR":
                case "NVARCHAR":
                case "NCHAR":
                case "LONGNVARCHAR":
                case "CLOB":
                    return "STRING";
                case "DATE":
                case "DATETIME":
                    //return "DATE";
                    return "STRING";
                case "TIME":
                case "TIMESTAMP":
                    //return "TIMESTAMP";
                    return "STRING";
                case "NUMERIC":
                case "DOUBLE":
                    return "DOUBLE";
                case "REAL":
                case "FLOAT":
                    return "FLOAT";
                case "DECIMAL":
                    return "DECIMAL";
                case "BIT":
                case "BOOLEAN":
                    return "BOOLEAN";
                case "TINYINT":
                    return "TINYINT";
                case "BIGINT":
                    return "BIGINT";
                default:
                    // TODO(Andrew): Support BINARY, VARBINARY, LONGVARBINARY, DISTINCT,
                    // BLOB, ARRAY, STRUCT, REF, JAVA_OBJECT.
                    return sqlServerDataType.ToUpper();
            }
        }

        public string GetDataFrameFromS3Key(Guid guid, string s3Key, int configID)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);

            /* Get the Basic Information from the Metadata Repository */
            DataElement dataElement = _datasetContext.DataElements.Where(x => x.DataElement_ID == dfc.DataElement_ID).FirstOrDefault();
            var dataObjectID = _datasetContext.Schemas.Where(x => x.DatasetFileConfig.ConfigId == dfc.ConfigId).FirstOrDefault().DataObject_ID;
            DataObject dataObject = _datasetContext.GetById<DataObject>(dataObjectID);

            String delimiter = null;
            if (dataElement.DataElementDetails.Any(x => x.DataElementDetailType_CDE == "FileDelimiter_TYP"))
            {
                delimiter = dataElement.DataElementDetails.Where(x => x.DataElementDetailType_CDE == "FileDelimiter_TYP").FirstOrDefault().DataElementDetailType_VAL;
            }

            String fileFormat = dataElement.DataElementDetails.Where(x => x.DataElementDetailType_CDE == "FileFormat_TYP").FirstOrDefault().DataElementDetailType_VAL;

            String headerRow = "false";
            if (dataObject.DataObjectDetails.Any(x => x.DataObjectDetailType_CDE == "HeaderRow_IND"))
            {
                headerRow = dataObject.DataObjectDetails.Where(x => x.DataObjectDetailType_CDE == "HeaderRow_IND").FirstOrDefault().DataObjectDetailType_VAL == "Y" ? "true" : "false";
            }

            return CreateSparkDataFrame(guid, fileFormat, headerRow, delimiter, s3Key);

        }

        private string CreateSparkDataFrame(Guid guid, String fileFormat, String headerRow, String delimiter, String s3Key)
        {
            String python = "tmp_" + guid.ToString("N") + $" = spark.read.format('{ fileFormat.ToLower() }').option('header', '{headerRow}')";

            if (fileFormat.ToLower() == "json")
            {
                python += ".option('multiline', 'true')";
            }

            if (delimiter != null)
            {
                python += $".option('delimiter', '{ delimiter }')";
            }

            python += ".option('inferSchema', 'true').load('" + s3Key + "');";
            python += "tmp_" + guid.ToString("N") + ".printSchema();";


            String quoted = System.Web.Helpers.Json.Encode(python);
            quoted = quoted.Substring(1, quoted.Length - 2);

            return quoted;
        }

        public List<HiveColumn> GetHiveColumns(LivyReply lr)
        {
            /* EXAMPLE:
                root
                |-- CRASH_ID: integer (nullable = true)
             */

            List<HiveColumn> hiveColumns = new List<HiveColumn>();
            string[] columns = lr.output.data.text.Split(new string[] { "|-- " }, StringSplitOptions.None);

            for (int i = 1; i < columns.Length; i++)
            {
                HiveColumn hc = new HiveColumn()
                {
                    name = columns[i].Substring(0, columns[i].IndexOf(":")).Trim(),
                    datatype = columns[i].Substring(columns[i].IndexOf(":") + 2, (columns[i].IndexOf("(") - columns[i].IndexOf(":") - 2)).Trim(),
                    nullable = Convert.ToBoolean(columns[i].Substring(columns[i].IndexOf("=") + 2).Replace("\n", "").Trim(')', ' '))
                };
                hiveColumns.Add(hc);
            }

            return hiveColumns;
        }

        public String CompareSchemas(List<HiveColumn> hiveColumns, IList<DataObjectField> dataObjectFields)
        {
            String output = "";
            foreach (DataObjectField b in dataObjectFields)
            {
                String hiveDataType = SQLServerDataTypeToHive(b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Datatype_TYP").DataObjectFieldDetailType_VAL);

                HiveColumn found = hiveColumns.FirstOrDefault(x => x.name.ToLower() == b.DataObjectField_NME.ToLower());
                found.found = true;

                //Check for the Name of the field in the file.
                if (found != null)
                {
                    if (found.datatype.ToUpper() == hiveDataType.ToUpper())
                    {

                    }
                    else
                    {
                        //Columns Data Type is Wrong
                        output += "(Wrong Data Type)(Column = " + found.name + ") : Found - " + found.datatype.ToUpper() + " ~ Expected - " + hiveDataType.ToUpper() + " \n ";
                    }
                }
                else
                {
                    output += "(Missing Column): " + b.DataObjectField_NME + " \n ";
                }
            }

            foreach (HiveColumn hc in hiveColumns.Where(x => x.found == false))
            {
                //Column does not exist.
                output += "(New Column) : " + hc.name + " \n ";
            }
            return output;
        }
    }
}