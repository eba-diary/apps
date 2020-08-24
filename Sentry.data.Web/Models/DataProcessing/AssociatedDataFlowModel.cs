using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class AssociatedDataFlowModel : DataFlowDetailModel
    {
        public AssociatedDataFlowModel(Core.DataFlowDetailDto dto) : base(dto) { }
        public AssociatedDataFlowModel(Tuple<Core.DataFlowDetailDto, List<Core.RetrieverJob>> dtoList) : base(dtoList.Item1)
        {
            RetrieverJobs = dtoList.Item2;
        }
        public List<Core.RetrieverJob> RetrieverJobs { get; set; }
    }
}