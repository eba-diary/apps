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
        private readonly IS3ServiceProvider _s3ServiceProvider;

        public DeadSparkJobService(IDeadJobProvider deadJobProvider, IS3ServiceProvider s3ServiceProvider)
        {
            _deadJobProvider = deadJobProvider;
            _s3ServiceProvider = s3ServiceProvider;
        }

        public List<DeadSparkJobDto> GetDeadSparkJobDtos(DateTime timeCreated)
        {
            List<DeadSparkJob> deadSparkJobList = _deadJobProvider.GetDeadSparkJobs(timeCreated);

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
            List<string> s3ObjectList = _s3ServiceProvider.ListObjects(deadSparkJob.SourceBucketName, deadSparkJob.TargetKey).ToList();

            bool reprocessingRequired = s3ObjectList.Any(s3Object => s3Object.Contains("_SUCCESS")) ? false : true;

            DeadSparkJobDto deadSparkJobDto = new DeadSparkJobDto
            {
                SubmissionTime = deadSparkJob.SubmissionCreated,
                DatasetName = deadSparkJob.DatasetName,
                SchemaName = deadSparkJob.SchemaName,
                SourceKey = deadSparkJob.SourceKey,
                FlowExecutionGuid = deadSparkJob.FlowExecutionGuid,
                RunInstanceGuid = deadSparkJob.RunInstanceGuid,
                ReprocessingRequired = reprocessingRequired,
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