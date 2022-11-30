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
        public ValidationResults Validate()
        {
            ValidationResults results = new ValidationResults();

            if (string.IsNullOrWhiteSpace(VariableName) && !Regex.IsMatch(VariableName, "^[A-Za-z0-9_]*$"))
            {
                results.Add($"RequestVariable[{Index}].VariableName", "Variable Name is required");
            }

            if (string.IsNullOrWhiteSpace(VariableValue))
            {
                results.Add($"RequestVariable[{Index}].VariableValue", "Variable Value is required");
            }
            else if (VariableIncrementType == RequestVariableIncrementType.Daily && !DateTime.TryParseExact(VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                results.Add($"RequestVariable[{Index}].VariableValue", $"Variable Value must be in yyyy-MM-dd format to use with '{VariableIncrementType.GetDescription()}'");
            }

            if (VariableIncrementType == RequestVariableIncrementType.None)
            {
                results.Add($"RequestVariable[{Index}].VariableIncrementType", "Variable Increment Type is required");
            }

            return results;
        }
        #endregion
    }
}