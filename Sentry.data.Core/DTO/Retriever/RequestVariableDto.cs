﻿namespace Sentry.data.Core
{
    public class RequestVariableDto
    {
        public string VariableName { get; set; }
        public string VariableValue { get; set; }
        public RequestVariableIncrementType VariableIncrementType { get; set; }
    }
}