using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum GlobalDatasetSortByOption
    {
        [Description("Relevance")]
        Relevance,
        [Description("Favorites")]
        Favorites,
        [Description("Alphabetical")]
        Alphabetical
    }
}
