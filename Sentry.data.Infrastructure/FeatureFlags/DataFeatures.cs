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
        public readonly static IWritableFeatureRepository databaseRepo_longCache = FeatureRepository.CreateCachedRepository(databaseConfig, TimeSpan.FromDays(1));
        private static readonly IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();

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
        public IFeatureFlag<string> CLA2671_RefactorEventsToJava { get; } = new StringFeatureFlag("CLA2671_RefactorEventsToJava", databaseRepo_longCache);
        public IFeatureFlag<string> CLA2671_RefactoredDataFlows { get; } = new StringFeatureFlag("CLA2671_RefactoredDataFlows", databaseRepo_longCache);
    }
}
