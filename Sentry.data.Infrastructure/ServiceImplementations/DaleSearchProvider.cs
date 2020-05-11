using System;
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
            string qSelect = "SELECT Server_NME,Database_NME,Object_NME AS Table_NME,Column_NME,Column_TYP,Precision_LEN,Scale_LEN,Effective_DTM,Expiration_DTM,LastScan_DTM ";
            string qFrom = "FROM Column_v ";
            string qWhereColumn = String.Empty;
            string qWhereObjectType = String.Empty;
            string qWhereStatement = String.Empty;
            string q = String.Empty;

            if (dto.Destiny == DaleDestiny.Table)
            {
                qWhereColumn = "Object_NME";
                qWhereObjectType = " AND Object_TYP = 'ST' ";
            }
            else if(dto.Destiny == DaleDestiny.Column)
            {
                qWhereColumn = "Column_NME";
            }
            else if(dto.Destiny == DaleDestiny.View)
            {
                qWhereColumn = "Object_NME";
                qWhereObjectType = " AND Object_TYP = 'SV' ";
            }

            qWhereStatement += "WHERE " + qWhereColumn + " LIKE '%" + dto.Criteria + "%'";
            qWhereStatement += qWhereObjectType;
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
            if ( (dto.Destiny != DaleDestiny.Table) && (dto.Destiny != DaleDestiny.Column) && (dto.Destiny != DaleDestiny.View))
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
            result.Table = (!reader.IsDBNull(2)) ? reader.GetString(2) : String.Empty;
            result.Column = (!reader.IsDBNull(3)) ? reader.GetString(3) : String.Empty;
            result.ColumnType = (!reader.IsDBNull(4)) ? reader.GetString(4) : String.Empty;

            if (!reader.IsDBNull(5))
            {
                result.PrecisionLength = reader.GetInt32(5);
            }

            if (!reader.IsDBNull(6))
            {
                result.ScaleLength = reader.GetInt32(6);
            }

            if (!reader.IsDBNull(7))
            {
                result.EffectiveDate = reader.GetDateTime(7);
            }

            if (!reader.IsDBNull(8))
            {
                result.ExpirationDate = reader.GetDateTime(8);
            }

            if (!reader.IsDBNull(9))
            {
                result.LastScanDate = reader.GetDateTime(9);
            }

            return result;

        }
    }
}
