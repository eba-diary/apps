using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sentry.data.Infrastructure
{
    public class DataInventorySqlExecuter : IDbExecuter
    {
        public void ExecuteCommand(object parameter)
        {
            using (SqlConnection connection = new SqlConnection(Configuration.Config.GetHostSetting("DataInventoryConnectionString")))
            {
                SqlCommand command = new SqlCommand("exec usp_DALE_BaseScanAction_UPDATE @sensitiveBlob", connection);

                command.Parameters.AddWithValue("@sensitiveBlob", System.Data.SqlDbType.NVarChar);
                command.Parameters["@sensitiveBlob"].Value = parameter;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public DataTable ExecuteQuery(DateTime startDateTime, DateTime endDateTime)
        {
            throw new System.NotSupportedException();
        }
    }
}
