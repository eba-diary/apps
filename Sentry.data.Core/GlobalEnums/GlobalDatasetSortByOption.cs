using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum GlobalDatasetSortByOption
    {
        [Description("Favorites")]
        Favorites,
        [Description("Alphabetical")]
        Alphabetical,
        [Description("Relevance")]
        Relevance
    }
}
