using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum DatasetSortByOption
    {
        [Description("Alphabetical")]
        Alphabetical,
        [Description("Favorites")]
        Favorites,
        [Description("Most Accessed")]
        MostAccessed,
        [Description("Recently Added")]
        RecentlyAdded,
        [Description("Recently Updated")]
        RecentlyUpdated
    }
}