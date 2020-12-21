using Sentry.data.Core.Entities.DataProcessing;
using System.Collections.Generic;

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
                SaidKeyCode = df.SaidKeyCode,
                Name = df.Name,
                CreateDTM = df.CreatedDTM,
                CreatedBy = df.CreatedBy
            };
        }

        public static SchemaMapDto ToDto(this SchemaMap scmMap)
        {
            return new SchemaMapDto()
            {
                Id = scmMap.Id,
                //DatasetId = scmMap.Dataset.DatasetId,
                SchemaId = scmMap.MappedSchema.SchemaId,
                SearchCriteria = scmMap.SearchCriteria,
                StepId = scmMap.DataFlowStepId.Id
            };
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
