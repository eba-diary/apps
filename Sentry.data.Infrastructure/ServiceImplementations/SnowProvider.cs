using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core;
using Snowflake.Data.Client;
using System;
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
            DataTable dt;
            string connectionString = Config.GetHostSetting("SnowConnectionString");
            Logger.Info("START STEP 1:  SnowProvider.ExecuteQuery() ConnectionString:" + connectionString + " Query:" + query);

            try
            {
                using (var connection = new SnowflakeDbConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Password = GetSecureString(Config.GetHostSetting("SnowPassword"));

                    System.Data.Common.DbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    command.CommandTimeout = 1800;

                    connection.Open();
                    Logger.Info("COMPLETE STEP 2:  SnowProvider.ExecuteQuery.ConnectionOpen()" + " Query:" + query);

                    System.Data.Common.DbDataReader reader = command.ExecuteReader();
                    Logger.Info("COMPLETE STEP 3:  SnowProvider.ExecuteQuery.command.ExecuteReader()" + " Query:" + query);

                    dt = FillDataTable(reader);

                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SnowProvider.ExecuteQuery() failed.  ConnectionString:" + connectionString + " Query:" + query, ex);
                throw;
            }

            Logger.Info($"END  STEP 6: SnowProvider.ExecuteQuery()" + " Query:" + query);
            return dt;
        }

        private DataTable FillDataTable(System.Data.Common.DbDataReader dr)
        {
            Logger.Info("START  STEP 4:  SnowProvider.FillDataTable()");
            DataTable schema = dr.GetSchemaTable();
            DataTable dt = new DataTable();

            if (schema != null)
            {
                foreach (DataRow r in schema.Rows)
                {
                    string columnName = System.Convert.ToString(r["ColumnName"]);
                    DataColumn column = new DataColumn(columnName, (Type)(r["DataType"]));
                    column.AllowDBNull = (bool)r["AllowDBNull"];
                    dt.Columns.Add(column);
                }
            }

            // Read rows from DataReader and populate the DataTable  with rows
            while (dr.Read())
            {
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dataRow[(dt.Columns[i])] = dr[i];
                }

                dt.Rows.Add(dataRow);
            }

            Logger.Info("END  STEP 5:  SnowProvider.FillDataTable()");

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
