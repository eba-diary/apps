using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Job
{
    public class SubmissionModel
    {
        public int Submission_Id { get; set; }
        public int Job_Id { get; set; }
        public string JobGuid { get; set; }
        public string Serialized_Job_Options { get; set; }
        public string Created_DTM { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string RunInstanceGuid { get; set; }
    }
}