using Newtonsoft.Json;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Sentry.data.Core
{
    public class RetrieverJob : IRetrieverJob, IValidatable
    {
        private string _jobOptions;
        private string _executionParameters;
        private string _requestVariables;

        public RetrieverJob()
        {
            IsEnabled = true;
            JobHistory = new List<JobHistory>();
        }

        public virtual int Id { get; set; }
        public virtual Guid JobGuid { get; set; }
        public virtual string Schedule { get; set; }
        public virtual string TimeZone { get; set; }

        public virtual string ReadableSchedule {
            get {

                if(Schedule == null)
                {
                    return "";
                }

                var cronParts = Schedule.Split(' ');

                if(cronParts.Length == 1)
                {
                    return cronParts[0];
                }

                //* * * 9 *  ~ September
                if (cronParts[3] != "*")
                {
                    return "Yearly";
                }
                //* * 15 * * ~ 15th of every month
                else if (cronParts[2] != "*")
                {
                    return "Monthly";
                }
                //* * * * 1 every Monday
                else if (cronParts[4] != "*")
                {
                    return "Weekly";
                }
                //* 12 * * * noon every day
                else if (cronParts[1] != "*")
                {
                    return "Daily";
                }
                //30 * * * * bottom of the hour every hour
                else if (cronParts[0] != "*")
                {
                    return "Hourly";
                }
                //
                else if(cronParts[0].StartsWith("*/"))
                {
                    return "Every " + cronParts[0].Substring(2) + " Minutes";
                }
                else
                {
                    return "Instant";
                }
            }
        }

        public virtual string RelativeUri { get; set; }
        public virtual DataSource DataSource { get; set; }
        public virtual DatasetFileConfig DatasetConfig { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual Boolean IsGeneric { get; set; }
        public virtual Boolean IsEnabled { get; set; }

        //Property is stored within database as string
        public virtual RetrieverJobOptions JobOptions
        {
            get
            {
                if (string.IsNullOrEmpty(_jobOptions))
                {
                    return null;
                }
                else
                {
                    RetrieverJobOptions a = JsonConvert.DeserializeObject<RetrieverJobOptions>(_jobOptions);
                    return a;
                }
            }
            set
            {
                _jobOptions = JsonConvert.SerializeObject(value);
            }
        }
        public virtual IList<JobHistory> JobHistory { get; set; }
        public virtual IList<Submission> Submissions { get; set; }
        public virtual FileSchema FileSchema { get; set; }
        public virtual DataFlow DataFlow { get; set; }
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }
        public virtual Dictionary<string, string> ExecutionParameters
        {
            get
            {
                return string.IsNullOrEmpty(_executionParameters) ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(_executionParameters);
            }
            set
            {
                _executionParameters = JsonConvert.SerializeObject(value);
            }
        }
        public virtual List<RequestVariable> RequestVariables
        {
            get
            {
                return string.IsNullOrEmpty(_requestVariables) ? new List<RequestVariable>() : JsonConvert.DeserializeObject<List<RequestVariable>>(_requestVariables);

            }
            set
            {
                _requestVariables = JsonConvert.SerializeObject(value);
            }
        }

        public virtual void AddOrUpdateExecutionParameter(string key, string value)
        {
            Dictionary<string, string> parameters = ExecutionParameters;
            parameters[key] = value;
            ExecutionParameters = parameters;
        }

        public virtual Uri GetUri()
        {
            return DataSource.CalcRelativeUri(this);
        }

        /// <summary>
        /// Return file name which incoming file should be renamed too
        /// </summary>
        /// <param name="incomingFileName">Incoming file name including extension</param>
        /// <returns></returns>
        public virtual string GetTargetFileName(string incomingFileName)
        {
            //Current functionality does not allow renaming of files extracted
            // from a ZIP file, therefore, pass the incomingFileName back.
            if (JobOptions.CompressionOptions.IsCompressed && Convert.ToInt32(JobOptions.CompressionOptions.CompressionType) == (int)CompressionTypes.ZIP)
            {
                return incomingFileName;
            }

            string outFileName = null;
            // Are we overwritting target file
                // Non-Regex and TargetFileName is null
                // Use SearchCriteria value
                if (!(JobOptions.IsRegexSearch) && String.IsNullOrWhiteSpace(JobOptions.TargetFileName))
                {
                    outFileName = JobOptions.SearchCriteria;
                }
                // Non-Regex and TargetFileName has value
                // Use TargetFileName value
                // or
                // Regex and TargetFileName has value
                // Use TargetFileName value
                else if ((!(JobOptions.IsRegexSearch) && !(String.IsNullOrWhiteSpace(JobOptions.TargetFileName))) ||
                        JobOptions.IsRegexSearch && !(String.IsNullOrWhiteSpace(JobOptions.TargetFileName)))
                {
                    outFileName = DataSource.Is<HTTPSSource>() ? JobOptions.TargetFileName : JobOptions.TargetFileName + Path.GetExtension(incomingFileName);
            }
                // Regex and TargetFileName is null - Use input file name
                else if (JobOptions.IsRegexSearch && String.IsNullOrWhiteSpace(JobOptions.TargetFileName))
                {
                    outFileName = incomingFileName;
                }

            return outFileName;
        }

        /// <summary>
        /// Return true if filename does not match job file filter criteria
        /// </summary>
        /// <param name="fileName">File name including extension</param>
        /// <returns></returns>
        public virtual Boolean FilterIncomingFile(string fileName)
        {
            var filterfile = false;
            //if there are no options, then no filtering can take place
            if (!String.IsNullOrEmpty(_jobOptions))
            {
                filterfile = JobOptions.IsRegexSearch ? !(Regex.IsMatch(fileName, JobOptions.SearchCriteria)) : fileName != JobOptions.SearchCriteria;

                if (filterfile) 
                { this.JobLoggerMessage("Info","Incoming file was filtered (search criteria)"); }
            }                  

            return filterfile;
        }

        public virtual void JobLoggerMessage(string severity, string message, Exception ex = null)
        {
            string jobSpecifics = this.DataFlow == null
                ? $"Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}"
                : $"Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | DataFlowName:{this.DataFlow.Name} | DataFlowID:{this.DataFlow.Id}";

            switch (severity.ToUpper())
            {
                case "DEBUG":
                    Sentry.Common.Logging.Logger.Debug($"{message} - {jobSpecifics}");
                    break;
                case "INFO":
                    Sentry.Common.Logging.Logger.Info($"{message} - {jobSpecifics}");
                    break;
                case "WARN":
                    if (ex == null) 
                    { Sentry.Common.Logging.Logger.Warn($"{message} - {jobSpecifics}"); }
                    else 
                    { Sentry.Common.Logging.Logger.Warn($"{message} - {jobSpecifics}", ex); }
                    break;
                case "ERROR":
                    if (ex == null) 
                    { Sentry.Common.Logging.Logger.Error($"{message} - {jobSpecifics}"); }
                    else 
                    { Sentry.Common.Logging.Logger.Error($"{message} - {jobSpecifics}", ex); }
                    break;
                default:
                    break;
            }
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            //Validations specific for HTTPSSource
            if ( /* Need to take into consideration pre-v3 dataflows may be associated with DatasetConfig and when saved for delete 
                 *   this validaiton will kick off.  Therefore, the the || is needed. */
                ((DataFlow != null && DataFlow.IngestionType == (int)IngestionType.DSC_Pull) || DatasetConfig != null))
            {
                DataSource.Validate(this, vr);
            }
            if (String.IsNullOrWhiteSpace(Schedule))
            {
                vr.Add(ValidationErrors.scheduleIsNull, "Schedule is required");
            }

            return vr;
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public static class ValidationErrors
        {
            public const string scheduleIsNull = "scheduleIsNull";
        }

        public virtual string JobName()
        {
            return this.DataFlow == null
                ? $"RJob~{this.DatasetConfig.ParentDataset.DatasetId}~{this.Id}~{this.DatasetConfig.ConfigId}~{this.DataSource.Name}"
                : $"RJob~df_{this.DataFlow.FlowStorageCode}~job_{this.Id}~dsrc_{this.DataSource.Name}";
        }
    }
}
