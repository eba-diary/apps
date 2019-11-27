using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Core
{
    public class DataStepService
    {
        public readonly IDatasetContext _datasetContext;

        public DataStepService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public List<DataFlowStepDto> ListAllDataStepsByDataFlow(int dataFlowId)
        {
            List<DataFlowStep> dfList = _datasetContext.DataFlowStep.Where(w => w.DataFlow.Id == dataFlowId).ToList();
            List<DataFlowStepDto> dtoList = new List<DataFlowStepDto>();
            foreach(DataFlowStep step in dfList)
            {
                DataFlowStepDto dto = new DataFlowStepDto();

                dto.Id = step.Id;
                dto.DataFlowId = step.DataFlow.Id;
                dto.DataFlowName = step.DataFlow.Name;
                dto.DataActionTypeId = (int)step.DataAction_Type_Id;
                dto.DataActionName = step.DataAction_Type_Id.ToString();
                dto.ActionId = step.Action.Id;
                dto.ActionName = step.Action.Name;
                dto.ExeuctionOrder = step.ExeuctionOrder;

                dtoList.Add(dto);
            }

            return dtoList;
        }
    }
}
