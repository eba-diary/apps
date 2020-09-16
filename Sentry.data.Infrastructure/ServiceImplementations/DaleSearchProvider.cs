using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Sentry.data.Core;
using Sentry.Common.Logging;
using Sentry.data.Core.GlobalEnums;
using System.Diagnostics;

namespace Sentry.data.Infrastructure
{
    public class DaleSearchProvider : IDaleSearchProvider
    {
        public DaleResultDto GetSearchResults(DaleSearchDto dto)
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

            //make sure incoming criteria is valid, or return empty results
            if (!IsCriteriaValid(dto))
            {
                daleResult.DaleEvent.QueryErrorMessage = "Invalid Criteria.  No Query executed.";
                daleResult.DaleEvent.QuerySuccess = false;
                return daleResult;
            }

            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
               
                string q = BuildAQuery(dto);
                SqlCommand command = new SqlCommand(q, connection);
                command.CommandTimeout = 0;

                command.Parameters.AddWithValue("@Criteria", System.Data.SqlDbType.VarChar);
                command.Parameters["@Criteria"].Value = "%" + dto.Criteria + "%";
                
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

            return daleResult;
        }

        public bool SaveSensitive(string sensitiveBlob)
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
                command.Parameters["@sensitiveBlob"].Value = sensitiveBlob;

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

        private string BuildAQuery(DaleSearchDto dto)
        {
            string q = String.Empty;
            string qSelect = "SELECT Asset_CDE, Server_NME,Database_NME,Base_NME,Type_DSC,Column_NME,Column_TYP,MaxLength_LEN,Precision_LEN,Scale_LEN,IsNullable_FLG,Effective_DTM,Alias_NME,Prod_Typ,BaseColumn_ID,IsSensitive_FLG,IsOwnerVerified_FLG ";
            string qFrom = (dto.Sensitive == DaleSensitive.SensitiveOnly)? "FROM ColumnSensitivityCurrent_v " : "FROM Column_v ";
            string qWhereStatement = BuildAWhere(dto);

            q = qSelect + qFrom + qWhereStatement;

            return q;
        }

        private string BuildAWhere(DaleSearchDto dto)
        {
            string qWhereColumn = String.Empty;
            string qWhereStatement = String.Empty;

            //set variable portion of WHERE STATEMENT
            if(dto.Sensitive == DaleSensitive.SensitiveOnly)
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

            return qWhereStatement;
        }

        private bool IsCriteriaValid(DaleSearchDto dto)
        {
            if (dto.Sensitive == DaleSensitive.SensitiveOnly)
            {
                return true;
            }

            //validate for white space only, null, empty string in criteria
            if (String.IsNullOrWhiteSpace(dto.Criteria))
            {
                return false;
            }

            //validate to ensure valid destination
            if 
            (    (dto.Destiny != DaleDestiny.Object) 
                    && (dto.Destiny != DaleDestiny.Column) 
                    && (dto.Destiny != DaleDestiny.SAID) 
                    && (dto.Destiny != DaleDestiny.Database)
                    && (dto.Destiny != DaleDestiny.Server)  
            )
            {
                return false;
            }
            return true;
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

            result.Alias = (!reader.IsDBNull(12)) ? reader.GetString(12) : String.Empty;
            result.ProdType = (!reader.IsDBNull(13)) ? reader.GetString(13) : String.Empty;
            
            if (!reader.IsDBNull(14))
            {
                result.BaseColumnId = reader.GetInt32(14);
            }

            if (!reader.IsDBNull(15))
            {
                result.IsSensitive = reader.GetBoolean(15);
            }

            if (!reader.IsDBNull(16))
            {
                result.IsOwnerVerified = reader.GetBoolean(16);
            }

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

            //make sure incoming criteria is valid, or return empty results
            if (!IsCriteriaValid(dto))
            {
                daleResult.DaleEvent.QueryErrorMessage = "Invalid Criteria.  No Query executed.";
                daleResult.DaleEvent.QuerySuccess = false;
      
            }

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
                qColumnToFilter = " Asset_CDE ";
            else if (dto.Destiny == DaleDestiny.Server)
                qColumnToFilter = " Server_NME ";
            else qColumnToFilter = " Database_NME ";

            q = qSelect + qFrom + qWhereStatement + " AND " + qColumnToFilter + "= @Criteria";
            q = "IF EXISTS ( " + q + " ) SELECT 1 AS TESTME";

            return q;
        }

    }
}
