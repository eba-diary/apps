using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Sentry.data.Web.Models.ApiModels.DatasetFile
{
    public class DatasetFileReprocessModel
    {
        public int DataFlowStepId { get; set; }
        public List<int> DatasetFileIds { get; set; }
    }
}