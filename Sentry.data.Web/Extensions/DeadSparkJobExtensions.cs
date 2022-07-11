using Newtonsoft.Json;
using Sentry.Configuration;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{

    public static class DeadSparkJobExtensions
    {
        public static List<DeadSparkJobModel> MapToModelList(this List<DeadSparkJobDto> deadSparkJobDtoList)
        {
            List<DeadSparkJobModel> modelList = new List<DeadSparkJobModel>();

            deadSparkJobDtoList.ForEach(model => modelList.Add(model.MapToModel()));

            return modelList;
        }

        public static DeadSparkJobModel MapToModel(this DeadSparkJobDto deadSparkJobDto)
        {
            DeadSparkJobModel model = new DeadSparkJobModel
            {
                SubmissionTime = deadSparkJobDto.SubmissionTime, 
                DatasetName = deadSparkJobDto.DatasetName,
                SchemaName = deadSparkJobDto.SchemaName,
                SourceKey = deadSparkJobDto.SourceKey,
                FlowExecutionGuid = deadSparkJobDto.FlowExecutionGuid,
                ReprocessingRequired = deadSparkJobDto.ReprocessingRequired,
                SubmissionID = deadSparkJobDto.SubmissionID,
                SourceBucketName = deadSparkJobDto.SourceBucketName,
                BatchID = deadSparkJobDto.BatchID,
                LivyAppID = deadSparkJobDto.LivyAppID,
                LivyDriverlogUrl = deadSparkJobDto.LivyDriverlogUrl,
                LivySparkUiUrl = deadSparkJobDto.LivySparkUiUrl,
                DatasetFileID = deadSparkJobDto.DatasetFileID,
                DataFlowStepID = deadSparkJobDto.DataFlowStepID
            };

            return model;
        }
    }
}