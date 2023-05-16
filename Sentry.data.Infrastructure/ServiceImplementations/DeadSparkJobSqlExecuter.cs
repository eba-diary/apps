using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sentry.data.Infrastructure
{
    public class DeadSparkJobSqlExecuter : IDbExecuter
    {
        public void ExecuteCommand(object parameter)
        {
            throw new System.NotSupportedException();
        }

        public DataTable ExecuteQuery(DateTime startDate, DateTime endDate)
        {
            //connect to database
            using (SqlConnection connection = new SqlConnection(Configuration.Config.GetHostSetting("DatabaseConnectionString")))
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "usp_GetDeadJobs";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;
                cmd.CommandTimeout = 300;

                //add parameter for time window of jobs created
                cmd.Parameters.Add(new SqlParameter("StartDate", startDate));
                cmd.Parameters.Add(new SqlParameter("endDate", endDate));

                DataTable dataTable = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                adapter.Fill(dataTable);

                return dataTable;
            }
        }
    }
}
