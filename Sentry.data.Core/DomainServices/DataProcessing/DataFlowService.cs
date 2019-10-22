using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.DataProcessing;


namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IMessagePublisher _messagePublisher;
        

        public DataFlowService(IDatasetContext datasetContext, IMessagePublisher messagePublisher)
        {
            _datasetContext = datasetContext;
            _messagePublisher = messagePublisher;
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

        public List<DataFlowStepDto> GetDataFlowStepDtoByTrigger(string key)
        {
            List<DataFlowStep> dfsList = _datasetContext.DataFlowStep.Where(w => w.TriggerKey == key).ToList();
            List<DataFlowStepDto> dtoList = new List<DataFlowStepDto>();
            MapToDtoList(dfsList, dtoList);
            return dtoList;
        }

        public bool CreateDataFlow()
        {
            int cnt = _datasetContext.DataFlow.Count();
            DataFlow df = new DataFlow()
            {
                Name = "CreateDataFlowTest_" + cnt.ToString(),
                CreatedBy = "072984",
                CreatedDTM = DateTime.Now
            };

            _datasetContext.Add(df);

            DataFlowStep step1 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.S3DropAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.S3Drop
            };

            AddDataFlowStep(df, step1);
            _datasetContext.Add(step1);

            DataFlowStep step2 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.RawStorageAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.RawStorage
            };

            AddDataFlowStep(df, step2);
            _datasetContext.Add(step2);

            DataFlowStep step3 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.QueryStorageAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.QueryStorage
            };

            AddDataFlowStep(df, step3);
            _datasetContext.Add(step3);



            _datasetContext.SaveChanges();

            return true;
        }

        
        //public bool GenerateJobRequest(int dataFlowStepId, string sourceBucket, string sourceKey, string executionGuid)
        //{
        //    DataFlowStep step = _datasetContext.GetById<DataFlowStep>(dataFlowStepId);
        //    step.GenerateStartEvent(sourceBucket, sourceKey, executionGuid);
        //    //string JobEvent = step.GenerateStartEvent(sourceBucket, sourceKey, );

        //    //_messagePublisher.PublishDSCEvent($"{step.DataFlow.Id}-{step.Id}", JobEvent);

        //    return true;
        //}

        #region Private Methods
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

        private void MapToDto(DataFlowStep step, DataFlowStepDto dto)
        {
            dto.Id = step.Id;
            dto.ActionId = step.Action.Id;
            dto.DataActionType = step.DataAction_Type_Id;
            dto.ExeuctionOrder = step.ExeuctionOrder;
            dto.ActionName = step.Action.Name;
            dto.TriggerKey = step.TriggerKey;
            dto.TargetPrefix = step.Action.TargetStoragePrefix;
        }

        private void MapToDtoList(List<DataFlowStep> steps, List<DataFlowStepDto> dtoList)
        {
            foreach (DataFlowStep step in steps)
            {
                DataFlowStepDto stepDto = new DataFlowStepDto();
                MapToDto(step, stepDto);
                dtoList.Add(stepDto);
            }
        }

        private void AddDataFlowStep(DataFlow df, DataFlowStep step)
        {
            if (df.Steps == null)
            {
                df.Steps = new List<DataFlowStep>();
            }

            //not the first step in list, get previous step to determine trigger
            SetTriggerKey(step, df.Steps.OrderByDescending(o => o.ExeuctionOrder).Take(1).FirstOrDefault());
            SetTargetPrefix(step);

            //Set exeuction order
            step.ExeuctionOrder = df.Steps.Count + 1;

            //Add to DataFlow
            df.Steps.Add(step);
        }

        private void SetTargetPrefix(DataFlowStep step)
        {
            step.TargetPrefix = $"{step.Action.TargetStoragePrefix}{step.DataFlow.Id}/";
        }

        private string GetTargetKey(DataFlowStep step)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(step.Action.TargetStoragePrefix);
            sb.Append(step.DataFlow.Id + "/");
            return sb.ToString();
        }

        private void SetTriggerKey(DataFlowStep step, DataFlowStep previousStep)
        {
            if (previousStep == null)
            {
                switch (step.DataAction_Type_Id)
                {
                    case DataActionType.S3Drop:
                        step.TriggerKey = $"{step.DataFlow.Id}/";
                        break;
                    //case DataActionType.None:
                    //case DataActionType.RawStorage:
                    //case DataActionType.QueryStorage:
                    //case DataActionType.SchemaLoad:
                    //case DataActionType.ConvertParquet:
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                step.TriggerKey = GetTargetKey(previousStep);
            }

        }
        #endregion
    }
}
