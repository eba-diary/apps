using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Sentry.data.Core
{
    public class RetrieverJob : IRetrieverJob
    {
        private string _jobOptions;

        public RetrieverJob()
        {

        }

        public virtual int Id { get; set; }
        public virtual string Schedule { get; set; }
        public virtual string TimeZone { get; set; }

        public virtual string ReadableSchedule {
            get {

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
            string outFileName = null;
            // Are we overwritting target file
            if (JobOptions.OverwriteDataFile)
            {
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
                    outFileName = JobOptions.TargetFileName;
                }
                // Regex and TargetFileName is null - Use input file name
                else if (JobOptions.IsRegexSearch && String.IsNullOrWhiteSpace(JobOptions.TargetFileName))
                {
                    outFileName = incomingFileName;
                }
            }
            else
            {
                JobLoggerMessage("Error", "OverwriteDataFile option is set to false. Change setting to true for job to work");
                throw new NotImplementedException("OverwriteDataFile setting of false is not implemented.  Change setting to true.");
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

            //if Job options already is filtering file, no need to check if file extension is correct
            if (!filterfile)
            {
                filterfile = (DatasetConfig.FileExtension.Name.ToLower().Trim() != "any" && DatasetConfig.FileExtension.Name.ToLower().Trim() != Path.GetExtension(fileName).Replace(".",""));
                if (filterfile) { this.JobLoggerMessage("Info", "Incoming file was filtered (file extension)"); }
            }                     

            return filterfile;
        }

        public virtual void JobLoggerMessage(string severity, string message, Exception ex = null)
        {
            switch (severity)
            {
                case "Debug":
                    Sentry.Common.Logging.Logger.Debug($"{message} - Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}");
                    break;
                case "Info":
                    Sentry.Common.Logging.Logger.Info($"{message} - Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}");
                    break;
                case "Warn":
                    if(ex == null){ Sentry.Common.Logging.Logger.Warn($"{message} - Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}"); }
                    else{ Sentry.Common.Logging.Logger.Warn($"{message} - Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}",ex); }
                    break;
                case "Error":
                    if (ex == null){ Sentry.Common.Logging.Logger.Error($"{message} - Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}"); }
                    else{ Sentry.Common.Logging.Logger.Error($"{message} - Job:{this.Id} | DataSource:{this.DataSource.Name} | DataSourceID:{this.DataSource.Id} | Schema:{this.DatasetConfig.Name} | SchemaID:{this.DatasetConfig.ConfigId} | Dataset:{this.DatasetConfig.ParentDataset.DatasetName} | DatasetID:{this.DatasetConfig.ParentDataset.DatasetId}", ex); }
                    break;
                default:
                    break;
            }
        }
    }
}
