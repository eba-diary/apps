using System.ComponentModel;

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
