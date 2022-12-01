using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nest;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Job;
using Sentry.data.Web.Models.ApiModels.Job;

namespace Sentry.data.Web.Extensions
{
    public static class JobExtensions
    {
        public static void ToModel(Core.Submission dto, SubmissionModel model)
        {
            model.Submission_Id = dto.SubmissionId;
            model.Job_Id = dto.JobId.Id;
            model.JobGuid = dto.JobGuid.ToString();
            model.Serialized_Job_Options = dto.Serialized_Job_Options;
            model.Created_DTM = dto.Created.ToString();
            model.FlowExecutionGuid = dto.FlowExecutionGuid;
            model.RunInstanceGuid = dto.RunInstanceGuid;
        }

        public static List<SubmissionModel> ToSubmissionModel(this List<Core.Submission> dtoList)
        {
            List<SubmissionModel> modelList = new List<SubmissionModel>();

            foreach(Core.Submission dto in dtoList)
            {
                SubmissionModel model = new SubmissionModel();
                ToModel(dto, model);
                modelList.Add(model);
            }

            return (modelList);
        }

        public static DfsMonitorModel ToModel(this DfsMonitorDto dto)
        {
            return new DfsMonitorModel()
            {
                JobId = dto.JobId,
                MonitorTarget = dto.MonitorTarget
            };
        }

        public static JavaOptionsOverrideDto ToDto(this JavaOptionsOverride options) 
        {
            return new JavaOptionsOverrideDto()
            {
                Arguments = options.Arguments,
                ConfigurationParameters = options.ConfigurationParameters,
                DriverMemory = options.DriverMemory,
                DriverCores = options.DriverCores,
                ExecutorMemory = options.ExecutorMemory,
                ExecutorCores = options.ExecutorCores,
                NumExecutors = options.NumExecutors,
                FlowExecutionGuid = (options.FlowExecutionGuid) ?? "00000000000000000",
                RunInstanceGuid = (options.RunInstanceGuid) ?? "00000000000000000",
                ClusterUrl = options.ClusterUrl
            };
        }
    }
}