using Azure.Storage.Sas;
using Sentry.Core;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class JobModel
    {
        public JobModel()
        {
            IsRegexSearch = true;
            FtpPattern = FtpPattern.None;
        }

        public string Schedule { get; set; }

        [DisplayName("Schedule")]
        public string SchedulePicker { get; set; }

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

        [DisplayName("Data Source")]
        public string SelectedDataSource { get; set; }

        [DisplayName("Source Type")]
        public string SelectedSourceType { get; set; }

        [DisplayName("Request Method")]
        public HttpMethods SelectedRequestMethod { get; set; }

        [DisplayName("Request Body Format")]
        public HttpDataFormat SelectedRequestDataFormat { get; set; }

        [DisplayName("FTP Pattern")]
        public FtpPattern FtpPattern { get; set; }

        [DisplayName("Page Token Field")]
        public string PageTokenField { get; set; }
        [DisplayName("Page Parameter Name")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[a-zA-Z0-9_\-\~\.]*$", ErrorMessage = "GET parameter names can only be alphanumeric with -._~")]
        public string PageParameterName { get; set; }
        [DisplayName("Paging Type")]
        public PagingType PagingType { get; set; }
        public List<RequestVariableModel> RequestVariables { get; set; } = new List<RequestVariableModel>();
        public List<DataSource> AvailableSources { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> SourcesForDropdown { get; set; }
        public IEnumerable<SelectListItem> RequestMethodDropdown { get; set; }
        public IEnumerable<SelectListItem> RequestDataFormatDropdown { get; set; }
        public IEnumerable<SelectListItem> FtpPatternDropDown { get; internal set; }
        public IEnumerable<SelectListItem> SchedulePickerDropdown { get; set; }
        public IEnumerable<SelectListItem> PagingTypeDropdown { get; set; }
        public List<string> SourceIds { get; set; }
        public UserSecurity Security { get; set; }
        public Dictionary<string, string> ExecutionParameters { get; set; }

        internal ValidationException Validate()
        {
            ValidationResults results = new ValidationResults();
            if (SelectedSourceType == null)
            {
                results.Add("SelectedSourceType", "Source type is required");
            }
            if (string.IsNullOrWhiteSpace(SelectedDataSource) || SelectedDataSource == "0")
            {
                results.Add("SelectedDataSource", "Data source is required");
            }
            if (string.IsNullOrWhiteSpace(SchedulePicker) || SchedulePicker == "0" || string.IsNullOrWhiteSpace(Schedule))
            {
                results.Add("SchedulePicker", "Schedule is required");
            }

            if (PagingType == PagingType.PageNumber && string.IsNullOrWhiteSpace(PageParameterName))
            {
                results.Add("PageParameterName", "Page Parameter Name is required");
            }

            if (PagingType == PagingType.Token)
            {
                if (string.IsNullOrWhiteSpace(PageParameterName))
                {
                    results.Add("PageParameterName", "Page Parameter Name is required");
                }

                if (string.IsNullOrWhiteSpace(PageTokenField))
                {
                    results.Add("PageTokenField", "Page Token Field is required");
                }
            }

            return new ValidationException(results);
        }
       
    }
}