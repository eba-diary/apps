using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Web.Models.ApiModels.Job;

namespace Sentry.data.Web.Extensions
{
    public static class JobExtensions
    {
        public static SubmissionModel ToSubmissionModel(this Core.Submission dto)
        {
            return new SubmissionModel()
            {
                Submission_Id = dto.SubmissionId,
                Job_Id = dto.JobId.Id,
                JobGuid = dto.JobGuid.ToString(),
                Serialized_Job_Options = dto.Serialized_Job_Options,
                Created_DTM = dto.Created.ToString()
            };
        }

        public static List<SubmissionModel> ToSubmissionModel(this List<Core.Submission> dtoList)
        {
            List<SubmissionModel> modelList = new List<SubmissionModel>();

            foreach(Core.Submission dto in dtoList)
            {
                modelList.Add(dto.ToSubmissionModel());
            }

            return (modelList);
        }
    }
}