﻿using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using Sentry.data.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseActionProvider : IBaseActionProvider
    {
        private readonly IDataFlowService _dataFlowService;
        private static Random random = new Random();
        private JObject _metricData;

        public JObject MetricData
        {
            get
            {
                if (_metricData == null)
                {
                    _metricData = new JObject();
                    return _metricData;
                }

                return _metricData;
            }
            set { _metricData = value; }
        }

        public BaseActionProvider(IDataFlowService dataFlowService)
        {
            _dataFlowService = dataFlowService;
        }

        public abstract void ExecuteAction(DataFlowStep step, DataFlowStepEvent stepEvent);
        public abstract Task ExecuteActionAsync(DataFlowStep step, DataFlowStepEvent stepEvent);

        public abstract void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);

        public abstract Task PublishStartEventAsync(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event);

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
                    BucketName = dStep.TriggerBucket,
                    ObjectKey = dStep.TriggerKey + $"{((storageCode == null) ? string.Empty : storageCode + "/")}{DataFlowHelpers.GenerateGuid(stepEvent.FlowExecutionGuid, stepEvent.RunInstanceGuid)}/"                    
                };

                targets.Add(target);
            }
            stepEvent.DownstreamTargets = targets;
        }

        protected static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
