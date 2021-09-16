using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;
using Sentry.FeatureFlags.Sql;
using System;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    public class DataFeatures : IDataFeatures
    {
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;
        private static SqlConfiguration databaseConfig =
            new SqlConfiguration(Sentry.Configuration.Config.GetHostSetting("DatabaseConnectionString"))
            {
                SchemaOptions = new SqlConfiguration.SchemaDefinition()
                {
                    KeyColumnName = "KeyCol"
                }
            };
        public readonly static IReadableFeatureRepository databaseRepo_longCache = FeatureRepository.CreateCachedRepository(databaseConfig, TimeSpan.FromDays(1));
        private static readonly IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();

        public DataFeatures(ISecurityService securityService, UserService userService)
        {
            _securityService = securityService;
            _userService = userService;

            CLA1656_DataFlowEdit_ViewEditPage = new BooleanFeatureFlagRequiringContext<string>("CLA1656_DataFlowEdit_ViewEditPage",
                                                                                                        databaseRepo_longCache,
                                                                                                        new AdminUserBaseConditionalStrategy(_securityService, _userService, databaseRepo_longCache));

            CLA1656_DataFlowEdit_SubmitEditPage = new BooleanFeatureFlagRequiringContext<string>("CLA1656_DataFlowEdit_SubmitEditPage",
                                                                                                        databaseRepo_longCache,
                                                                                                        new AdminUserBaseConditionalStrategy(_securityService, _userService, databaseRepo_longCache));
        }


        /* 
            Configuration file feature flags
        */
        public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; } = new BooleanFeatureFlag("Remove_Mock_Uncompress_Logic_CLA_759", configRepo);
        public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; } = new BooleanFeatureFlag("Remove_ConvertToParquet_Logic_CLA_747", configRepo);
        public IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; } = new BooleanFeatureFlag("Remove_Mock_GoogleAPI_Logic_CLA_1679", configRepo);
        public IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; } = new BooleanFeatureFlag("Remove_ClaimIQ_mock_logic_CLA_758", configRepo);
        public IFeatureFlag<bool> Dale_Expose_EditOwnerVerified_CLA_1911 { get; } = new BooleanFeatureFlag("Dale_Expose_EditOwnerVerified_CLA_1911", configRepo);
        public IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 { get; } = new BooleanFeatureFlag("Expose_Dataflow_Metadata_CLA_2146", configRepo);

        /* 
            Database feature flags
        */
        public IFeatureFlag<string> CLA2671_RefactorEventsToJava { get; } = new StringFeatureFlag("CLA2671_RefactorEventsToJava", databaseRepo_longCache);
        public IFeatureFlag<string> CLA2671_RefactoredDataFlows { get; } = new StringFeatureFlag("CLA2671_RefactoredDataFlows", databaseRepo_longCache);
        public IFeatureFlagRequiringContext<bool, string> CLA1656_DataFlowEdit_ViewEditPage { get; }
        public IFeatureFlagRequiringContext<bool, string> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        public IFeatureFlag<bool> CLA3240_UseDropLocationV2 { get; } = new BooleanFeatureFlag("CLA3240_UseDropLocationV2", databaseRepo_longCache);
        public IFeatureFlag<bool> CLA3241_DisableDfsDropLocation { get; } = new BooleanFeatureFlag("CLA3241_DisableDfsDropLocation", databaseRepo_longCache);
    }
}
