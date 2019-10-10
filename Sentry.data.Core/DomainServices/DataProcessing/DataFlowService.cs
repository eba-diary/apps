using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;

        public DataFlowService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public List<DataFlowDto> ListDataFlows()
        {
            List<DataFlow> dfList = _datasetContext.DataFlow.ToList();
            List<DataFlowDto> dtoList = new List<DataFlowDto>();
            MapToDtoList(dfList, dtoList);
            return dtoList;
        }

        public DataFlowDetailDto GetDataFlowDetailDto(int id)
        {
            DataFlow df = _datasetContext.GetById<DataFlow>(id);
            DataFlowDetailDto dto = new DataFlowDetailDto();
            MapToDetailDto(df, dto);
            return dto;
        }





        private void MapToDtoList(List<DataFlow> dfList, List<DataFlowDto> dtoList)
        {
            foreach (DataFlow df in dfList)
            {
                DataFlowDto dfDto = new DataFlowDto();
                MapToDto(df, dfDto);
                dtoList.Add(dfDto);
            }
        }

        private void MapToDto(DataFlow df, DataFlowDto dto)
        {
            dto.Id = df.Id;
            dto.FlowGuid = df.FlowGuid;
            dto.Name = df.Name;
            dto.CreateDTM = df.CreatedDTM;
            dto.CreatedBy = df.CreatedBy;
        }

        private void MapToDetailDto(DataFlow flow, DataFlowDetailDto dto)
        {
            List<DataFlowStepDto> stepDtoList = new List<DataFlowStepDto>();
            MapToDtoList(flow.Steps.ToList(), stepDtoList);

            dto.steps = stepDtoList;

            MapToDto(flow, dto);

        }

        private void MapToDto (DataFlowStep step, DataFlowStepDto dto)
        {
            dto.Id = step.Id;
            dto.ActionId = step.Action.Id;
            dto.ExeuctionOrder = step.ExeuctionOrder;
        }

        private void MapToDtoList (List<DataFlowStep> steps, List<DataFlowStepDto> dtoList)
        {
            foreach(DataFlowStep step in steps)
            {
                DataFlowStepDto stepDto = new DataFlowStepDto();
                MapToDto(step, stepDto);
                dtoList.Add(stepDto);
            }
        }
    }
}
