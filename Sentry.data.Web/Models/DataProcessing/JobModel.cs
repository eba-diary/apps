using Sentry.Core;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

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

            results.MergeInResults(ValidatePaging());
            results.MergeInResults(ValidateRequestVariables());

            return new ValidationException(results);
        }

        #region Private
        private ValidationResults ValidatePaging()
        {
            ValidationResults validationResults = new ValidationResults();

            //parameter name required when paging type of page number
            if (PagingType == PagingType.PageNumber && string.IsNullOrWhiteSpace(PageParameterName))
            {
                validationResults.Add("PageParameterName", "Page Parameter Name is required");
            }

            if (PagingType == PagingType.Token)
            {
                //parameter name required when paging type of token
                if (string.IsNullOrWhiteSpace(PageParameterName))
                {
                    validationResults.Add("PageParameterName", "Page Parameter Name is required");
                }

                //token field required when paging type of token
                if (string.IsNullOrWhiteSpace(PageTokenField))
                {
                    validationResults.Add("PageTokenField", "Page Token Field is required");
                }
            }

            return validationResults;
        }

        private ValidationResults ValidateRequestVariables()
        {
            ValidationResults validationResults = new ValidationResults();

            //check if relative uri contains any variables
            MatchCollection matches = Regex.Matches(RelativeUri, string.Format(Indicators.REQUESTVARIABLEINDICATOR, "[A-Za-z0-9]+"));
            List<string> checkedMatches = new List<string>();

            foreach (Match match in matches)
            { 
                //match is not already checked 
                if (!checkedMatches.Contains(match.Value))
                {
                    //relative uri variable does not have a matching request variable
                    if (!RequestVariables.Any(x => string.Format(Indicators.REQUESTVARIABLEINDICATOR, x.VariableName) == match.Value))
                    {
                        validationResults.Add("RetrieverJob.RelativeUri", $"Request Variable for {match.Value} is not defined");
                    }

                    checkedMatches.Add(match.Value);
                }
            }

            foreach (RequestVariableModel requestVariable in RequestVariables)
            {
                validationResults.MergeInResults(requestVariable.Validate());

                //variable is not used in the relative uri
                string variablePlaceholder = string.Format(Indicators.REQUESTVARIABLEINDICATOR, requestVariable.VariableName);
                if (!string.IsNullOrWhiteSpace(requestVariable.VariableName) && !RelativeUri.Contains(variablePlaceholder))
                {
                    validationResults.Add($"RequestVariable[{requestVariable.Index}].VariableName", $"{variablePlaceholder} is not found in Relative URI");
                }
            }

            return validationResults;
        }
        #endregion
    }
}