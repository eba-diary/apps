using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public static class DataFlowExtensions
    {
        public static List<DataFlowDto> ToDtoList(this List<DataFlow> dfList)
        {
            List<DataFlowDto> dtoList = new List<DataFlowDto>();
            foreach (DataFlow df in dfList)
            {
                dtoList.Add(df.ToDto());
            }
            return dtoList;
        }

        public static DataFlowDto ToDto(this DataFlow df)
        {
            return new DataFlowDto()
            {
                Id = df.Id,
                FlowGuid = df.FlowGuid,
                SaidKeyCode = df.SaidKeyCode,
                Name = df.Name,
                CreateDTM = df.CreatedDTM,
                CreatedBy = df.CreatedBy,
                ObjectStatus = df.ObjectStatus,
                DeleteIssuer = df.DeleteIssuer,
                DeleteIssueDTM = df.DeleteIssueDTM
            };
        }

        public static void MapToRetrieverJobDto(this DataFlow dataFlow, RetrieverJobDto dto)
        {
            dto.DataFlow = dataFlow.Id;
            dto.FileSchema = dataFlow.SchemaId;
        }

        public static SchemaMapDto ToDto(this SchemaMap scmMap)
        {
            return new SchemaMapDto()
            {
                Id = scmMap.Id,
                DatasetId = (scmMap.Dataset != null) ? scmMap.Dataset.DatasetId : 0,
                SchemaId = scmMap.MappedSchema.SchemaId,
                SearchCriteria = scmMap.SearchCriteria,
                StepId = scmMap.DataFlowStepId.Id
            };
        }


        //DETERMINE WHETHER TO CREATE DFSDROP LOCATIONS OR NOT DEPENDING ON DataFlow properties
        public static bool ShouldCreateDFSDropLocations(this DataFlow df, IDataFeatures dataFeatures)
        {
            //LOGIC: CREATE DFSDROPLOCATION IF FEATURE FLAG DISABLED AND EITHER TOPIC/BACKFILLED OR DFSDROP
            if (!dataFeatures.CLA3241_DisableDfsDropLocation.GetValue()
                   &&
                   (
                       (df.IngestionType == (int)GlobalEnums.IngestionType.Topic && df.IsBackFillRequired)
                       ||
                       (df.IngestionType == (int)GlobalEnums.IngestionType.DFS_Drop)
                   )
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetDataFlowStepPrefix(string key)
        {
            string filePrefix = null;
            //three level prefixes
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX))
            {
                return key.Substring(0, key.Length - GetNthIndex(key, '/', 3));
            }
            //two level prefixes
            // example -  <rawstorage prefix>/<job Id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX))
            {
                return key.Substring(0, key.Length - GetNthIndex(key, '/', 2));
            }
            return null;
        }


        public static int GetNthIndex(string s, char t, int n)
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

        public static EventMetric LogExecution(this DataFlowStep step, string executionGuid, string runInstanceGuid, string log, Log_Level level, Exception ex = null)
        {
            return step.LogExecution(executionGuid, runInstanceGuid, log, level, null, ex);
        }
        public static EventMetric LogExecution(this DataFlowStep step, string executionGuid, string runInstanceGuid, string log, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        {
            EventMetric metricData = LoggingUtils.LogExecution(executionGuid, runInstanceGuid, log, level, contextVariables, ex);
            metricData.Step = step;
            return metricData;
        }
        public static EventMetric LogExecution(this DataFlowStep step, string executionGuid, string runInstanceGuid, JObject metricData, Log_Level level, Exception ex = null)
        {
            return step.LogExecution(executionGuid, runInstanceGuid, metricData, level, null, ex);
        }
        public static EventMetric LogExecution(this DataFlowStep step, string executionGuid, string runInstanceGuid, JObject metricData, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        {
            //log specific log message if it exists, otherwise log the metricData object
            string msg = (metricData.ContainsKey("log")) ? metricData.GetValue("log").ToString() : metricData.ToString();
            LoggingUtils.LogMessage(msg, level, contextVariables, ex);
            return step.ToEventMetric(executionGuid, runInstanceGuid, metricData);
        }
        public static EventMetric LogExecution(this DataFlowStep step, DataFlowStepEvent eventItem, JObject metricData, Log_Level level, Exception ex = null)
        {
            return step.LogExecution(eventItem, metricData, level, null, ex);
        }
        public static EventMetric LogExecution(this DataFlowStep step, DataFlowStepEvent eventItem, JObject metricData, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        {
            string logMsg = (metricData.ContainsKey("log")) ? metricData.GetValue("log").ToString() : null;
            LoggingUtils.LogMessage(logMsg, level, contextVariables, ex);
            return step.ToEventMetric(eventItem, metricData);
        }
        public static EventMetric LogExecution(this DataFlow flow, string executionGuid, string runInstanceGuid, string log, Log_Level level, Exception ex = null)
        {
            return LogExecution(flow, executionGuid, runInstanceGuid, log, level, null, ex);
        }
        public static EventMetric LogExecution(this DataFlow flow, string executionGuid, string runInstanceGuid, string log, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        {
            EventMetric metricData = LoggingUtils.LogExecution(executionGuid, runInstanceGuid, log, level, contextVariables, ex);
            return metricData;
        }


        //Convert to EventMetric extensions
        public static EventMetric ToEventMetric(this DataFlowStep step, string executionGuid, string runInstanceGuid, JObject metricData)
        {
            return new EventMetric()
            {
                FlowExecutionGuid = executionGuid,
                RunInstanceGuid = runInstanceGuid,
                Step = step,
                MessageValue = (metricData.ContainsKey("message_value")) ? metricData.GetValue("message_value").ToString() : null,
                MachineName = System.Environment.MachineName,
                //ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                ApplicationName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "").ToUpper(),
                MetricsData = metricData.ToString(),
                CreatedDTM = DateTime.Now,
                StatusCode = (metricData.ContainsKey("status")) ? metricData.GetValue("status").ToString() : null
            };
        }
        public static EventMetric ToEventMetric(this DataFlowStep step, DataFlowStepEvent eventItem, JObject metricData)
        {
            return new EventMetric()
            {
                FlowExecutionGuid = eventItem.FlowExecutionGuid,
                RunInstanceGuid = eventItem.RunInstanceGuid,
                Step = step,
                MessageValue = (metricData.ContainsKey("message_value")) ? metricData.GetValue("message_value").ToString() : null,
                MachineName = System.Environment.MachineName,
                MetricsData = metricData.ToString(),
                CreatedDTM = DateTime.Now,
                //ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                ApplicationName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "").ToUpper(),
                StatusCode = (metricData.ContainsKey("status")) ? metricData.GetValue("status").ToString() : null
            };
        }
    }
}
