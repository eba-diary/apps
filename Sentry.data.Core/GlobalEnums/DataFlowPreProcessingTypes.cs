﻿using System.ComponentModel;

namespace Sentry.data.Core
{
    /// <summary>
    /// Adjustements to this Enum need to be reflected within 
    ///  Sentry.data.Database\Scripts\Post-Deploy\StaticData\DataFlowPreProcessingTypes.sql
    /// </summary>
    public enum DataFlowPreProcessingTypes
    {        
        [Description("Google API")]
        googleapi = 1,
        [Description("Claim IQ")]
        claimiq = 2
    }
}
