using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Sentry.data.Web
{
    public static class DataFlowExtensions
    {
        public static List<DataFlowModel> ToModelList(this List<Core.DataFlowDto> dtoList)
        {
            List<DataFlowModel> modelList = new List<DataFlowModel>();
            foreach (Core.DataFlowDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }
        public static DataFlowModel ToModel(this Core.DataFlowDto dto)
        {
            return new DataFlowModel(dto) { };
        }
    }
}