using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DeadSparkJobService : IDeadSparkJobService
    {
        private readonly IDeadJobProvider _deadJobProvider;

        public DeadSparkJobService(IDeadJobProvider deadJobProvider)
        {
            _deadJobProvider = deadJobProvider;
        }  

        public List<DeadSparkJobDto> GetDeadSparkJobDtos(DateTime timeCreated)
        {
            List<DeadSparkJob> deadSparkJobList = _deadJobProvider.GetDeadSparkJobs(timeCreated);

            return MapToDtoList(deadSparkJobList);
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

        private List<DeadSparkJobDto> MapToDtoList(List<DeadSparkJob> deadSparkJobList)
        {
            List<DeadSparkJobDto> deadSparkJobDtoList = new List<DeadSparkJobDto>();

            deadSparkJobList.ForEach(x => deadSparkJobDtoList.Add(MapToDto(x)));

            return deadSparkJobDtoList;
        }
    }
}