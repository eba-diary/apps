using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Core
{
    public class RetrieverJob : IRetrieverJob, IValidatable
    {
        private string _jobOptions;

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
                if (String.IsNullOrEmpty(_jobOptions))
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
                    if (DataSource.Is<HTTPSSource>())
                    {
                        outFileName = JobOptions.TargetFileName;
                    }
                    else
                    {
                        outFileName = JobOptions.TargetFileName + Path.GetExtension(incomingFileName);
                    }
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
                if (JobOptions.IsRegexSearch)
                {
                    filterfile = !(Regex.IsMatch(fileName, JobOptions.SearchCriteria));
                }
                else
                {
                    filterfile = (fileName != JobOptions.SearchCriteria);
                }

                if (filterfile) { this.JobLoggerMessage("Info","Incoming file was filtered (search criteria)"); }
            }

            ////if Job options already is filtering file, no need to check if file extension is correct
            //if (!filterfile)
            //{
            //    switch (DatasetConfig.FileExtension.Name.ToLower().Trim())
            //    {
            //        case "any":
            //        case "delimited":
            //            //do not perform any extension checking
            //            break;
            //        default:
            //            //Check if incoming extension matches
            //            filterfile = (DatasetConfig.FileExtension.Name.ToLower().Trim() != Path.GetExtension(fileName).Replace(".", ""));
            //            break;
            //    }

            //    if (filterfile) { this.JobLoggerMessage("Info", "Incoming file was filtered (file extension)"); }
            //}                     

            return filterfile;
        }

        public virtual void JobLoggerMessage(string severity, string message, Exception ex = null)
        {
            string jobSpecifics;
            if (this.DataFlow == null)
            {
                jobSpecifics = $"Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}";
            }
            else
            {
                jobSpecifics = $"Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | DataFlowName:{this.DataFlow.Name} | DataFlowID:{this.DataFlow.Id}";                
            }

            switch (severity.ToUpper())
            {
                case "DEBUG":
                    Sentry.Common.Logging.Logger.Debug($"{message} - {jobSpecifics}");
                    break;
                case "INFO":
                    Sentry.Common.Logging.Logger.Info($"{message} - {jobSpecifics}");
                    break;
                case "WARN":
                    if (ex == null) { Sentry.Common.Logging.Logger.Warn($"{message} - {jobSpecifics}"); }
                    else { Sentry.Common.Logging.Logger.Warn($"{message} - {jobSpecifics}", ex); }
                    break;
                case "ERROR":
                    if (ex == null) { Sentry.Common.Logging.Logger.Error($"{message} - {jobSpecifics}"); }
                    else { Sentry.Common.Logging.Logger.Error($"{message} - {jobSpecifics}", ex); }
                    break;
                default:
                    break;
            }
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            //Validations specific for HTTPSSource
            if (DataSource.Is<HTTPSSource>())
            {
                if (String.IsNullOrWhiteSpace(JobOptions.TargetFileName))
                {
                    vr.Add(ValidationErrors.httpsTargetFileNameBlank, "Target file name is required for HTTPS data sources");
                }
            }
            return vr;
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
            //throw new NotImplementedException();
        }
        public class ValidationErrors
        {
            public const string httpsTargetFileNameBlank = "keyIsBlank";
        }

        public virtual string JobName()
        {
            if (this.DataFlow == null)
            {
                return $"RJob~{this.DatasetConfig.ParentDataset.DatasetId}~{this.Id}~{this.DatasetConfig.ConfigId}~{this.DataSource.Name}";
            }
            else
            {
                return $"RJob~df_{this.DataFlow.Id}~job_{this.Id}~dsrc_{this.DataSource.Name}";
            }
        }
    }
}
