﻿using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    public class DataFeatures : IDataFeatures
    {
        private static IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();

        public IFeatureFlag<bool> Remove_Mock_UncompressGzip_Logic_CLA_757 { get; } = new BooleanFeatureFlag("Remove_Mock_UncompressGzip_Logic_CLA_757", configRepo);
        public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; } = new BooleanFeatureFlag("Remove_Mock_Uncompress_Logic_CLA_759", configRepo);
        public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; } = new BooleanFeatureFlag("Remove_ConvertToParquet_Logic_CLA_747", configRepo);
        public IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; } = new BooleanFeatureFlag("Remove_Mock_GoogleAPI_Logic_CLA_1679", configRepo);
        public IFeatureFlag<bool> Expose_TrainingMaterials_CLA_911 { get; } = new BooleanFeatureFlag("Expose_TrainingMaterials_CLA_911", configRepo);
        public IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; } = new BooleanFeatureFlag("Remove_ClaimIQ_mock_logic_CLA_758", configRepo);
        public IFeatureFlag<bool> Expose_DaleSearch_CLA_1450 { get; } = new BooleanFeatureFlag("Expose_DaleSearch_CLA_1450", configRepo);
    }
}
