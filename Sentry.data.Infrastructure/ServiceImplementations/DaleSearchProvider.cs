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
            string q = String.Empty;
            string qSelect = "SELECT Asset_CDE, Server_NME,Database_NME,Base_NME,Type_DSC,Column_NME,Column_TYP,MaxLength_LEN,Precision_LEN,Scale_LEN,IsNullable_FLG,Effective_DTM,Alias_NME,Prod_Typ,BaseColumn_ID ";
            string qFrom = "FROM Column_v ";
            string qWhereStatement = BuildAWhere(dto);

            q = qSelect + qFrom + qWhereStatement;

            return q;
        }

        private string BuildAWhere(DaleSearchDto dto)
        {
            string qWhereColumn = String.Empty;
            string qWhereStatement = String.Empty;

            if(dto.Sensitive)
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
                qWhereStatement += "WHERE " + qWhereColumn + " LIKE '%" + dto.Criteria + "%'";
            }

            qWhereStatement += " AND Expiration_DTM IS NULL";

            return qWhereStatement;
        }

        private bool IsCriteriaValid(DaleSearchDto dto)
        {
            if (dto.Sensitive)
                return true;

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
                result.Scale = reader.GetInt32(14);
            }

            return result;
        }
    }
}
