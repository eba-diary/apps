using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities;

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
    }
}
