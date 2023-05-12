using Sentry.data.Core.Interfaces.DataProcessing;
using System.Collections.Generic;
using System.Text;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlowStep : IDataStep
    {
        public virtual int Id { get; set; }
        public virtual DataFlow DataFlow { get; set; }
        public virtual DataActionType DataAction_Type_Id { get; set; }
        public virtual BaseAction Action { get; set; }
        public virtual int ExeuctionOrder { get; set; }
        public virtual string TriggerKey { get; set; }
        public virtual string TargetPrefix { get; set; }
        public virtual string SourceDependencyPrefix { get; set; }
        public virtual string SourceDependencyBucket { get; set; }
        public virtual string TriggerBucket { get; set; }
        public virtual string TargetBucket { get; set; }
        public virtual IList<SchemaMap> SchemaMappings { get; set; }
        public virtual IList<EventMetric> Executions { get; set; }

        public virtual void ProcessEvent(DataFlowStepEvent stepEvent, string FlowExecutionGuid)
        {
            Action.ExecuteAction(this, stepEvent, FlowExecutionGuid);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"stepid:{Id} actiontype:{DataAction_Type_Id.ToString()} triggerbucket:{TriggerBucket} triggerkey:{TriggerKey} targetbucket:{TargetBucket} targetprefix:{TargetPrefix}  FlowGuid:{DataFlow.FlowGuid}");

            return sb.ToString();
        }

        //public virtual EventMetric LogExecution(string executionGuid, string runInstanceGuid, string log, Log_Level level, Exception ex = null)
        //{
        //    string logMsg = $"{executionGuid}{((runInstanceGuid != null) ? "-" + runInstanceGuid : String.Empty)} {log}";
        //    LoggingUtils.LogMessage(logMsg, level, null, ex);

        //    return new EventMetric()
        //    {
        //        //DataFlow = this.DataFlow,
        //        FlowExecutionGuid = executionGuid,
        //        RunInstanceGuid = runInstanceGuid,
        //        MachineName = System.Environment.MachineName,
        //        CreatedDTM = DateTime.Now,
        //        Step = this
        //    };
        //}
        //public virtual EventMetric LogExecution(string executionGuid, string runInstanceGuid, string log, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        //{
        //    //string logMsg = $"{executionGuid}{((runInstanceGuid != null) ? "-" + runInstanceGuid : String.Empty)} {log}";
        //    LoggingUtils.LogMessage(log, level, contextVariables, ex);

        //    return new EventMetric()
        //    {
        //        //DataFlow = this.DataFlow,
        //        FlowExecutionGuid = executionGuid,
        //        RunInstanceGuid = runInstanceGuid,
        //        Step = this,
        //        MachineName = System.Environment.MachineName,
        //        CreatedDTM = DateTime.Now,
        //        MetricsData = log,
        //        ApplicationName = "S"
        //    };
        //}
        //public virtual EventMetric LogExecution(string executionGuid, string runInstanceGuid, JObject metricData, string log, Log_Level level, Exception ex = null)
        //{
        //    LoggingUtils.LogMessage(metricData.ToString(), level, null, ex);

        //    EventMetric envMet = new EventMetric()
        //    {
        //        FlowExecutionGuid = executionGuid,
        //        RunInstanceGuid = runInstanceGuid,
        //        Step = this,
        //        MessageValue = null,
        //        ApplicationName = "S",
        //        MachineName = System.Environment.MachineName,
        //        MetricsData = metricData.ToString(),
        //        CreatedDTM = DateTime.Now,
        //        StatusCode = (metricData.ContainsKey("status")) ? metricData.GetValue("status").ToString() : null
        //    };

        //    return envMet;
        //}
        //public virtual EventMetric LogExecution(string executionGuid, string runInstanceGuid, JObject metricData, string log, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        //{
        //    LoggingUtils.LogMessage(metricData.ToString(), level, contextVariables, ex);

        //    EventMetric envMet = new EventMetric()
        //    {
        //        FlowExecutionGuid = executionGuid,
        //        RunInstanceGuid = runInstanceGuid,
        //        Step = this,
        //        MessageValue = null,
        //        ApplicationName = "S",
        //        MachineName = System.Environment.MachineName,
        //        MetricsData = metricData.ToString(),
        //        CreatedDTM = DateTime.Now,
        //        StatusCode = (metricData.ContainsKey("status")) ? metricData.GetValue("status").ToString() : null
        //    };

        //    return envMet;
        //}
        //public virtual EventMetric LogExecution(DataFlowStepEvent eventItem, JObject metricData, Log_Level level, Exception ex = null)
        //{
        //    return LogExecution(eventItem, metricData, level, null, ex);
        //}
        //public virtual EventMetric LogExecution(DataFlowStepEvent eventItem, JObject metricData, Log_Level level, List<Variable> contextVariables, Exception ex = null)
        //{
        //    LoggingUtils.LogMessage(string.Empty, level, contextVariables, ex);

        //    EventMetric envMet = new EventMetric(this, eventItem, metricData);

        //    return envMet;
        //}
    }
}
