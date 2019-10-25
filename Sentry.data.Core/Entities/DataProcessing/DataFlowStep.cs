using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

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
        public virtual IList<SchemaMap> SchemaMappings { get; set; }
        public virtual IList<DataFlow_Log> Executions { get; set; }

        public virtual void ProcessEvent(DataFlowStepEvent stepEvent, string FlowExecutionGuid)
        {
            Action.ExecuteAction(this, stepEvent, FlowExecutionGuid);
        }

        //public virtual void GenerateStartEvent(string bucket, string key, string FlowExecutionGuid)
        //{
        //    Action.PublishStartEvent(this, bucket, key, FlowExecutionGuid);
        //}

        //public IList<ActionExecution> Executions { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"stepid:{Id} actiontype:{DataAction_Type_Id.ToString()} triggerkey:{TriggerKey} targetbucket:{Action.TargetStorageBucket} targetprefix:{TargetPrefix}  FlowGuid:{DataFlow.FlowGuid}");

            return sb.ToString();
        }

        public virtual DataFlow_Log LogExecution(string executionGuid, string runInstanceGuid, string log, Log_Level level, Exception ex = null)
        {
            string logMsg = $"{executionGuid}{((runInstanceGuid != null) ? "-" + runInstanceGuid : String.Empty)} {log}";
            switch (level)
            {
                case Log_Level.Info:
                    Logger.Info(logMsg);
                    break;
                case Log_Level.Warning:
                    Logger.Warn(logMsg);
                    break;
                case Log_Level.Debug:
                    Logger.Debug(logMsg);
                    break;
                default:
                case Log_Level.Error:
                    Logger.Error(logMsg, ex);
                    break;
            }

            return new DataFlow_Log()
            {
                DataFlow = this.DataFlow,
                FlowExecutionGuid = executionGuid,
                RunInstanceGuid = runInstanceGuid,
                Log_Entry = log,
                Machine_Name = System.Environment.MachineName,
                Level = level,
                CreatedDTM = DateTime.Now,
                Step = this
            };
        }
    }
}
