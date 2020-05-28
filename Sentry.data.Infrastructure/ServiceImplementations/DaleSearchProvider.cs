﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Sentry.data.Core;
using Sentry.Common.Logging;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Infrastructure
{
    public class DaleSearchProvider : IDaleSearchProvider
    {
        public List<DaleResultDto> GetSearchResults(DaleSearchDto dto)
        {
            List<DaleResultDto> daleResults = new List<DaleResultDto>();

            //make sure incoming criteria is valid, or return empty results
            if (!IsCriteriaValid(dto))
            {
                return daleResults;
            }

            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(BuildAQuery(dto), connection);
                command.CommandTimeout = 0;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        daleResults.Add(CreateDaleResultDto(reader));
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Logger.Fatal("Failed to Ask Dale a question", ex);
                }
            }

            return daleResults;
        }

        private string BuildAQuery(DaleSearchDto dto)
        {
            string qSelect = "SELECT Server_NME,Database_NME,Base_NME,Type_DSC,Column_NME,Column_TYP,MaxLength_LEN,Precision_LEN,Scale_LEN,IsNullable_FLG,Effective_DTM ";
            string qFrom = "FROM Column_v ";
            string qWhereColumn = String.Empty;
            string qWhereStatement = String.Empty;
            string q = String.Empty;

            if (dto.Destiny == DaleDestiny.Object)
            {
                qWhereColumn = "Base_NME";
            }
            else if(dto.Destiny == DaleDestiny.Column)
            {
                qWhereColumn = "Column_NME";
            }
            qWhereStatement += "WHERE " + qWhereColumn + " LIKE '%" + dto.Criteria + "%'";
            qWhereStatement += " AND Expiration_DTM IS NULL";
            q = qSelect + qFrom + qWhereStatement;

            return q;
        }

        private bool IsCriteriaValid(DaleSearchDto dto)
        {
            //validate for white space only, null, empty string in criteria
            if (String.IsNullOrWhiteSpace(dto.Criteria))
            {
                return false;
            }

            //validate to ensure valid destination
            if ( (dto.Destiny != DaleDestiny.Object) && (dto.Destiny != DaleDestiny.Column) )
            {
                return false;
            }
            return true;
        }

        private DaleResultDto CreateDaleResultDto(SqlDataReader reader)
        {
            DaleResultDto result = new DaleResultDto();
            result.Server = (!reader.IsDBNull(0)) ? reader.GetString(0) : String.Empty;
            result.Database = (!reader.IsDBNull(1)) ? reader.GetString(1) : String.Empty;
            result.Object = (!reader.IsDBNull(2)) ? reader.GetString(2) : String.Empty;
            result.ObjectType = (!reader.IsDBNull(3)) ? reader.GetString(3) : String.Empty;
            result.Column = (!reader.IsDBNull(4)) ? reader.GetString(4) : String.Empty;
            result.ColumnType = (!reader.IsDBNull(5)) ? reader.GetString(5) : String.Empty;

            if (!reader.IsDBNull(6))
            {
                result.MaxLength = reader.GetInt32(6);
            }

            if (!reader.IsDBNull(7))
            {
                result.Precision = reader.GetInt32(7);
            }

            if (!reader.IsDBNull(8))
            {
                result.Scale = reader.GetInt32(8);
            }

            if (!reader.IsDBNull(9))
            {
                result.IsNullable = reader.GetBoolean(9);
            }

            if (!reader.IsDBNull(10))
            {
                result.EffectiveDate = reader.GetDateTime(10);
            }

            return result;
        }
    }
}
