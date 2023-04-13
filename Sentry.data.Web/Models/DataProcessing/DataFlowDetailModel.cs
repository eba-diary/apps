using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class DataFlowDetailModel : DFModel
    {
        public DataFlowDetailModel(DataFlowDetailDto dto) : base(dto)
        {
            Step = new List<DataFlowStepModel>();
            foreach (DataFlowStepDto stepDto in dto.steps)
            {
                Step.Add(stepDto.ToModel());
            };

        }
        public List<DataFlowStepModel> Step { get; set; }
        public List<SchemaMapModel> MappedSchema { get; set; }
        public UserSecurity UserSecurity { get; set; }
        public bool DisplayDataflowEdit { get; set; }
        public string ProducerAssetGroupName { get; set; }
    }
}