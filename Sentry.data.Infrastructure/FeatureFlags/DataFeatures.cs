using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    public class DataFeatures : IDataFeatures
    {
        private static IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();

        public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; } = new BooleanFeatureFlag("Remove_Mock_Uncompress_Logic_CLA_759", configRepo);
        public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; } = new BooleanFeatureFlag("Remove_ConvertToParquet_Logic_CLA_747", configRepo);
        public IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; } = new BooleanFeatureFlag("Remove_Mock_GoogleAPI_Logic_CLA_1679", configRepo);
        public IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; } = new BooleanFeatureFlag("Remove_ClaimIQ_mock_logic_CLA_758", configRepo);
        public IFeatureFlag<bool> Use_AWS_v2_Configuration_CLA_1488 { get; } = new BooleanFeatureFlag("Use_AWS_v2_Configuration_CLA_1488", configRepo);
        public IFeatureFlag<bool> Confluent_Kafka_CLA_1793 { get; } = new BooleanFeatureFlag("Confluent_Kafka_CLA_1793", configRepo);
        public IFeatureFlag<bool> Dale_Expose_EditOwnerVerified_CLA_1911 { get; } = new BooleanFeatureFlag("Dale_Expose_EditOwnerVerified_CLA_1911", configRepo);

    }
}
