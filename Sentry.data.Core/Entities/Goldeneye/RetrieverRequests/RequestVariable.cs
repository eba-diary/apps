using System;
using System.Globalization;

namespace Sentry.data.Core
{
    public class RequestVariable
    {
        public string VariableName { get; set; }
        public string VariableValue { get; set; }
        public RequestVariableIncrementType VariableIncrementType { get; set; }

        public void IncrementVariableValue()
        {
            if (VariableIncrementType == RequestVariableIncrementType.DailyExcludeToday)
            {
                DateTime previousDate = DateTime.ParseExact(VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                VariableValue = previousDate.AddDays(1).ToString("yyyy-MM-dd");
            }
        }

        public bool IsValidVariableValue()
        {
            if (VariableIncrementType == RequestVariableIncrementType.DailyExcludeToday)
            {
                DateTime currentValue = DateTime.ParseExact(VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                return currentValue < DateTime.Today;
            }

            return false;
        }

        public bool EqualTo(RequestVariable other)
        {
            return other.VariableValue == VariableValue &&
                other.VariableName == VariableName &&
                other.VariableIncrementType == VariableIncrementType;
        }
    }
}
