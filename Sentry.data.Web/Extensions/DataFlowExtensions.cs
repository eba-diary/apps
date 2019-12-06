using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Sentry.data.Web
{
    public static class DataFlowExtensions
    {
        public static List<DFModel> ToModelList(this List<Core.DataFlowDto> dtoList)
        {
            List<DFModel> modelList = new List<DFModel>();
            foreach (Core.DataFlowDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }
        public static DFModel ToModel(this Core.DataFlowDto dto)
        {
            return new DFModel(dto) { };
        }
    }
}