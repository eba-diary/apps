using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum TileSearchSortByOption
    {
        [Description("Alphabetical")]
        Alphabetical,
        [Description("Favorites")]
        Favorites,
        [Description("Recently Added")]
        RecentlyAdded,
        [Description("Recently Updated")]
        RecentlyUpdated
    }
}
