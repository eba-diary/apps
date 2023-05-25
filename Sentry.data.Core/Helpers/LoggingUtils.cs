using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Helpers
{
    public static class LoggingUtils
    {
        public static void LogMessage(string msg, Log_Level level, Exception ex = null)
        {
            LogMessage(msg, level, null, ex);
        }
        public static void LogMessage(string msg, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        {
            if (contextVariables != null)
            {
                switch (level)
                {
                    case Log_Level.Info:
                        Logger.Info(msg, contextVariables.ToArray());
                        break;
                    case Log_Level.Warning:
                        Logger.Warn(msg, contextVariables.ToArray());
                        break;
                    case Log_Level.Debug:
                        Logger.Debug(msg, contextVariables.ToArray());
                        break;
                    default:
                    case Log_Level.Error:
                        Logger.Error(msg, ex, contextVariables.ToArray());
                        break;
                }
            }
            else
            {
                switch (level)
                {
                    case Log_Level.Info:
                        Logger.Info(msg);
                        break;
                    case Log_Level.Warning:
                        Logger.Warn(msg);
                        break;
                    case Log_Level.Debug:
                        Logger.Debug(msg);
                        break;
                    default:
                    case Log_Level.Error:
                        if (ex == null)
                        {
                            Logger.Error(msg);
                        }
                        else
                        {
                            Logger.Error(msg, ex);
                        }
                        break;
                }
            }
        }

        public static EventMetric LogExecution(string executionGuid, string runInstanceGuid, string log, Log_Level level, Exception ex = null)
        {
            return LogExecution(executionGuid, runInstanceGuid, log, level, null, ex);
        }
        public static EventMetric LogExecution(string executionGuid, string runInstanceGuid, string log, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        {
            string logMsg = $"{executionGuid}{((runInstanceGuid != null) ? "-" + runInstanceGuid : String.Empty)} {log}";
            LoggingUtils.LogMessage(logMsg, level, contextVariables, ex);

            return new EventMetric()
            {
                FlowExecutionGuid = executionGuid,
                RunInstanceGuid = runInstanceGuid,
                MachineName = System.Environment.MachineName,
                CreatedDTM = DateTime.Now,
                ApplicationName = "S",
                MetricsData = log
            };
        }

        public static void AddOrUpdateValue(this JObject obj, string key, string value)
        {
            if (obj.ContainsKey(key))
            {
                obj[key] = value;
            }
            else
            {
                obj.Add(key, value);
            }
        }
    }
}
