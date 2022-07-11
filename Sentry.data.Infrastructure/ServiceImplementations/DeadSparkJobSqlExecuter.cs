using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sentry.data.Infrastructure
{
    public class DeadSparkJobSqlExecuter : IDbExecuter<DeadSparkJob>
    {
        public void ExecuteCommand(object parameter)
        {
            throw new System.NotSupportedException();
        }

        public List<DeadSparkJob> ExecuteQuery(int timeCreated)
        {
            using (SqlConnection connection = new SqlConnection(Configuration.Config.GetHostSetting("DatabaseConnectionString")))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("dbo.uspTest", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@timeCheck", timeCreated);

                /*SqlCommand command = new SqlCommand("exec usp_GetDeadJobs @JobID, @TimeCreated", connection);

                command.Parameters.AddWithValue("@JobID", System.Data.SqlDbType.Int);
                command.Parameters["@JobID"].Value = Configuration.Config.GetHostSetting("DeadDataSparkJobID");

                command.Parameters.AddWithValue("@TimeCreated", System.Data.SqlDbType.Int);
                command.Parameters["@TimeCreated"].Value = timeCreated;*/

                DataTable dataTable = new DataTable(); 

                SqlDataAdapter adapter = new SqlDataAdapter(command);

                adapter.Fill(dataTable);

                List<DeadSparkJob> deadSparkJobList = new List<DeadSparkJob>();

                if(dataTable.Rows != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        DeadSparkJob deadSparkJob = new DeadSparkJob();

                        deadSparkJob.SubmissionID = int.Parse(row["Submission_id"].ToString());
                        deadSparkJob.SubmissionCreated = Convert.ToDateTime(row["sub_Created"]);
                        deadSparkJob.DatasetName = row["Dataset_NME"].ToString();
                        deadSparkJob.SchemaName = row["Schema_NME"].ToString();
                        deadSparkJob.SourceBucketName = row["SourceBucketName"].ToString();
                        deadSparkJob.SourceKey = row["SourceKey"].ToString();
                        deadSparkJob.TargetKey = row["TargetKey"].ToString();
                        deadSparkJob.BatchID = int.Parse(row["BatchId"].ToString());
                        deadSparkJob.State = row["State"].ToString();
                        deadSparkJob.LivyAppID = row["LivyAppId"].ToString();
                        deadSparkJob.LivyDriverlogUrl = row["LivyDriverlogUrl"].ToString();
                        deadSparkJob.LivySparkUiUrl = row["LivySparkUiUrl"].ToString();
                        deadSparkJob.DayOfMonth = int.Parse(row["Day of Month"].ToString());
                        deadSparkJob.HourOfDay = int.Parse(row["Hour of Day"].ToString());
                        deadSparkJob.TriggerKey = row["TriggerKey"].ToString();
                        deadSparkJob.TriggerBucket = row["TriggerBucket"].ToString();
                        deadSparkJob.ExecutionGuid = row["ExecutionGuid"].ToString();
                        deadSparkJob.DatasetID = int.Parse(row["Dataset_ID"].ToString());
                        deadSparkJob.SchemaID = int.Parse(row["Schema_ID"].ToString());
                        deadSparkJob.DatasetFileID = int.Parse(row["DatasetFile_ID"].ToString());
                        deadSparkJob.DataFlowID = int.Parse(row["DataFlow_ID"].ToString());
                        deadSparkJob.DataFlowStepID = int.Parse(row["DataFlowStep_ID"].ToString());

                        deadSparkJobList.Add(deadSparkJob);
                    }
                }

                return deadSparkJobList;
            }
        }
    }
}
