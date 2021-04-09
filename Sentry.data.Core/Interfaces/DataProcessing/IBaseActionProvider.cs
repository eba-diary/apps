﻿using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IBaseActionProvider
    {
        void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent);
        Task ExecuteActionAsync(DataFlowStep step, DataFlowStepEvent stepEvent);
        void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);
        Task PublishStartEventAsync(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);
        void GenerateDependencyTargets(DataFlowStepEvent stepEvent);
    }
}
