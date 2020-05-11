﻿using Sentry.FeatureFlags;

namespace Sentry.data.Core
{
    public interface IDataFeatures
    {
        IFeatureFlag<bool> Remove_Mock_UncompressGzip_Logic_CLA_757 { get; }
        IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; }
        IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; }
        IFeatureFlag<bool> Expose_TrainingMaterials_CLA_911 { get; }
        IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; }
        IFeatureFlag<bool> Expose_DaleSearch_CLA_1450 { get; }
        IFeatureFlag<bool> Use_AWS_v2_Configuration_CLA_1488 { get; }
    }
}
