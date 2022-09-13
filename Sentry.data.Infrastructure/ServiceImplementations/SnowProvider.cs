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

        //File name compare
        public System.Data.DataTable GetExceptRows(SnowCompareConfig snowCompareConfig)
        {  
            string queryClause = "";

            queryClause = CreateAuditWhereClause(snowCompareConfig.AuditSearchType, snowCompareConfig.QueryParameter);

            string exceptQuery = "";

            exceptQuery += $"drop table if exists {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.SrcCompare; ";
            exceptQuery += $"create temporary table {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.SrcCompare as SELECT ETL_FILE_NAME_ONLY FROM {snowCompareConfig.SourceDb}.{snowCompareConfig.Schema}.{snowCompareConfig.Table} data {queryClause} GROUP BY ETL_FILE_NAME_ONLY; ";
            exceptQuery += $"drop table if exists {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.TgtCompare; ";
            exceptQuery += $"create temporary table {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.TgtCompare as SELECT ETL_FILE_NAME_ONLY FROM {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.VW_{snowCompareConfig.Table} data {queryClause} GROUP BY ETL_FILE_NAME_ONLY; ";

            exceptQuery += $"SELECT";
            exceptQuery += $"COALESCE(tgt.ETL_FILE_NAME_ONLY,src.ETL_FILE_NAME_ONLY) AS ETL_FILE_NAME ";
            exceptQuery += $"FROM {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.TgtCompare AS tgt ";
            exceptQuery += $"FULL OUTER JOIN {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.SrcCompare AS src ON tgt.ETL_FILE_NAME_ONLY = src.ETL_FILE_NAME_ONLY;";

            return ExecuteQuery(exceptQuery);
        }

        //File row count compare
        public System.Data.DataTable GetCompareRows(SnowCompareConfig snowCompareConfig)
        {
            string queryClause = "";

            queryClause = CreateAuditWhereClause(snowCompareConfig.AuditSearchType, snowCompareConfig.QueryParameter);

            string compareQuery = "";

            compareQuery += $"drop table if exists {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.SrcCompare; ";
            compareQuery += $"create temporary table {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.SrcCompare as SELECT ETL_FILE_NAME_ONLY, COUNT(1) AS RAW_COUNT FROM {snowCompareConfig.SourceDb}.{snowCompareConfig.Schema}.{snowCompareConfig.Table} data {queryClause} GROUP BY ETL_FILE_NAME_ONLY; ";
            compareQuery += $"drop table if exists {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.TgtCompare; ";
            compareQuery += $"create temporary table {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.TgtCompare as SELECT ETL_FILE_NAME_ONLY, COUNT(1) AS PAR_COUNT FROM {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.VW_{snowCompareConfig.Table} data {queryClause} group by ETL_FILE_NAME_ONLY; ";

            compareQuery += $"SELECT ";
            compareQuery += $"COALESCE(tgt.ETL_FILE_NAME_ONLY,src.ETL_FILE_NAME_ONLY) AS ETL_FILE_NAME, ";
            compareQuery += $"ifnull(src.RAW_COUNT, 0) AS RAW_COUNT, ";
            compareQuery += $"ifnull(tgt.PAR_COUNT, 0) AS PAR_COUNT ";
            compareQuery += $"FROM {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.TgtCompare AS tgt ";
            compareQuery += $"FULL OUTER JOIN {snowCompareConfig.TargetDb}.{snowCompareConfig.Schema}.SrcCompare AS src ON tgt.ETL_FILE_NAME_ONLY = src.ETL_FILE_NAME_ONLY;";

            return ExecuteQuery(compareQuery);
        }

        private string CreateAuditWhereClause(AuditSearchType auditSearchType, string queryParameter)
        {
            string queryClause = "";

            switch (auditSearchType)
            {
                case AuditSearchType.fileName:
                    queryClause = $"WHERE \"ETL_FILE_NAME_ONLY\" = '{queryParameter}' AND DATE_PARTITION > '{DateTime.Now.AddDays(-30):yyyy-MM-dd}'";
                    break;
                case AuditSearchType.dateSelect:
                    queryClause = $"WHERE DATE_PARTITION > '{queryParameter}'";
                    break;
            }

            return queryClause;
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
            Logger.Info(query);

            DataTable dt = new DataTable();
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
                    command.CommandTimeout = 0;

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

        private DataTable FillDataTable(System.Data.Common.DbDataReader reader)
        {
            DataTable dt = new DataTable();
            Logger.Info("STEP 4 START:  SnowProvider.FillDataTable()");
            DataTable schema = reader.GetSchemaTable();

            Logger.Info("START  STEP 4.5:  SnowProvider.FillDataTable() BUILD COLUMNS");
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

            Logger.Info("STEP 4.6:  SnowProvider.ExecuteQuery() dt.Load START");
            dt.Load(reader);

            Logger.Info("STEP 4.7 END:  SnowProvider.FillDataTable()");
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
