using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.NHibernate;
using NHibernate;
using System.Data.SqlClient;

namespace Sentry.data.Infrastructure
{
    public class DeadJobProvider : IDeadJobProvider
    {
        public readonly IDbExecuter<DeadSparkJob> _dbExecuter;

        public DeadJobProvider(IDbExecuter<DeadSparkJob> dbExecuter)
        {
            _dbExecuter = dbExecuter;
        }

        public List<DeadSparkJobDto> GetDeadSparkJobDtos(int timeCreated)
        {
            List<DeadSparkJob> deadSparkJobList = _dbExecuter.ExecuteQuery(-10);

            return MapToDtoList(deadSparkJobList);
        }

        private List<DeadSparkJobDto> MapToDtoList(List<DeadSparkJob> deadSparkJobList)
        {
            List<DeadSparkJobDto> deadSparkJobDtoList = new List<DeadSparkJobDto>();

            deadSparkJobList.ForEach(x => deadSparkJobDtoList.Add(MapToDto(x)));

            return deadSparkJobDtoList;
        }

        private DeadSparkJobDto MapToDto(DeadSparkJob deadSparkJob)
        {
            bool isReprocessingRequired = deadSparkJob.TargetKey.Contains("_SUCCESS") ? false : true;

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
                DatasetFileID = deadSparkJob.DatasetFileID
            };

            return deadSparkJobDto;
        }
    }
}
