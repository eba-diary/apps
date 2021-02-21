using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;
using Sentry.FeatureFlags.Sql;
using System;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    public class DataFeatures : IDataFeatures
    {
        private static SqlConfiguration databaseConfig = new SqlConfiguration(Sentry.Configuration.Config.GetHostSetting("DatabaseConnectionString"));
        public static IWritableFeatureRepository databaseRepo = FeatureRepository.CreateCachedRepository(databaseConfig, TimeSpan.FromSeconds(30));

        private static IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();

        /* 
            Configuration file feature flags
        */
        public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; } = new BooleanFeatureFlag("Remove_Mock_Uncompress_Logic_CLA_759", configRepo);
        public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; } = new BooleanFeatureFlag("Remove_ConvertToParquet_Logic_CLA_747", configRepo);
        public IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; } = new BooleanFeatureFlag("Remove_Mock_GoogleAPI_Logic_CLA_1679", configRepo);
        public IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; } = new BooleanFeatureFlag("Remove_ClaimIQ_mock_logic_CLA_758", configRepo);
        public IFeatureFlag<bool> Confluent_Kafka_CLA_1793 { get; } = new BooleanFeatureFlag("Confluent_Kafka_CLA_1793", configRepo);
        public IFeatureFlag<bool> Dale_Expose_EditOwnerVerified_CLA_1911 { get; } = new BooleanFeatureFlag("Dale_Expose_EditOwnerVerified_CLA_1911", configRepo);
        public IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 { get; } = new BooleanFeatureFlag("Expose_Dataflow_Metadata_CLA_2146", configRepo);
        public IFeatureFlag<bool> Remove_DfsWatchers_CLA_2346 { get; } = new BooleanFeatureFlag("Remove_DfsWatchers_CLA_2346", configRepo);

        /* 
            Database feature flags
        */
        public IFeatureFlag<bool> CLA2671_DFSEVENTEventHandler { get; } = new BooleanFeatureFlag("CLA2671_DFSEVENTEventHandler", databaseRepo);
        public IFeatureFlag<bool> CLA2671_S3DropEventHandler { get; } = new BooleanFeatureFlag("CLA2671_S3DropEventHandler", databaseRepo);
        public IFeatureFlag<bool> CLA2671_RawStorageEventHandler { get; } = new BooleanFeatureFlag("CLA2671_RawStorageEventHandler", databaseRepo);
        public IFeatureFlag<bool> CLA2671_QueryStorageEventHandler { get; } = new BooleanFeatureFlag("CLA2671_QueryStorageEventHandler", databaseRepo);
        public IFeatureFlag<bool> CLA2671_SchemaMapEventHandler { get; } = new BooleanFeatureFlag("CLA2671_SchemaMapEventHandler", databaseRepo);
        public IFeatureFlag<bool> CLA2671_SchemaLoadEventHandler { get; } = new BooleanFeatureFlag("CLA2671_SchemaLoadEventHandler", databaseRepo);
    }
}
