﻿using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AccessRequestType
    {
        Default = 0,
        [Description("AWS ARN Consume")]
        AwsARN = 1,
        [Description("Access for Data Producers")]
        Producer = 2,
        [Description("Dataset Permission Inheritance from SAID Asset")]
        Inheritance = 3
    }
}
