using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.NHibernate;
using NHibernate;
using System.Data.SqlClient;
using System.Data;
using Sentry.data.Core.Helpers;

namespace Sentry.data.Infrastructure
{
    public class DeadJobProvider : IDeadJobProvider
    {
        public readonly IDbExecuter _dbExecuter;

        public DeadJobProvider(IDbExecuter dbExecuter)
        {
            _dbExecuter = dbExecuter;
        }

        public List<DeadSparkJob> GetDeadSparkJobs(DateTime timeCreated)
        {
            DataTable dataTable = _dbExecuter.ExecuteQuery(timeCreated);

            return MapToEntity(dataTable);
        }

        

        private List<DeadSparkJob> MapToEntity(DataTable dataTable)
        {
            List<DeadSparkJob> deadSparkJobList = new List<DeadSparkJob>();

            if (dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    DeadSparkJob deadSparkJob = new DeadSparkJob();

                    deadSparkJob.SubmissionID =         DatabaseHelper.SafeDatabaseInt(row["Submission_id"]);
                    deadSparkJob.BatchID =              DatabaseHelper.SafeDatabaseInt(row["BatchId"]);
                    deadSparkJob.DayOfMonth =           DatabaseHelper.SafeDatabaseInt(row["Day of Month"]);
                    deadSparkJob.HourOfDay =            DatabaseHelper.SafeDatabaseInt(row["Hour of Day"]);
                    deadSparkJob.DatasetID =            DatabaseHelper.SafeDatabaseInt(row["Dataset_ID"]);
                    deadSparkJob.SchemaID =             DatabaseHelper.SafeDatabaseInt(row["Schema_ID"]);
                    deadSparkJob.DatasetFileID =        DatabaseHelper.SafeDatabaseInt(row["DatasetFile_ID"]);
                    deadSparkJob.DataFlowID =           DatabaseHelper.SafeDatabaseInt(row["DataFlow_ID"]);
                    deadSparkJob.DataFlowStepID =       DatabaseHelper.SafeDatabaseInt(row["DataFlowStep_ID"]);
                    deadSparkJob.SubmissionCreated =    DatabaseHelper.SafeDatabaseDate(row["sub_Created"]);
                    deadSparkJob.DatasetName =          DatabaseHelper.SafeDatabaseString(row["Dataset_NME"]);
                    deadSparkJob.SchemaName =           DatabaseHelper.SafeDatabaseString(row["Schema_NME"]);
                    deadSparkJob.SourceBucketName =     DatabaseHelper.SafeDatabaseString(row["SourceBucketName"]);
                    deadSparkJob.SourceKey =            DatabaseHelper.SafeDatabaseString(row["SourceKey"]);
                    deadSparkJob.TargetKey =            DatabaseHelper.SafeDatabaseString(row["TargetKey"]);
                    deadSparkJob.State =                DatabaseHelper.SafeDatabaseString(row["State"]);
                    deadSparkJob.LivyAppID =            DatabaseHelper.SafeDatabaseString(row["LivyAppId"]);
                    deadSparkJob.LivyDriverlogUrl =     DatabaseHelper.SafeDatabaseString(row["LivyDriverlogUrl"]);
                    deadSparkJob.LivySparkUiUrl =       DatabaseHelper.SafeDatabaseString(row["LivySparkUiUrl"]);
                    deadSparkJob.TriggerKey =           DatabaseHelper.SafeDatabaseString(row["TriggerKey"]);
                    deadSparkJob.TriggerBucket =        DatabaseHelper.SafeDatabaseString(row["TriggerBucket"]);
                    deadSparkJob.FlowExecutionGuid =    DatabaseHelper.SafeDatabaseString(row["FlowExecutionGuid"]);

                    deadSparkJobList.Add(deadSparkJob);
                }
            }

            return deadSparkJobList;
        }
    }
}
