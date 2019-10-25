using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.Common.Logging;
using Sentry.data.Core.Interfaces.DataProcessing;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class DataFlowProvider : IDataFlowProvider
    {

        private List<DataFlow_Log> logs = new List<DataFlow_Log>();
        private DataFlow _flow;
        private string flowExecutionGuid = null;
        private string runInstanceGuid = null;

        public async Task ExecuteDependenciesAsync(S3ObjectEvent s3e)
        {
            await ExecuteDependenciesAsync(s3e.s3.bucket.name, s3e.s3._object.key);
            //await ExecuteDependenciesAsync("sentry-dataset-management-np-nr", "data/17/TestFile.csv");
        }
        public async Task ExecuteDependenciesAsync(string bucket, string key)
        {
            bool IsNewFile = true;

            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext dsContext = container.GetInstance<IDatasetContext>();
                IDataFlowService dfService = container.GetInstance<IDataFlowService>();
                IDataStepService _stepService = container.GetInstance<IDataStepService>();
                //IMessagePublisher messagePublisher = container.GetInstance<IMessagePublisher>();

                Logger.Info($"start-method <executedependencies>");

                //Get prefix
                string stepPrefix = GetDataFlowStepPrefix(key);
                if (stepPrefix != null)
                {
                    try
                    {
                        //Find Dependencies
                        List<DataFlowStep> stepList = dsContext.DataFlowStep.Where(w => w.TriggerKey == stepPrefix).ToList();

                        _flow = stepList.Select(s => s.DataFlow).Distinct().Single();



                        //determine DataFlow execution and run instance guids to ensure processing is tied.
                        GetExecutionGuids(key);

                        if (flowExecutionGuid == null)
                        {
                            int Epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                            flowExecutionGuid = Epoch.ToString();

                            IsNewFile = true;

                            _flow.Logs.Add(_flow.LogExecution(flowExecutionGuid, $"Initialize flow execution bucket:{bucket}, key:{key}, file:{Path.GetFileName(key)}", Log_Level.Info));
                        };


                        //log dependency steps
                        LogDetectedSteps(key, stepList, flowExecutionGuid, _flow);

                        //save new logs
                        dsContext.SaveChanges();

                        //Generate Start Events
                        foreach (DataFlowStep step in stepList)
                        {
                            //Check if rerun scenario, if so generate a runinstanceguid
                            if (step.Executions.Where(w => w.FlowExecutionGuid == flowExecutionGuid).Any() && runInstanceGuid == null)
                            {
                                int InstanceEpoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                runInstanceGuid = InstanceEpoch.ToString();
                            }

                            ////step.GenerateStartEvent(bucket, key, flowExecutionGuid);
                            //step.LogExecution(flowExecutionGuid, RunInstanceGuid, $"dataflowprovider-sendingstartevent", Log_Level.Debug);
                            //dsContext.SaveChanges();
                            _stepService.PublishStartEvent(step, bucket, key, flowExecutionGuid, runInstanceGuid);
                        }
                        _flow.LogExecution(flowExecutionGuid, $"end-method <executedependencies>", Log_Level.Info);
                        //save new logs
                        dsContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logs.Add(_flow.LogExecution(flowExecutionGuid, $"dataflowprovider-ExecuteDependenciesAsync-failed", Log_Level.Error, ex));
                        _flow.LogExecution(flowExecutionGuid, $"end-method <executedependencies>", Log_Level.Info);
                        foreach (var log in logs)
                        {
                            _flow.Logs.Add(log);
                        }
                        dsContext.SaveChanges();
                    }                    
                }
                else
                {
                    Logger.Info($"executedependencies - invalidstepprefix bucket: {bucket} key:{key}");
                    Logger.Info($"end-method <executedependencies>");
                }                
            }
        }

        #region Private Methods

        private void LogDetectedSteps(string key, List<DataFlowStep> stepList, string executionGuid, DataFlow flow)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"dataflowprovider_detecteddependencies {stepList.Count.ToString()} step(s) dependencies were detected for {key}");
            foreach(DataFlowStep step in stepList)
            {
                sb.AppendLine(step.ToString());
            }

            logs.Add(flow.LogExecution(executionGuid, sb.ToString(), Log_Level.Info));
        }

        protected string GetDataFlowStepPrefix(string key)
        {
            Logger.Info($"start-method <getdataflowstepprefix>");

            string filePrefix = null;
            //three level prefixes - temp locations
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX) || 
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX)
               )
            {
                int idx = GetNthIndex(key, '/', 3);
                filePrefix = key.Substring(0, (idx + 1));
            }
            //two level prefixes - non-temp locations
            // example -  <rawstorage prefix>/<job Id>/
            if (filePrefix == null && key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX)
               )
            {
                int idx = GetNthIndex(key, '/', 2);
                filePrefix = key.Substring(0, (idx + 1));
            }

            //For drop location prefixes
            // single level prefix
            // example = <data flow id>/
            if (filePrefix == null)
            {
                int idx = GetNthIndex(key, '/', 1);
                bool IsInt = int.TryParse(key.Substring(0, (idx)), out int jobId);
                bool validFlowId = false;
                if (IsInt)
                {                    
                    using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                    {
                        IDatasetContext dsContext = container.GetInstance<IDatasetContext>();
                        validFlowId = dsContext.DataFlow.Any(a => a.Id == jobId);
                    }
                }

                if (validFlowId)
                {
                    filePrefix = key.Substring(0, (idx + 1));
                }                
            }
            Logger.Info($"end-method <getdataflowstepprefix>");

            return filePrefix;            
        }

        private void GetExecutionGuids(string key)
        {
            Logger.Info($"start-method <getflowguid>");

            string guidPrefix = null;

            //four level prefixes - temp locations
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX))
            {
                int strtIdx = GetNthIndex(key, '/', 4);
                int endIdx = GetNthIndex(key, '/', 5);
                guidPrefix = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            //three level prefixes - temp locations
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX)
               )
            {
                int strtIdx = GetNthIndex(key, '/', 3);
                int endIdx = GetNthIndex(key, '/', 4);
                guidPrefix = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            //two level prefixes - non-temp locations
            // example -  <rawstorage prefix>/<job Id>/
            if (guidPrefix == null && key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX))
            {
                int strtIdx = GetNthIndex(key, '/', 2);
                int endIdx = GetNthIndex(key, '/', 3);
                guidPrefix = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            //populate runinstance guid
            if (guidPrefix != null)
            {
                int strtIdx = GetNthIndex(guidPrefix, '-', 1);

                //if a dash exists, extract the run instance guid
                runInstanceGuid = (strtIdx > 0) ? guidPrefix.Substring(strtIdx + 1, (guidPrefix.Length - strtIdx) - 1) : null;

                //if a dash exists, extract the flow execution guid
                flowExecutionGuid = (strtIdx > 0) ? guidPrefix.Substring(0, strtIdx) : guidPrefix;
            }

            Logger.Info($"end-method <getflowguid>");            
        }


        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        #endregion
    }
}
