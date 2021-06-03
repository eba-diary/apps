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
            Logger.Info("SnowProvider.ExecuteQuery() QUERY TO BE EXECUTED:" + query); 
            DataTable dt;
            try
            {
                using (var connection = new SnowflakeDbConnection())
                {
                    connection.ConnectionString = Config.GetHostSetting("SnowConnectionString");
                    Logger.Info("SnowProvider.ExecuteQuery() Connection:" + connection.ConnectionString);
                    connection.Password = GetSecureString(Config.GetHostSetting("SnowPassword"));
                    Logger.Info("SnowProvider.ExecuteQuery() word:" + connection.Password);

                    System.Data.Common.DbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    Logger.Info("SnowProvider.ExecuteQuery() commandText set");
                    connection.Open();
                    Logger.Info("SnowProvider.ExecuteQuery() Connection Open Complete" + connection.ConnectionString);
                    System.Data.Common.DbDataReader reader = command.ExecuteReader();
                    Logger.Info("SnowProvider.ExecuteQuery() Reader initialized" + connection.ConnectionString);
                    dt = FillDataTable(reader);

                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SnowProvider.ExecuteQuery() failed:" + query, ex);
                throw;
            }

            Logger.Info($"End method SnowProvider.ExecuteQuery()");
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
            {
                str.ToCharArray().ToList().ForEach(f => secureStr.AppendChar(f));
            }
            return secureStr;
        }






    }
}
