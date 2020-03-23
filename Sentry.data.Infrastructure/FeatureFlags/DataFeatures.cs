using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;
using Sentry.Configuration;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    public class DataFeatures : IDataFeatures
    {
        private static IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();

        public IFeatureFlag<bool> Expose_BusinessArea_Pages_CLA_1424 { get; } = new BooleanFeatureFlag("Expose_BusinessArea_Pages_CLA_1424", configRepo);
        public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; } = new BooleanFeatureFlag("Remove_Mock_Uncompress_Logic_CLA_759", configRepo);
        public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; } = new BooleanFeatureFlag("Remove_ConvertToParquet_Logic_CLA_747", configRepo);
    }
}
