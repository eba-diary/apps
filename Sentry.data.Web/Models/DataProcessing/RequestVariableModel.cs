using Sentry.Core;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class RequestVariableModel
    {
        #region Properties
        public string Index { get; set; }

        [DisplayName("Variable Name")]
        public string VariableName { get; set; }

        [DisplayName("Variable Value")]
        public string VariableValue { get; set; }

        [DisplayName("Variable Increment Type")]
        public RequestVariableIncrementType VariableIncrementType { get; set; }
        public List<SelectListItem> VariableIncrementTypeDropdown { get; set; }
        #endregion

        #region Methods
        internal ValidationResults Validate()
        {
            ValidationResults results = new ValidationResults();

            string idPrefix = $"RetrieverJob.RequestVariable[{Index}].";

            //variable name is required
            if (string.IsNullOrWhiteSpace(VariableName))
            {
                results.Add($"{idPrefix}VariableName", "Variable Name is required");
            }
            else if (!Regex.IsMatch(VariableName, "^[A-Za-z0-9]*$"))
            {
                //variable name can only be alphanumeric with _
                results.Add($"{idPrefix}VariableName", "Variable Name must be alphanumeric");
            }

            //variable value is required
            if (string.IsNullOrWhiteSpace(VariableValue))
            {
                results.Add($"{idPrefix}VariableValue", "Variable Value is required");
            }
            else if (VariableIncrementType == RequestVariableIncrementType.Daily && !DateTime.TryParseExact(VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                //variable value must be in acceptable datetime format
                results.Add($"{idPrefix}VariableValue", $"Variable Value must be in yyyy-MM-dd format to use with '{VariableIncrementType.GetDescription()}'");
            }

            //increment type is required
            if (VariableIncrementType == RequestVariableIncrementType.None)
            {
                results.Add($"{idPrefix}VariableIncrementType", "Variable Increment Type is required");
            }

            return results;
        }
        #endregion
    }
}