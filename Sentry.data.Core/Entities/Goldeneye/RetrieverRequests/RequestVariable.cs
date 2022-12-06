using Sentry.Common.Logging;
using System.Globalization;
using System;

namespace Sentry.data.Core
{
    public class RequestVariable
    {
        public string VariableName { get; set; }
        public string VariableValue { get; set; }
        public RequestVariableIncrementType VariableIncrementType { get; set; }

        public void IncrementVariableValue()
        {
            if (VariableIncrementType == RequestVariableIncrementType.Daily)
            {
                DateTime previousDate = DateTime.ParseExact(VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime nextDate = previousDate.AddDays(1);
                VariableValue = nextDate.ToString("yyyy-MM-dd");
            }
        }
    }
}
