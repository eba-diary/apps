using Sentry.data.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class RequestVariableModel
    {
        [DisplayName("Variable Name")]
        public string VariableName { get; set; }

        [DisplayName("Variable Value")]
        public string VariableValue { get; set; }

        [DisplayName("Variable Increment Type")]
        public RequestVariableIncrementType VariableIncrementType { get; set; }
        public List<SelectListItem> VariableIncrementTypeDropdown { get; set; }
    }
}