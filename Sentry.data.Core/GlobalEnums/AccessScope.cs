using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AccessScope
    {
        [Description("Dataset Level Access")]
        Dataset = 0,
        [Description("Asset Level Access")]
        Asset = 1
    }
}
