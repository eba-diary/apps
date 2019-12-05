using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DataFlowDetailModel : DataFlowModel
    {
        public DataFlowDetailModel(DataFlowDetailDto dto) : base(dto)
        {
            Step = new List<DataFlowStepModel>();
            foreach (DataFlowStepDto stepDto in dto.steps)
            {
                Step.Add(new DataFlowStepModel(stepDto));
            };            
        }
        public List<DataFlowStepModel> Step { get; set; }
    }
}