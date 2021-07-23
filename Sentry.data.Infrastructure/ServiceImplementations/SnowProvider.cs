﻿using Sentry.Common.Logging;
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
            //TODO: GORDON DELETE COMMENTS once working
            //string selectMe = "SELECT DATE_PARTITION,ID,LEGACYID,EXTERNALCALLID,REPORTTYPE,CALLSOURCE,STARTED,CNVT_STARTED,COMPLETED,CNVT_COMPLETED,INITIALSOURCECODE,STARTEDBYUSERID,STARTEDBYUSERNAME,STARTEDBYUSERLOCATION,COMPLETEDBYUSERID,COMPLETEDBYUSERNAME,COMPLETEDBYUSERLOCATION,CREATED,CNVT_CREATED,LASTUPDATED,CNVT_LASTUPDATED,LASTUPDATEDBYUSERID,ISRECORDDELETED,CURRENTPAGEID,CLAIMNUMBER,ISESCALATED,DATESUBMITTED,CNVT_DATESUBMITTED,MESSAGEMETADATA,FNOLINFO,RAPIDFAXINFORMATIONCATEGORY,PERSONREPORTINGEXTERNAL,FACTSOFLOSS,WCFACTSOFLOSS,ACCTSEARCHMAIN,POLICYSEARCH,JOBCLASSCODECATEGORY,ACCTLOCATION,DUPLICATECLAIMSEARCHCATEGORY,PERSONREPORTING,RISKLOCATION,LOSSLOCATION,TRANSPORTATIONCATEGORY,INSUREDVEHICLECATEGORY,ESTIMATESUMMARY,INSUREDVEHICLEOWNERCATEGORY,INSUREDDRIVERCATEGORY,INSUREDVEHICLEPASSENGERSUMMARY,OTHERVEHICLESUMMARY,OTHERTHIRDPARTYSUMMARY,POLICEANDFIRE,CSCCLAIMCAT,HIDDENESCCATEGORY,COMMENTSREMARKS,SUBROGATIONLOOKUPRESULTS,ETL_FILE_NAME,ETL_FILE_NAME_ONLY,ETL_LOAD_DATE,CNVT_ETL_LOAD_DATE,ETL_LOAD_DTM,CNVT_ETL_LOAD_DTM,ETL_ORIG_LOAD_DATE,CNVT_ETL_ORIG_LOAD_DATE,ETL_ORIG_LOAD_DTM,CNVT_ETL_ORIG_LOAD_DTM,KAFKA_PARTITION,CNVT_KAFKA_PARTITION,KAFKA_OFFSET,CNVT_KAFKA_OFFSET,KAFKA_TIMESTAMP,CNVT_KAFKA_TIMESTAMP ";
            //rows = 10;
            //string fromMe = " FROM " + db + "." + schema + "." + table + " LIMIT " + rows.ToString();
            //return selectMe + fromMe;
            
            return "SELECT * FROM " + db + "." + schema + "." + table + " LIMIT " + rows.ToString();
        }

        private string BuildIfExistsQuery(string db, string schema, string table)
        {
            return "SHOW OBJECTS LIKE '" + table + "' IN " + db + "." + schema ;
        }

        private System.Data.DataTable ExecuteQuery(string query)
        {
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
