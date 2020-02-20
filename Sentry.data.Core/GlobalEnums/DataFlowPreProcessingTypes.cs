using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public enum DataFlowPreProcessingTypes
    {
        [Description("Google API")]
        googleapi = 1,
        [Description("Claim IQ")]
        claimiq = 2
    }
}
