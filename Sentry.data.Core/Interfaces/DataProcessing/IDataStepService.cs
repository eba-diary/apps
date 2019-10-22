using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataStepService
    {
        void ExecuteStep(DataFlowStepEvent stepEvent);
        void PublishStartEvent(DataFlowStep step, string bucket, string key, string FlowExecutionGuid, string runInstanceGuid);
    }
}
