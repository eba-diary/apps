﻿using System;
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
                //return (String.IsNullOrEmpty(_jobOptions)) ? null : JsonConvert.DeserializeObject<RetrieverJobOptions>(_jobOptions);
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
            //if (JobOptions.TargetFileName != null)
            //{
            //    return JobOptions.TargetFileName;
            //}
            //else return incomingFileName;

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
                else if (!(JobOptions.IsRegexSearch) && !(String.IsNullOrWhiteSpace(JobOptions.TargetFileName)))
                {
                    outFileName = JobOptions.TargetFileName;
                }
                // Regex and TargetFileName has value
                // Use TargetFileName value
                else if (JobOptions.IsRegexSearch && !(String.IsNullOrWhiteSpace(JobOptions.TargetFileName)))
                {
                    outFileName = JobOptions.TargetFileName;
                }
                // Regex and TargetFileName is null - Use input file name
                else if (JobOptions.IsRegexSearch && String.IsNullOrWhiteSpace(JobOptions.TargetFileName))
                {
                    outFileName = incomingFileName;
                }
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
            }

            //if Job options already is filtering file, no need to check if file extension is correct
            if (!filterfile)
            {
                filterfile = (DatasetConfig.FileExtension.Name.ToLower().Trim() != "any" && DatasetConfig.FileExtension.Name.ToLower().Trim() != Path.GetExtension(fileName).Replace(".",""));
            }                     

            return filterfile;
        }
    }
}
