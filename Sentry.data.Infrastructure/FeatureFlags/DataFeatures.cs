using LaunchDarkly.Sdk.Server;
using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;
using Sentry.FeatureFlags.Sql;
using Sentry.FeatureFlags.LaunchDarkly;
using System;
using System.Linq;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    /// <summary>
    /// Feature Flag class for DSC - this class holds all the feature flags that the application needs to evaluate
    /// This class is registered as a singleton in the bootstrapper so that everyone that requests this class gets the same copy
    /// This is needed so that there's only one LdClient instance for the entire application
    /// </summary>
    public class DataFeatures : IDataFeatures, IDisposable
    {
        private readonly UserService _userService;
        private readonly LdClient LdClient;

        // LaunchDarkly feature flags - property definitions
        public IFeatureFlag<bool> CLA1656_DataFlowEdit_ViewEditPage { get; }
        public IFeatureFlag<bool> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        public IFeatureFlag<bool> CLA3329_Expose_HR_Category { get; }
        public IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS { get; }
        public IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS { get; }
        public IFeatureFlag<bool> CLA3541_Dataset_Details_Tabs { get; }
        public IFeatureFlag<bool> CLA3332_ConsolidatedDataFlows { get; }
        public IFeatureFlag<bool> CLA3497_UniqueLivySessionName { get; }
        public IFeatureFlag<bool> CLA3240_UseDropLocationV2 { get; }
        public IFeatureFlag<bool> CLA3605_AllowSchemaParquetUpdate { get; }
        public IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY { get; }
        public IFeatureFlag<bool> CLA3553_SchemaSearch { get; }
        public IFeatureFlag<bool> CLA3861_RefactorGetUserSecurity { get; }
        public IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY { get; }
        public IFeatureFlag<bool> CLA3718_Authorization { get; }


        public DataFeatures(UserService userService)
        {
            _userService = userService;
            LdClient = new LdClientFactory().BuildLdClient();

            // LaunchDarkly feature flags - property initialization
            CLA1656_DataFlowEdit_ViewEditPage = new BooleanFeatureFlagAmbientContext("CLA1656_DataFlowEdit_ViewEditPage", false, LdClient, () => LdUser);
            CLA1656_DataFlowEdit_SubmitEditPage = new BooleanFeatureFlagAmbientContext("CLA1656_DataFlowEdit_SubmitEditPage", false, LdClient, () => LdUser);
            CLA3329_Expose_HR_Category = new BooleanFeatureFlagAmbientContext("CLA3329_Expose_HR_Category", false, LdClient, () => LdUser);
            CLA2838_DSC_ANOUNCEMENTS = new BooleanFeatureFlagAmbientContext("CLA2838_DSC_ANOUNCEMENTS", false, LdClient, () => LdUser);
            CLA3332_ConsolidatedDataFlows = new BooleanFeatureFlagAmbientContext("CLA3332_ConsolidatedDataFlows", false, LdClient, () => LdUser);
            CLA3497_UniqueLivySessionName = new BooleanFeatureFlagAmbientContext("CLA3497_UniqueLivySessionName", false, LdClient, () => LdUser);
            CLA3550_DATA_INVENTORY_NEW_COLUMNS = new BooleanFeatureFlagAmbientContext("CLA3550_DATA_INVENTORY_NEW_COLUMNS", false, LdClient, () => LdUser);
            CLA3541_Dataset_Details_Tabs = new BooleanFeatureFlagAmbientContext("CLA3541_DatasetDetailsTabs", false, LdClient, () => LdUser);
            CLA3240_UseDropLocationV2 = new BooleanFeatureFlagAmbientContext("CLA3240_UseDropLocationV2", false, LdClient, () => LdUser);
            CLA3605_AllowSchemaParquetUpdate = new BooleanFeatureFlagAmbientContext("CLA3605_AllowSchemaParquetUpdate", false, LdClient, () => LdUser);
            CLA3637_EXPOSE_INV_CATEGORY = new BooleanFeatureFlagAmbientContext("CLA3637_EXPOSE_INV_CATEGORY", false, LdClient, () => LdUser);
            CLA3553_SchemaSearch = new BooleanFeatureFlagAmbientContext("CLA3553_SchemaSearch", false, LdClient, () => LdUser);
            CLA3882_DSC_NOTIFICATION_SUBCATEGORY = new BooleanFeatureFlagAmbientContext("CLA3882_DSC_NOTIFICATION_SUBCATEGORY", false, LdClient, () => LdUser);
            CLA3861_RefactorGetUserSecurity = new BooleanFeatureFlagAmbientContext("CLA3861_RefactorGetUserSecurity", false, LdClient, () => LdUser);
            CLA3718_Authorization = new BooleanFeatureFlagAmbientContext("CLA3718_Authorization", false, LdClient, () => LdUser);
        }

        /// <summary>
        /// This property builds the LdUser object that LaunchDarkly uses to evaluate feature flags
        /// </summary>
        private LaunchDarkly.Sdk.User LdUser
        {
            get
            {
                var permissionsBuilder = LaunchDarkly.Sdk.LdValue.BuildArray();
                _userService.GetCurrentUser().Permissions.ToList().ForEach((p) => permissionsBuilder.Add(p));
                return LaunchDarkly.Sdk.User.Builder(_userService.GetCurrentUser().AssociateId).Custom("Permissions", permissionsBuilder.Build()).Build();
            }
        }

        #region "Legacy Feature Flags"

        private static SqlConfiguration databaseConfig =
            new SqlConfiguration(Sentry.Configuration.Config.GetHostSetting("DatabaseConnectionString"))
            {
                SchemaOptions = new SqlConfiguration.SchemaDefinition()
                {
                    KeyColumnName = "KeyCol"
                }
            };

        public readonly static IWritableFeatureRepository databaseRepo_longCache = FeatureRepository.CreateCachedRepository(databaseConfig, TimeSpan.FromMinutes(5));
        private static readonly IReadableFeatureRepository configRepo = new Sentry.FeatureFlags.SentryConfig.FeatureRepository();


        /* 
            Configuration file feature flags
        */
        public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; } = new BooleanFeatureFlag("Remove_Mock_Uncompress_Logic_CLA_759", configRepo);
        public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; } = new BooleanFeatureFlag("Remove_ConvertToParquet_Logic_CLA_747", configRepo);
        public IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; } = new BooleanFeatureFlag("Remove_Mock_GoogleAPI_Logic_CLA_1679", configRepo);
        public IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; } = new BooleanFeatureFlag("Remove_ClaimIQ_mock_logic_CLA_758", configRepo);
        public IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 { get; } = new BooleanFeatureFlag("Expose_Dataflow_Metadata_CLA_2146", configRepo);

        /* 
            Database feature flags
        */
        public IFeatureFlag<bool> CLA3241_DisableDfsDropLocation { get; } = new BooleanFeatureFlag("CLA3241_DisableDfsDropLocation", databaseRepo_longCache);
        public IFeatureFlag<bool> CLA3048_StandardizeOnUTCTime { get; } = new BooleanFeatureFlag("CLA3048_StandardizeOnUTCTime", databaseRepo_longCache);
        public IFeatureFlag<bool> CLA3819_EgressEdgeMigration { get; } = new BooleanFeatureFlag("CLA3819_EgressEdgeMigration", databaseRepo_longCache);

        #endregion

        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                LdClient.Flush();
            }

            disposed = true;
        }
        #endregion
    }
}
