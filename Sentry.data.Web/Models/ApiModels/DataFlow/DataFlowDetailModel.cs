using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web.Models.ApiModels.Dataflow
{

    public class DataFlowDetailModel : DataFlowModel
    {
        public List<DataFlowStepDto> steps { get; set; }
    }
}
