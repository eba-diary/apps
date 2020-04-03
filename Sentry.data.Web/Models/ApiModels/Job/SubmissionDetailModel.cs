using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Job
{
    public class SubmissionDetailModel : SubmissionModel
    {
        public string LastStatus { get; set; }
        public List<JobHistoryModel> JobHistory { get; set; }
    }
}