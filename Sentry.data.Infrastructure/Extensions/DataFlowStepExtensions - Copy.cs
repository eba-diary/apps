using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure
{
    public static class DataFlowStepExtensions
    {
        public static string GenerateStepEventKey(DataFlowStep step)
        {
            return $"{step.DataFlow.FlowStorageCode}-{step.Id}";
        }
    }
}
