using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class CreateJobModel
    {
        public CreateJobModel()
        {

        }

        public CreateJobModel(int configID, int datasetID)
        {
            this.Schedule = "";
            this.RelativeUri = "";
            this.CompressionType = "";
            this.IsSourceCompressed = false;
            this.DatasetConfigID = configID;
            this.DatasetID = datasetID;

            this.IsRegexSearch = true;
            this.SearchCriteria = "\\.";
            this.OverwriteDataFile = false;
            this.CreateCurrentFile = false;
            this.TargetFileName = "";

        }

        public string Schedule { get; set; }

        [Required]
        [DisplayName("Schedule")]
        public int SchedulePicker { get; set; }

        [Required]
        [DisplayName("Relative URI")]
        public string RelativeUri { get; set; }

        [DisplayName("Https Body")]
        public string HttpRequestBody { get; set; }

        [DisplayName("Search Criteria")]
        public string SearchCriteria { get; set; }

        [DisplayName("Is Search Regex?")]
        public Boolean IsRegexSearch { get; set; }

        [DisplayName("Overwrite Data File")]
        public Boolean OverwriteDataFile { get; set; }

        [DisplayName("Target File Name")]
        public string TargetFileName { get; set; }

        [DisplayName("Create Current File")]
        public Boolean CreateCurrentFile { get; set; }

        public int DatasetID { get; set; }
        public int DatasetConfigID { get; set; }

        [DisplayName("Decompress File?")]
        public Boolean IsSourceCompressed { get; set; }
        [DisplayName("Compression Type")]
        public string CompressionType { get; set; }
        public IEnumerable<SelectListItem> CompressionTypesDropdown { get; set; }
        public string NewFileNameExclusionList { get; set; }


        //This is for post backs that fail.
        public List<string> FileNameExclusionList { get; set; }


        [DisplayName("Data Source")]
        public int SelectedDataSource { get; set; }

        [DisplayName("Source Type")]
        public string SelectedSourceType { get; set; }
        [DisplayName("Request Method")]
        public HttpMethods SelectedRequestMethod { get; set; }
        [DisplayName("Request Body Format")]
        public HttpDataFormat SelectedRequestDataFormat { get; set; }
        [DisplayName("FTP Pattern")]
        public FtpPattern FtpPattern { get; set; }
        public List<DataSource> AvailableSources { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> SourcesForDropdown { get; set; }
        public IEnumerable<SelectListItem> RequestMethodDropdown { get; set; }
        public IEnumerable<SelectListItem> RequestDataFormatDropdown { get; set; }
        public IEnumerable<SelectListItem> FtpPatternDropDown { get; internal set; }
        public IEnumerable<SelectListItem> SchedulePickerDropdown { get; set; }
        public List<string> SourceIds { get; set; }

        public UserSecurity Security { get; set; }

        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (this.SchedulePicker == 0)
            {
                errors.Add("Need to pick a Schedule");
            }
            if (this.SchedulePicker > 0 && String.IsNullOrEmpty(this.Schedule))
            {
                errors.Add("Need to specify when to execute job");
            }
            if (!String.IsNullOrEmpty(this.Schedule) && this.SchedulePicker != 0)
            {
                // pulled regex from https://stackoverflow.com/a/17858524/9694826
                var rx = new Regex("^(\\*|([0-9]|1[0-9]|2[0-9]|3[0-9]|4[0-9]|5[0-9])|\\*\\/([0-9]|1[0-9]|2[0-9]|3[0-9]|4[0-9]|5[0-9])) (\\*|([0-9]|1[0-9]|2[0-3])|\\*\\/([0-9]|1[0-9]|2[0-3])) (\\*|([1-9]|1[0-9]|2[0-9]|3[0-1])|\\*\\/([1-9]|1[0-9]|2[0-9]|3[0-1])) (\\*|([1-9]|1[0-2])|\\*\\/([1-9]|1[0-2])) (\\*|([0-6])|\\*\\/([0-6]))$");

                if (!rx.IsMatch(this.Schedule))
                {
                    errors.Add($"Generated schedule is not valid ({Schedule}), specify valid schedule");
                }
            }

            return errors;
        }
    }
}