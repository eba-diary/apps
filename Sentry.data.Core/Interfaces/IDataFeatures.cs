using Sentry.FeatureFlags;

namespace Sentry.data.Core
{
    public interface IDataFeatures
    {
        IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; }
        IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; }
        IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; }
        IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; }
        IFeatureFlag<bool> Confluent_Kafka_CLA_1793 { get; }
        IFeatureFlag<bool> Dale_Expose_EditOwnerVerified_CLA_1911 { get; }
        IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 { get; }
        IFeatureFlag<bool> Remove_DfsWatchers_CLA_2346 { get; }
        IFeatureFlag<bool> CLA2671_DFSEVENTEventHandler { get; }
        IFeatureFlag<bool> CLA2671_S3DropEventHandler { get; }
        IFeatureFlag<bool> CLA2671_RawStorageEventHandler { get; }
        IFeatureFlag<bool> CLA2671_QueryStorageEventHandler { get; }
        IFeatureFlag<bool> CLA2671_SchemaMapEventHandler { get; }
        IFeatureFlag<bool> CLA2671_SchemaLoadEventHandler { get; }

    }
}
