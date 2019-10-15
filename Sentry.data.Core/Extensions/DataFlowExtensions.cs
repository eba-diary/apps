using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities;
using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public static class DataFlowExtensions
    {
        public static List<DataFlowDto> ToDtoList(this List<DataFlow> dfList)
        {
            List<DataFlowDto> dtoList = new List<DataFlowDto>();
            foreach(DataFlow df in dfList)
            {
                dtoList.Add(df.ToDto());
            }
            return dtoList;
        }

        public static DataFlowDto ToDto(this DataFlow df)
        {
            return new DataFlowDto()
            {
                Id = df.Id,
                FlowGuid = df.FlowGuid,
                Name = df.Name,
                CreateDTM = df.CreatedDTM,
                CreatedBy = df.CreatedBy
            };
        }

        public static string GenerateStartEvent(this DataFlowStep step, string bucket, string key)
        {
            DataFlowStepEvent stepEvent = new DataFlowStepEvent()
            {
                DataFlowId = step.DataFlow.Id,
                DataFlowGuid = step.DataFlow.FlowGuid.ToString(),
                ActionId = step.Action.Id,
                ActionGuid = step.Action.ActionGuid.ToString(),
                SourceBucket = bucket,
                SourceKey = key,
                TargetBucket = step.Action.TargetStorageBucket,
                TargetPrefix = step.Action.TargetStoragePrefix
            };

            switch (step.DataAction_Type_Id)
            {
                case DataActionType.S3Drop:
                    stepEvent.EventType = GlobalConstants.DataFlowStepEvent.S3_DROP_START;
                    break;
                case DataActionType.RawStorage:
                    stepEvent.EventType = GlobalConstants.DataFlowStepEvent.RAW_STORAGE_START;
                    break;
                case DataActionType.QueryStorage:
                    stepEvent.EventType = GlobalConstants.DataFlowStepEvent.QUERY_STORAGE;
                    break;
                case DataActionType.SchemaLoad:
                    stepEvent.EventType = GlobalConstants.DataFlowStepEvent.SCHEMA_LOAD;
                    break;
                case DataActionType.ConvertParquet:
                    stepEvent.EventType = GlobalConstants.DataFlowStepEvent.CONVERT_TO_PARQUET;
                    break;
                case DataActionType.None:
                default:
                    throw new NotImplementedException();
            }

            return JsonConvert.SerializeObject(stepEvent);
        }

        public static string GetDataFlowStepPrefix(string key)
        {
            string filePrefix = null;
            //three level prefixes
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX))
            {
                return key.Substring(0, key.Length - GetNthIndex(key, '/', 3));
            }
            //two level prefixes
            // example -  <rawstorage prefix>/<job Id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX))
            {
                return key.Substring(0, key.Length - GetNthIndex(key, '/', 2));
            }
            return null;
        }


        public static int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
