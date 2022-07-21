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

        public DataTable ExecuteQuery(int timeCreated)
        {
            //connect to database
            using (SqlConnection connection = new SqlConnection(Configuration.Config.GetHostSetting("DatabaseConnectionString")))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("exec usp_GetDeadJobs @JobID, @TimeCreated", connection);

                //add parameter for current enviroment (@JobId)
                command.Parameters.AddWithValue("@JobID", System.Data.SqlDbType.Int);
                command.Parameters["@JobID"].Value = Configuration.Config.GetHostSetting("DeadDataSparkJobID");

                //add parameter for time window of jobs created (@TimeCreated) 
                command.Parameters.AddWithValue("@TimeCreated", System.Data.SqlDbType.Int);
                command.Parameters["@TimeCreated"].Value = timeCreated;

                DataTable dataTable = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(command);

                adapter.Fill(dataTable);

                return dataTable;
            }
        }
    }
}
