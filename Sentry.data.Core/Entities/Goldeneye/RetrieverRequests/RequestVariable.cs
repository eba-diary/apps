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

        public bool TryIncrementVariableValue()
        {
            if (VariableIncrementType == RequestVariableIncrementType.Daily)
            {
                DateTime previousDate = DateTime.ParseExact(VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime nextDate = previousDate.AddDays(1);

                if (nextDate <= DateTime.Today)
                {
                    VariableValue = nextDate.ToString("yyyy-MM-dd");
                    return true;
                }
            }

            return false;
        }
    }
}
