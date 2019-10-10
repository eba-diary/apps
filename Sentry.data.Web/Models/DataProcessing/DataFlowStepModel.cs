using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Web
{
    public class DataFlowStepModel
    {
        public DataFlowStepModel(DataFlowStepDto dto)
        {
            Id = dto.Id;
            ActionId = dto.ActionId;
            ExecutionOrder = dto.ExeuctionOrder;
        }
        public int Id { get; set; }
        public int ActionId { get; set; }
        public int ExecutionOrder { get; set; }
    }
}