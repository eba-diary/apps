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
            if (PagingType != PagingType.None && string.IsNullOrWhiteSpace(PageParameterName))
            {
                validationResults.Add("PageParameterName", "Page Parameter Name is required");
            }

            return validationResults;
        }

        private ValidationResults ValidateRequestVariables()
        {
            ValidationResults validationResults = ValidateRelativeUriVariables();

            foreach (RequestVariableModel requestVariable in RequestVariables)
            {
                validationResults.MergeInResults(requestVariable.Validate());

                if (!string.IsNullOrWhiteSpace(requestVariable.VariableName))
                {
                    //variable is not used in the relative uri
                    string variablePlaceholder = string.Format(Indicators.REQUESTVARIABLEINDICATOR, requestVariable.VariableName);
                    if (!string.IsNullOrEmpty(RelativeUri) && !RelativeUri.Contains(variablePlaceholder))
                    {
                        validationResults.Add($"RetrieverJob.RequestVariables[{requestVariable.Index}].VariableName", $"{variablePlaceholder} is not used in Relative URI");
                    }
                }
            }

            return validationResults;
        }

        private ValidationResults ValidateRelativeUriVariables()
        {
            ValidationResults validationResults = new ValidationResults();

            if (!string.IsNullOrEmpty(RelativeUri))
            {
                //check if relative uri contains any variables
                string escapedIndicator = Indicators.REQUESTVARIABLEINDICATOR.Replace("[", @"\[").Replace("]", @"\]");
                string variablePlaceholder = string.Format(escapedIndicator, "[A-Za-z0-9]+");
                MatchCollection matches = Regex.Matches(RelativeUri, variablePlaceholder);
                List<string> undefinedVariables = new List<string>();

                foreach (Match match in matches)
                {
                    //relative uri variable does not have a matching request variable
                    if (!undefinedVariables.Contains(match.Value) && !RequestVariables.Any(x => string.Format(Indicators.REQUESTVARIABLEINDICATOR, x.VariableName) == match.Value))
                    {
                        undefinedVariables.Add(match.Value);
                    }
                }

                if (undefinedVariables.Any())
                {
                    validationResults.Add("RetrieverJob.RelativeUri", $"Request Variable(s) not defined for {string.Join(", ", undefinedVariables)}");
                }
            }

            return validationResults;
        }
        #endregion
    }
}