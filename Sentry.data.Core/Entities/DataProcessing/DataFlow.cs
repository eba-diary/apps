using Sentry.Common.Logging;
using Sentry.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class DataFlow : IValidatable
    {
        public DataFlow()
        {
            //Assign new guild value
            Guid g = Guid.NewGuid();
            FlowGuid = g;
        }
        public virtual int Id { get; set; }
        public virtual Guid FlowGuid { get; set; }
        public virtual string FlowStorageCode { get; set; }
        public virtual string SaidKeyCode { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string Questionnaire { get; set; }
        public virtual IList<DataFlowStep> Steps { get; set; }
        public virtual IList<DataFlow_Log> Logs { get; set; }
        public virtual DataFlow_Log LogExecution(string executionGuid, string log, Log_Level level, Exception ex = null)
        {
            string logMsg = $"{executionGuid} {log}";
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
                DataFlow = this,
                FlowExecutionGuid = executionGuid,
                Log_Entry = log,
                Machine_Name = System.Environment.MachineName,
                Level = level,
                CreatedDTM = DateTime.Now,
                Step = null
            };
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();
            if (string.IsNullOrWhiteSpace(Name))
            {
                vr.Add(ValidationErrors.nameIsBlank, "The Name of the data flow is required");
            }
            return vr;
        }

        public class ValidationErrors
        {
            public const string nameIsBlank = "nameIsBlank";
            public const string nameContainsReservedWords = "nameContainsReservedWords";
            public const string nameMustBeUnique = "nameMustBeUnique";
            public const string stepsContainsAtLeastOneSchemaMap = "stepsContainsAtLeastOneSchemaMap";
            public const string saidAssetIsBlank = "saidAssetIsBlank";
        }
    }
}
