using Sentry.FeatureFlags;

namespace Sentry.data.Core
{
    public interface IDataFeatures
    {
        IFeatureFlag<bool> Expose_BusinessArea_Pages_CLA_1424 { get; }
        IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; }
        IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; }
        IFeatureFlag<bool> Expose_TrainingMaterials_CLA_911 { get; }
    }
}
