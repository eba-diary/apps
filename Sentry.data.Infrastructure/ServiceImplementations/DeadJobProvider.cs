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

namespace Sentry.data.Infrastructure
{
    public class DeadJobProvider : IDeadJobProvider
    {
        public readonly IDbExecuter _dbExecuter;

        public DeadJobProvider(IDbExecuter dbExecuter)
        {
            _dbExecuter = dbExecuter;
        }

        public List<DeadSparkJobDto> GetDeadSparkJobDtos(int timeCreated)
        {
            DataTable dataTable = _dbExecuter.ExecuteQuery(timeCreated);

            List<DeadSparkJob> deadSparkJobList = MapToEntity(dataTable);

            return MapToDtoList(deadSparkJobList);
        }

        private List<DeadSparkJobDto> MapToDtoList(List<DeadSparkJob> deadSparkJobList)
        {
            List<DeadSparkJobDto> deadSparkJobDtoList = new List<DeadSparkJobDto>();

            deadSparkJobList.ForEach(x => deadSparkJobDtoList.Add(MapToDto(x)));

            return deadSparkJobDtoList;
        }

        private List<DeadSparkJob> MapToEntity(DataTable dataTable)
        {
            List<DeadSparkJob> deadSparkJobList = new List<DeadSparkJob>();

            if (dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    DeadSparkJob deadSparkJob = new DeadSparkJob();

                    deadSparkJob.SubmissionID = int.Parse(row["Submission_id"].ToString());
                    deadSparkJob.SubmissionCreated = Convert.ToDateTime(row["sub_Created"]);
                    deadSparkJob.DatasetName = row["Dataset_NME"].ToString().Trim();
                    deadSparkJob.SchemaName = row["Schema_NME"].ToString().Trim();
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

        private DeadSparkJobDto MapToDto(DeadSparkJob deadSparkJob)
        {
            string isReprocessingRequired = deadSparkJob.TargetKey.Contains("_SUCCESS") ? "Yes" : "No";

            DeadSparkJobDto deadSparkJobDto = new DeadSparkJobDto
            {
                SubmissionTime = deadSparkJob.SubmissionCreated,
                DatasetName = deadSparkJob.DatasetName,
                SchemaName = deadSparkJob.SchemaName,
                SourceKey = deadSparkJob.SourceKey,
                FlowExecutionGuid = deadSparkJob.ExecutionGuid,
                ReprocessingRequired = isReprocessingRequired,
                SubmissionID = deadSparkJob.SubmissionID,
                SourceBucketName = deadSparkJob.SourceBucketName,
                BatchID = deadSparkJob.BatchID,
                LivyAppID = deadSparkJob.LivyAppID,
                LivyDriverlogUrl = deadSparkJob.LivyDriverlogUrl,
                LivySparkUiUrl = deadSparkJob.LivySparkUiUrl,
                DatasetFileID = deadSparkJob.DatasetFileID,
                DataFlowStepID = deadSparkJob.DataFlowStepID
            };

            return deadSparkJobDto;
        }
    }
}
