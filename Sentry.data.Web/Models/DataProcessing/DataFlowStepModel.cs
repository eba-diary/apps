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
            ActionName = dto.ActionName;
            ExecutionOrder = dto.ExeuctionOrder;
            TriggetKey = dto.TriggerKey;
            TargetPrefix = dto.TargetPrefix;
        }
        public int Id { get; set; }
        public int ActionId { get; set; }
        public string ActionName { get; set; }
        public int ExecutionOrder { get; set; }
        public string TriggetKey { get; set; }
        public string TargetPrefix { get; set; }
    }
}