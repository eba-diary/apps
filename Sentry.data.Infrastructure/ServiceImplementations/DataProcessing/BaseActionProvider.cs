using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseActionProvider : IBaseActionProvider
    {
        private readonly IDataFlowService _dataFlowService;

        public BaseActionProvider(IDataFlowService dataFlowService)
        {
            _dataFlowService = dataFlowService;
        }

        public abstract void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent);

        public abstract void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);

        public void GenerateDependencyTargets(DataFlowStepEvent stepEvent)
        {
            List<DataFlowStepEventTarget> targets = new List<DataFlowStepEventTarget>();
            /****************************************
            *  Find all dependent data flow steps
            ****************************************/
            List<DataFlowStep> dependentSteps = _dataFlowService.GetDependentDataFlowStepsForDataFlowStep(stepEvent.StepId);

            foreach (DataFlowStep dStep in dependentSteps)
            {
                /**************************************
                *  If downsteam step has schema aware target storage, find storage code
                *  If not, storage key will remain null and not be included in resulting ObjectKey
                **************************************/
                string storageCode = null;
                if (dStep.Action.TargetStorageSchemaAware)
                {
                    storageCode = _dataFlowService.GetSchemaStorageCodeForDataFlow(dStep.DataFlow.Id);
                }

                DataFlowStepEventTarget target = new DataFlowStepEventTarget
                {
                    BucketName = dStep.Action.TargetStorageBucket,
                    ObjectKey = dStep.TriggerKey + $"{((storageCode == null) ? string.Empty : storageCode + "/")}{stepEvent.FlowExecutionGuid}{((stepEvent.RunInstanceGuid == null) ? String.Empty : "-" + stepEvent.RunInstanceGuid)}/"
                };

                targets.Add(target);
            }
            stepEvent.DownstreamTargets = targets;
        }
    }
}
