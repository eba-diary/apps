﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Sentry.data.Core;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class DaleSearchProvider : IDaleSearchProvider
    {


        public List<DaleResultDto> GetSearchResults(DaleSearchDto dto)
        {

            List<DaleResultDto> daleResults = new List<DaleResultDto>();

            dto.Criteria = "spam";
            
            string connectionString = Configuration.Config.GetHostSetting("DaleConnectionString");

            string qSelect  = "SELECT  Server_NME,Database_NME,Table_NME,Column_NME,Column_TYP,Precision_LEN,Scale_LEN,Effective_DTM,Expiration_DTM,LastScan_DTM ";
            string qFrom    = "FROM Column_v ";
            string qWhereColumn = (dto.Column) ? "Column_NME " : "Table_NME";
            string qWhereStatement = "WHERE " + qWhereColumn + " LIKE '%" + dto.Criteria + "%'";

            string q = qSelect + qFrom + qWhereStatement;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(q, connection);
                //command.Parameters.AddWithValue("@pricePoint", paramValue);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DaleResultDto result = new DaleResultDto();

                        result.Server           = (!reader.IsDBNull(0)) ? reader.GetString(0) : String.Empty;
                        result.Database         = (!reader.IsDBNull(1)) ? reader.GetString(1) : String.Empty;
                        result.Table            = (!reader.IsDBNull(2)) ? reader.GetString(2) : String.Empty;
                        result.Column           = (!reader.IsDBNull(3)) ? reader.GetString(3) : String.Empty;
                        result.ColumnType       = (!reader.IsDBNull(4)) ? reader.GetString(4) : String.Empty;

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

                        daleResults.Add(result);
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





    }
}
