using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
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
        public virtual int IngestionType { get; set; }
        public virtual string FlowStorageCode { get; set; }
        public virtual string SaidKeyCode { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string Questionnaire { get; set; }
        public virtual IList<DataFlowStep> Steps { get; set; }
        public virtual IList<EventMetric> Logs { get; set; }

        //Delete implementation
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }

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

        private void LogMessage(string msg, Log_Level level,  Exception ex = null)
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
}
