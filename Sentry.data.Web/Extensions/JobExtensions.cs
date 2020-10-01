using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public static List<DfsMonitorModel> ToModel(this List<Core.RetrieverJob> dtoList)
        {
            List<DfsMonitorModel> modelList = new List<DfsMonitorModel>();

            foreach (Core.RetrieverJob dto in dtoList)
            {
                DfsMonitorModel model = new DfsMonitorModel()
                {
                    JobId = dto.Id,
                    MonitorTarget = dto.DataSource.CalcRelativeUri(dto).LocalPath
                };
                
                modelList.Add(model);
            }

            return (modelList);
        }
        
    }
}