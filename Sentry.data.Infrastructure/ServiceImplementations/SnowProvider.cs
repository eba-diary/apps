using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core;
using Snowflake.Data.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Sentry.data.Infrastructure
{
    public class SnowProvider : ISnowProvider
    {


        public System.Data.DataTable GetTopNRows(string db, string schema, string table, int rows)
        {
            string q = BuildSelectQuery(db, schema, table, rows);
            return ExecuteQuery(q);
        }


        public bool CheckIfExists(string db, string schema, string table)
        {
            //Logger.Debug($"start method <{System.Reflection.MethodBase.GetCurrentMethod().Name}>");
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();

            string q = BuildIfExistsQuery(db,schema,table);
            System.Data.DataTable dt = ExecuteQuery(q);
            if(dt.Rows.Count >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }


            //Logger.Debug($"end method <{System.Reflection.MethodBase.GetCurrentMethod().Name}>");
        }

        private string BuildSelectQuery(string db, string schema, string table, int rows)
        {
            return "SELECT * FROM " + db + "." + schema + "." + table + " LIMIT " + rows.ToString();
        }

        private string BuildIfExistsQuery(string db, string schema, string table)
        {
            return "SHOW OBJECTS LIKE '" + table + "' IN " + db + "." + schema ;
        }

        private System.Data.DataTable ExecuteQuery(string query)
        {
            DataTable dt;
            try
            {
                using (var connection = new SnowflakeDbConnection())
                {
                    connection.ConnectionString = Config.GetHostSetting("SnowConnectionString");
                    connection.Password = GetSecureString(Config.GetHostSetting("SnowPassword"));

                    System.Data.Common.DbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    connection.Open();
                    System.Data.Common.DbDataReader reader = command.ExecuteReader();
                    dt = FillDataTable(reader);

                    if (reader != null)
                        reader.Close();
                }
            }
            catch (AggregateException aggEx)
            {
                //aggEx.Handle(inner =>
                //{
                //    Logger.Error("Inner aggregate execution", inner);
                //    //return true;
                //});
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to execute snowflake reader", ex);
                throw;
            }

            return dt;
        }

        private DataTable FillDataTable(System.Data.Common.DbDataReader dr)
        {
            DataTable dtSchema = dr.GetSchemaTable();
            DataTable dt = new DataTable();
            List<DataColumn> listCols = new List<DataColumn>();
            if (dtSchema != null)
            {
                foreach (DataRow drow in dtSchema.Rows)
                {
                    string columnName = System.Convert.ToString(drow["ColumnName"]);
                    DataColumn column = new DataColumn(columnName, (Type)(drow["DataType"]));
                    column.AllowDBNull = (bool)drow["AllowDBNull"];
                    listCols.Add(column);
                    dt.Columns.Add(column);
                }

            }

            // Read rows from DataReader and populate the DataTable 
            while (dr.Read())
            {
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < listCols.Count; i++)
                {
                    dataRow[((DataColumn)listCols[i])] = dr[i];
                }

                dt.Rows.Add(dataRow);
            }

            return dt;


        }

        private System.Security.SecureString GetSecureString(string str)
        {
            var secureStr = new System.Security.SecureString();
            if (str.Length > 0)
                str.ToCharArray().ToList().ForEach(f => secureStr.AppendChar(f));
            return secureStr;
        }






    }
}
