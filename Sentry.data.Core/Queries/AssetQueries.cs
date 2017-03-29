using System.Linq;

namespace Sentry.data.Core
{
    public static class AssetQueries
    {
        public static IQueryable<Asset> WhereUp(this IQueryable<Asset> source)
        {
            return source.Where(((i) => i.DynamicDetails.State == AssetState.Up));
        }

        public static IQueryable<Asset> WhereDown(this IQueryable<Asset> source)
        {
            return source.Where(((i) => i.DynamicDetails.State == AssetState.Down));
        }

        public static IQueryable<Asset> WhereUnknown(this IQueryable<Asset> source)
        {
            return source.Where(((i) => i.DynamicDetails.State == AssetState.Unknown));
        }

        public static IQueryable<Asset> WhereWaiting(this IQueryable<Asset> source)
        {
            return source.Where(((i) => i.DynamicDetails.State == AssetState.Waiting));
        }

        public static IQueryable<Asset> InCategory(this IQueryable<Asset> source, Category category)
        {
            return source.Where(((i) => i.Categories.Contains(category)));
        }

    }
}
