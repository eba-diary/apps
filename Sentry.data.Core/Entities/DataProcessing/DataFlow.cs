﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlow
    {
        public DataFlow()
        {
            //Assign new guild value
            Guid g = Guid.NewGuid();
            FlowGuid = g;
        }
        public virtual int Id { get; set; }
        public virtual Guid FlowGuid { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual IList<DataFlowStep> Steps { get; set; }
        public virtual IList<DataFlow_Log> Logs { get; set; }
        public virtual DataFlow_Log LogExecution(string executionGuid, string log, Log_Level level, Exception ex = null)
        {
            switch (level)
            {
                case Log_Level.Info:
                    Logger.Info(log);
                    break;
                case Log_Level.Warning:
                    Logger.Warn(log);
                    break;
                case Log_Level.Debug:
                    Logger.Debug(log);
                    break;
                default:
                case Log_Level.Error:
                    Logger.Error(log, ex);
                    break;
            }

            return new DataFlow_Log()
            {
                DataFlow = this,
                FlowExecutionGuid = executionGuid,
                Log_Entry = log,
                Machine_Name = System.Environment.MachineName,
                Level = level,
                CreatedDTM = DateTime.Now,
                Step = null
            };
        }
    }
}
