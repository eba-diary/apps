﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class DataFlowProvider : IDataFlowProvider
    {

        private List<EventMetric> logs = new List<EventMetric>();
        private DataFlow _flow;
        private string flowExecutionGuid;
        private string runInstanceGuid;
        private JObject _metricData;

        private JObject MetricData
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


        public async Task ExecuteDependenciesAsync(S3ObjectEvent s3e)
        {
            await ExecuteDependenciesAsync(s3e.s3.bucket.name, s3e.s3.Object.key, s3e);
        }
        public async Task ExecuteDependenciesAsync(string bucket, string key, S3ObjectEvent s3Event)
        {
            bool IsNewFile = false;

            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext dsContext = container.GetInstance<IDatasetContext>();
                IDataStepService _stepService = container.GetInstance<IDataStepService>();

                Logger.Info($"start-method <executedependencies>");
                Logger.Debug($"<executedependencies> bucket:{bucket} key:{key} s3Event:{JsonConvert.SerializeObject(s3Event)}");
                //Get prefix
                string stepPrefix = GetDataFlowStepPrefix(bucket, key);
                if (stepPrefix != null)
                {
                    try
                    {
                        //Find DataFlow step which should be started based on step trigger prefix
                        List<DataFlowStep> stepList = dsContext.DataFlowStep.Where(w => w.TriggerKey == stepPrefix).ToList();

                        _flow = stepList.Select(s => s.DataFlow).Distinct().Single();

                        //determine DataFlow execution and run instance guids to ensure processing is tied.
                        GetExecutionGuids(key);

                        //establish flow execution guid if null and log data flow level initalization message
                        if (flowExecutionGuid == null)
                        {
                            flowExecutionGuid = GetNewGuid();

                            IsNewFile = true;

                            Logger.AddContextVariable(new TextVariable("flowexecutionguid", flowExecutionGuid));
                        }
                        else
                        {
                            Logger.AddContextVariable(new TextVariable("flowexecutionguid", flowExecutionGuid));
                        }

                        //log dependency steps
                        LogDetectedSteps(key, stepList, flowExecutionGuid, _flow);

                        //save new logs
                        dsContext.SaveChanges();

                        //Generate Start Events
                        foreach (DataFlowStep step in stepList)
                        {
                            //Generate new runinstance quid if in rerun scenario
                            if (step.Executions.Where(w => w.FlowExecutionGuid == flowExecutionGuid).Any() && runInstanceGuid == null)
                            {
                                runInstanceGuid = GetNewGuid();
                            }

                            if (runInstanceGuid != null)
                            {
                                Logger.AddContextVariable(new TextVariable("runinstanceguid", runInstanceGuid));
                            }

                            if (IsNewFile)
                            {
                                MetricData.Add("status", "C");
                                MetricData.Add("log", $"Initialize flow execution bucket:{bucket}, key:{key}, file:{Path.GetFileName(key)}");
                                step.Executions.Add(step.LogExecution(flowExecutionGuid, runInstanceGuid, MetricData, Log_Level.Info));
                                //step.Executions.Add(step.LogExecution(flowExecutionGuid, $"Initialize flow execution bucket:{bucket}, key:{key}, file:{Path.GetFileName(key)}", Log_Level.Info));
                            }

                            _stepService.PublishStartEvent(step, flowExecutionGuid, runInstanceGuid, s3Event);
                        }

                        _flow.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <executedependencies>", Log_Level.Info);

                        //save new logs
                        dsContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logs.Add(_flow.LogExecution(flowExecutionGuid, runInstanceGuid, $"dataflowprovider-ExecuteDependenciesAsync-failed", Log_Level.Error, ex));

                        _flow.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <executedependencies>", Log_Level.Info);

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

        public async Task ExecuteStep(DataFlowStepEvent stepEvent)
        {
            try
            {
                using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                {
                    IDataStepService stepService = container.GetInstance<IDataStepService>();

                    //GetExecutionGuids(stepEvent.SourceKey);
                    Logger.AddContextVariable(new TextVariable("flowexecutionguid", stepEvent.FlowExecutionGuid));

                    S3ObjectEvent s3Event = JsonConvert.DeserializeObject<S3ObjectEvent>(stepEvent.OriginalS3Event);
                    Logger.AddContextVariable(new LongVariable("objectsize", s3Event.s3.Object.size));
                    if (stepEvent.RunInstanceGuid != null)
                    {
                        Logger.AddContextVariable(new TextVariable("runinstanceguid", stepEvent.RunInstanceGuid));
                    }

                    stepService.ExecuteStep(stepEvent);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"dataflowprovider-executestep failed", ex);
            }
            finally
            {
                Logger.RemoveContextVariable("objectsize");
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

            logs.Add(flow.LogExecution(executionGuid, runInstanceGuid, sb.ToString(), Log_Level.Info));
        }

        protected string GetDataFlowStepPrefix(string bucket, string key)
        {
            Logger.Info($"start-method <getdataflowstepprefix>");

            string filePrefix = null;
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.PRODUCER_S3_DROP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.UNCOMPRESS_ZIP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.UNCOMPRESS_GZIP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_MAP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.GOOGLEAPI_PREPROCESSING_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CLAIMIQ_PREPROCESSING_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.FIXEDWIDTH_PREPROCESSING_PREFIX) ||
                ((bucket.EndsWith("-droplocation-ae2") && bucket.StartsWith("sentry-data-")) && key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.DROP_LOCATION_PREFIX)))
            {
                Logger.Debug($"Using Get4thIndex strategy to detect prefix");
                int idx = GetNthIndex(key, '/', 4);
                filePrefix = key.Substring(0, (idx + 1));
            }

            if (filePrefix == null && key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.DROP_LOCATION_PREFIX))
            {
                Logger.Debug($"Using Get3thIndex strategy to detect prefix");
                int idx = GetNthIndex(key, '/', 3);
                filePrefix = key.Substring(0, (idx + 1));
            }

            Logger.Debug($"key:{key} | filePrefix: {filePrefix}");
            Logger.Info($"end-method <getdataflowstepprefix>");

            return filePrefix;            
        }

        private void GetExecutionGuids(string key)
        {
            Logger.Info($"start-method <getflowguid>");

            string guidPrefix = null;

            //five level prefixes - temp locations
            //temp-file/<step prefix>/<env ind>/<data flow id>/<flowGuid>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.PRODUCER_S3_DROP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.UNCOMPRESS_ZIP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.UNCOMPRESS_GZIP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_MAP_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.GOOGLEAPI_PREPROCESSING_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CLAIMIQ_PREPROCESSING_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.FIXEDWIDTH_PREPROCESSING_PREFIX))
            {
                int strtIdx = GetNthIndex(key, '/', 4);
                int endIdx = GetNthIndex(key, '/', 5);
                guidPrefix = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            //six level prefixes - temp locations
            //temp-file/<step prefix>/<env ind>/<data flow id>/<storagecode>/<flowGuid>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX))
            {
                int strtIdx = GetNthIndex(key, '/', 5);
                int endIdx = GetNthIndex(key, '/', 6);
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

        private string GetNewGuid()
        {
            return DateTime.UtcNow.ToString(GlobalConstants.DataFlowGuidConfiguration.GUID_FORMAT);
        }
        #endregion
    }
}