using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Sentry.data.Core;
using Sentry.Common.Logging;
using Sentry.data.Core.GlobalEnums;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class DaleSearchProvider : IDaleSearchProvider
    {
        public Task<DaleResultDto> GetSearchResults(DaleSearchDto dto)
        {
            DaleResultDto daleResult = new DaleResultDto();
            daleResult.DaleResults = new List<DaleResultRowDto>();


            daleResult.DaleEvent = new DaleEventDto()
            {
                Criteria = dto.Criteria,
                Destiny = dto.Destiny.GetDescription(),
                QuerySuccess = true,
                Sensitive = dto.Sensitive.GetDescription()
            };

            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandTimeout = 0;
                string q = BuildAQuery(dto, ref command);
                command.CommandText = q;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        daleResult.DaleResults.Add(CreateDaleResultRow(reader));
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    string daleMessage = "Dale Failed!!  Query: " + q;
                    daleResult.DaleEvent.QuerySuccess = false;
                    daleResult.DaleEvent.QueryErrorMessage = daleMessage + " " + ex.Message;
                    Logger.Fatal(daleMessage, ex);
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                daleResult.DaleEvent.QueryRows = daleResult.DaleResults.Count;
                daleResult.DaleEvent.QuerySeconds = ts.Seconds;

                Logger.Info("DaleSearchProvider.GetSearchResults()  Row Count:" + daleResult.DaleResults.Count + " Elapsed Seconds:" + ts.Seconds + " Query:" + q);
            }

            return Task.FromResult(daleResult);
        }

        public FilterSearchDto GetSearchFilters(DaleSearchDto dto)
        {
            throw new NotImplementedException();
        }

        private string BuildAQuery(DaleSearchDto dto, ref SqlCommand command)
        {
            string q = String.Empty;
            string qSelect = "SELECT Asset_CDE, Server_NME,Database_NME,Base_NME,Type_DSC,Column_NME,Column_TYP,MaxLength_LEN,Precision_LEN,Scale_LEN,IsNullable_FLG,Effective_DTM,Prod_Typ,BaseColumn_ID,IsSensitive_FLG,IsOwnerVerified_FLG,Source_NME,ScanList_NME,SAIDList_NME ";
            string qFrom = (dto.Sensitive == DaleSensitive.SensitiveOnly)? "FROM ColumnSensitivityCurrent_v " : "FROM Column_v ";
            string qWhereStatement = BuildAWhere(dto, ref command);

            q = qSelect + qFrom + qWhereStatement;

            return q;
        }

        private string BuildAWhere(DaleSearchDto dto, ref SqlCommand command)
        {
            string qWhereColumn = String.Empty;
            string qWhereStatement = String.Empty;

            //ADVANCED QUERY
            if (dto.Destiny == DaleDestiny.Advanced)
            {
                qWhereStatement = AddAllAdvancedParams(dto.AdvancedCriteria, ref command);
            }
            else //EVERY OTHER KIND OF QUERY
            {
                if (dto.Sensitive == DaleSensitive.SensitiveOnly)
                {
                    qWhereStatement = "WHERE IsSensitive_FLG = 1 ";
                }
                else
                {
                    if (dto.Destiny == DaleDestiny.Object)
                    {
                        qWhereColumn = "Base_NME";
                    }
                    else if (dto.Destiny == DaleDestiny.Column)
                    {
                        qWhereColumn = "Column_NME";
                    }
                    else if (dto.Destiny == DaleDestiny.SAID)
                    {
                        qWhereColumn = "Asset_CDE";
                    }

                    qWhereStatement += "WHERE " + qWhereColumn + " LIKE @Criteria ";

                    //ONLY apply logic here if they dont want to see any sensitive information
                    if (dto.Sensitive == DaleSensitive.SensitiveNone)
                    {
                        qWhereStatement += " AND IsSensitive_FLG = 0 ";
                    }

                    qWhereStatement += " AND Expiration_DTM IS NULL";
                }

                command.Parameters.AddWithValue("@Criteria", System.Data.SqlDbType.VarChar);
                command.Parameters["@Criteria"].Value = "%" + dto.Criteria + "%";

            }

            return qWhereStatement;
        }

        //CREATE dynamic SQL WHERE STATEMENT AND ADD PARMS to SqlCommand
        //NOTE!! only add params if they are used
        private string AddAllAdvancedParams(DaleAdvancedCriteriaDto advanced, ref SqlCommand command)
        {
            string qWhereStatement = string.Empty;

            if (!string.IsNullOrWhiteSpace(advanced.Asset))
            {
                qWhereStatement = AddSingleAdvancedParam(qWhereStatement, "Asset_CDE", advanced.Asset, ref command);
            }

            if (!string.IsNullOrWhiteSpace(advanced.Server))
            {
                qWhereStatement += AddSingleAdvancedParam(qWhereStatement, "Server_NME", advanced.Server, ref command);
            }

            if (!string.IsNullOrWhiteSpace(advanced.Database))
            {
                qWhereStatement += AddSingleAdvancedParam(qWhereStatement, "Database_NME", advanced.Database, ref command);
            }

            if (!string.IsNullOrWhiteSpace(advanced.Object))
            {
                qWhereStatement += AddSingleAdvancedParam(qWhereStatement, "Base_NME", advanced.Object, ref command);
            }

            if (!string.IsNullOrWhiteSpace(advanced.ObjectType))
            {
                qWhereStatement += AddSingleAdvancedParam(qWhereStatement, "Type_DSC", advanced.ObjectType, ref command);
            }

            if (!string.IsNullOrWhiteSpace(advanced.Column))
            {
                qWhereStatement += AddSingleAdvancedParam(qWhereStatement, "Column_NME", advanced.Column, ref command);
            }

            if (!string.IsNullOrWhiteSpace(advanced.SourceType))
            {
                qWhereStatement += AddSingleAdvancedParam(qWhereStatement, "Source_NME", advanced.SourceType, ref command);
            }

            if (!string.IsNullOrWhiteSpace(qWhereStatement))
            {
                qWhereStatement = " WHERE " + qWhereStatement + " AND Expiration_DTM IS NULL";
            }

            return qWhereStatement;

        }

        //Add a single param to the dynamic sql statement and add the PARAM to SqlCommand
        //the calling function here ONLY CALLS this if its valid
        private string AddSingleAdvancedParam(string currentWhereStatement, string name, string value, ref SqlCommand command)
        {
            string fullParamName = "@AdvancedCriteria" + name;
            string singleWhere = " " + name + " LIKE " + fullParamName + " ";
            if (!String.IsNullOrWhiteSpace(currentWhereStatement))
            {
                singleWhere = " AND " + singleWhere;
            }

            command.Parameters.AddWithValue(fullParamName, System.Data.SqlDbType.VarChar);
            command.Parameters[fullParamName].Value = "%" + value + "%";

            return singleWhere;
        }


        public bool SaveSensitive(List<DaleSensitiveDto> dtos)
        {
            bool success = true;

            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                string q = "exec usp_DALE_BaseScanAction_UPDATE @sensitiveBlob";
                SqlCommand command = new SqlCommand(q, connection);
                command.CommandTimeout = 0;

                command.Parameters.AddWithValue("@sensitiveBlob", System.Data.SqlDbType.NVarChar);
                command.Parameters["@sensitiveBlob"].Value = Newtonsoft.Json.JsonConvert.SerializeObject(dtos);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Fatal("DaleSearchProvider.SaveSensitive() Failed!!  Query: " + q, ex);

                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Logger.Info("DaleSearchProvider.SaveSensitive()  Elapsed Seconds:" + ts.Seconds + " Query:" + q);
            }

            return success;
        }

        private DaleResultRowDto CreateDaleResultRow(SqlDataReader reader)
        {
            DaleResultRowDto result = new DaleResultRowDto();
            result.Asset = (!reader.IsDBNull(0)) ? reader.GetString(0) : String.Empty;
            result.Server = (!reader.IsDBNull(1)) ? reader.GetString(1) : String.Empty;
            result.Database = (!reader.IsDBNull(2)) ? reader.GetString(2) : String.Empty;
            result.Object = (!reader.IsDBNull(3)) ? reader.GetString(3) : String.Empty;
            result.ObjectType = (!reader.IsDBNull(4)) ? reader.GetString(4) : String.Empty;
            result.Column = (!reader.IsDBNull(5)) ? reader.GetString(5) : String.Empty;
            result.ColumnType = (!reader.IsDBNull(6)) ? reader.GetString(6) : String.Empty;

            if (!reader.IsDBNull(7))
            {
                result.MaxLength = reader.GetInt32(7);
            }

            if (!reader.IsDBNull(8))
            {
                result.Precision = reader.GetInt32(8);
            }

            if (!reader.IsDBNull(9))
            {
                result.Scale = reader.GetInt32(9);
            }

            if (!reader.IsDBNull(10))
            {
                result.IsNullable = reader.GetBoolean(10);
            }

            if (!reader.IsDBNull(11))
            {
                result.EffectiveDate = reader.GetDateTime(11);
            }

            result.ProdType = (!reader.IsDBNull(12)) ? reader.GetString(12) : String.Empty;
            
            if (!reader.IsDBNull(13))
            {
                result.BaseColumnId = reader.GetInt32(13);
            }

            if (!reader.IsDBNull(14))
            {
                result.IsSensitive = reader.GetBoolean(14);
            }

            if (!reader.IsDBNull(15))
            {
                result.IsOwnerVerified = reader.GetBoolean(15);
            }

            result.SourceType = (!reader.IsDBNull(16)) ? reader.GetString(16) : String.Empty;

            result.ScanCategory = (!reader.IsDBNull(17)) ? reader.GetString(17) : String.Empty;
            result.ScanType = (!reader.IsDBNull(18)) ? reader.GetString(18) : String.Empty;

            return result;
        }

        public DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto)
        {
            DaleContainSensitiveResultDto daleResult = new DaleContainSensitiveResultDto();
            daleResult.DoesContainSensitiveResults = false;

            daleResult.DaleEvent = new DaleEventDto()
            {
                Criteria = dto.Criteria,
                Destiny = dto.Destiny.GetDescription(),
                QuerySuccess = true,
                Sensitive = dto.Sensitive.GetDescription()
            };

            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                string q = BuildDoesContainSensitiveQuery(dto);
                SqlCommand command = new SqlCommand(q, connection);
                command.CommandTimeout = 0;

                command.Parameters.AddWithValue("@Criteria", System.Data.SqlDbType.VarChar);
                command.Parameters["@Criteria"].Value =  dto.Criteria;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        daleResult.DoesContainSensitiveResults = true;
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    string daleMessage = "Dale Failed!!  Query: " + q;
                    daleResult.DaleEvent.QuerySuccess = false;
                    daleResult.DaleEvent.QueryErrorMessage = daleMessage + " " + ex.Message;
                    Logger.Fatal(daleMessage, ex);
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                daleResult.DaleEvent.QuerySeconds = ts.Seconds;
                Logger.Info("DaleSearchProvider.DoesItemContainSensitive()  Elapsed Seconds:" + ts.Seconds + " Query:" + q);
            }

            return daleResult;
        }

        private string BuildDoesContainSensitiveQuery(DaleSearchDto dto)
        {
            string q = String.Empty;
            string qSelect = "SELECT TOP 1 * ";
            string qFrom = " FROM Column_v ";
            string qWhereStatement = " WHERE Expiration_DTM IS NULL AND IsSensitive_FLG = 1 ";
            string qColumnToFilter = String.Empty;

            if (dto.Destiny == DaleDestiny.SAID)
            {
                qColumnToFilter = " Asset_CDE ";
            }
            else if (dto.Destiny == DaleDestiny.Server)
            {
                qColumnToFilter = " Server_NME ";
            }
            else
            {
                qColumnToFilter = " Database_NME ";
            }

            q = qSelect + qFrom + qWhereStatement + " AND " + qColumnToFilter + "= @Criteria";
            q = "IF EXISTS ( " + q + " ) SELECT 1 AS TESTME";

            return q;
        }

        public DaleCategoryResultDto GetCategoriesByAsset(string search)
        {
            DaleCategoryResultDto daleResult = new DaleCategoryResultDto();
            daleResult.DaleCategories = new List<DaleCategoryDto>();

            daleResult.DaleEvent = new DaleEventDto()
            {
                Criteria = search,
                QuerySuccess = true
            };

            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                string q = BuildGetCategoriesByAssetQuery();
                SqlCommand command = new SqlCommand(q, connection);
                command.CommandTimeout = 0;

                command.Parameters.AddWithValue("@Criteria", System.Data.SqlDbType.VarChar);
                command.Parameters["@Criteria"].Value = search;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        daleResult.DaleCategories.Add(CreateCategoryResult(reader));
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    string daleMessage = "Dale Failed!!  Query: " + q;
                    daleResult.DaleEvent.QuerySuccess = false;
                    daleResult.DaleEvent.QueryErrorMessage = daleMessage + " " + ex.Message;
                    Logger.Fatal(daleMessage, ex);
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                daleResult.DaleEvent.QuerySeconds = ts.Seconds;
                Logger.Info("DaleSearchProvider.GetCategoriesByAsset()  Elapsed Seconds:" + ts.Seconds + " Query:" + q);
            }

            return daleResult;
        }

        private string BuildGetCategoriesByAssetQuery()
        {
            //NOTE: Multiple SAIDExposure_NME's can exist in the list coming back so the GROUP BYs on this essentially bring back a distinct list
            //the use of MAX here is a trick to bring back anything that is IsSensitive_FLG = 1
            string q = @"SELECT DALE.SAIDExposure_NME,CAST(MAX(DALE.IsSensitive_FLG) AS BIT) AS IsSensitive_FLG
                        FROM
                        (
	                        SELECT ScanAction.SAIDExposure_NME, CASE WHEN BaseScanAction_SENSITIVE.ScanAction_ID IS NOT NULL THEN CAST(1 AS INT) ELSE CAST(0 AS INT) END AS IsSensitive_FLG 
	                        FROM ScanAction ScanAction
	                        LEFT JOIN
	                        (
		                        SELECT BaseScanAction.ScanAction_ID
		                        FROM BaseScanAction BaseScanAction
		                        JOIN Column_v Column_v
			                        ON BaseScanAction.Base_ID = Column_v.BaseColumn_ID
		                        WHERE Sensitive_FLG = 1
				                        AND Column_v.Expiration_DTM IS NULL
				                        AND Column_v.Asset_CDE = @Criteria
		                        GROUP BY ScanAction_ID
	                        ) AS BaseScanAction_SENSITIVE
		                        ON ScanAction.ScanAction_ID = BaseScanAction_SENSITIVE.ScanAction_ID
	                        WHERE ScanAction.Active_FLG = 1 AND ScanAction.WebUse_FLG = 1
	                        GROUP BY  ScanAction.SAIDExposure_NME, CASE WHEN BaseScanAction_SENSITIVE.ScanAction_ID IS NOT NULL THEN CAST(1 AS INT) ELSE CAST(0 AS INT) END
                        ) AS DALE
                        GROUP BY DALE.SAIDExposure_NME";

            return q;
        }

        private DaleCategoryDto CreateCategoryResult(SqlDataReader reader)
        {
            DaleCategoryDto result = new DaleCategoryDto();
            result.Category = (!reader.IsDBNull(0)) ? reader.GetString(0) : String.Empty;

            if (!reader.IsDBNull(1))
            {
                result.IsSensitive = reader.GetBoolean(1);
            }

            return result;
        }

    }
}
