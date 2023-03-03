using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;
using Sentry.FeatureFlags.Sql;
using Sentry.FeatureFlags.LaunchDarkly;
using System;
using System.Linq;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    /// <summary>
    /// Feature Flag class for DSC - this class holds all the feature flags that the application needs to evaluate
    /// </summary>
    public class DataFeatures : IDataFeatures
    {
        private readonly UserService _userService;
        private readonly ILdClient _ldClient;

        // LaunchDarkly feature flags - property definitions
        public IFeatureFlag<bool> CLA1656_DataFlowEdit_ViewEditPage { get; }
        public IFeatureFlag<bool> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        public IFeatureFlag<bool> CLA3329_Expose_HR_Category { get; }
        public IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS { get; }
        public IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS { get; }
        public IFeatureFlag<bool> CLA3541_Dataset_Details_Tabs { get; }
        public IFeatureFlag<bool> CLA3497_UniqueLivySessionName { get; }
        public IFeatureFlag<bool> CLA3605_AllowSchemaParquetUpdate { get; }
        public IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY { get; }
        public IFeatureFlag<bool> CLA3553_SchemaSearch { get; }
        public IFeatureFlag<bool> CLA3861_RefactorGetUserSecurity { get; }
        public IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY { get; }
        public IFeatureFlag<bool> CLA3718_Authorization { get; }
        public IFeatureFlag<bool> CLA4049_ALLOW_S3_FILES_DELETE { get; }
        public IFeatureFlag<bool> CLA4152_UploadFileFromUI { get; }
        public IFeatureFlag<bool> CLA1130_SHOW_ALTERNATE_EMAIL { get; }
        public IFeatureFlag<bool> CLA4310_UseHttpClient { get; }
        public IFeatureFlag<string> CLA4260_QuartermasterNamedEnvironmentTypeFilter { get; }
        public IFeatureFlag<bool> CLA3756_UpdateSearchPages { get; }
        public IFeatureFlag<bool> CLA4258_DefaultProdSearchFilter { get; }
        public IFeatureFlag<bool> CLA4410_StopCategoryBasedConsumptionLayerCreation { get; }
        public IFeatureFlag<string> CLA440_CategoryConsumptionLayerCreateLineInSand { get; }
        public IFeatureFlag<bool> CLA3878_ManageSchemasAccordion { get; }
        public IFeatureFlag<bool> CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL { get; }
        public IFeatureFlag<bool> CLA4411_Goldeneye_Consume_NP_Topics { get; }
        public IFeatureFlag<bool> CLA3945_Telematics { get; }
        public IFeatureFlag<bool> CLA2868_APIPaginationSupport { get; }
        public IFeatureFlag<bool> CLA1797_DatasetSchemaMigration { get; }
        public IFeatureFlag<bool> CLA4553_PlatformActivity { get; }


        public DataFeatures(UserService userService, ILdClient ldClient)
        {
            _userService = userService;
            _ldClient = ldClient;

            // LaunchDarkly feature flags - property initialization
            CLA1656_DataFlowEdit_ViewEditPage = new BooleanFeatureFlagAmbientContext("CLA1656_DataFlowEdit_ViewEditPage", false, _ldClient, () => LdUser);
            CLA1656_DataFlowEdit_SubmitEditPage = new BooleanFeatureFlagAmbientContext("CLA1656_DataFlowEdit_SubmitEditPage", false, _ldClient, () => LdUser);
            CLA3329_Expose_HR_Category = new BooleanFeatureFlagAmbientContext("CLA3329_Expose_HR_Category", false, _ldClient, () => LdUser);
            CLA2838_DSC_ANOUNCEMENTS = new BooleanFeatureFlagAmbientContext("CLA2838_DSC_ANOUNCEMENTS", false, _ldClient, () => LdUser);
            CLA3497_UniqueLivySessionName = new BooleanFeatureFlagAmbientContext("CLA3497_UniqueLivySessionName", false, _ldClient, () => LdUser);
            CLA3550_DATA_INVENTORY_NEW_COLUMNS = new BooleanFeatureFlagAmbientContext("CLA3550_DATA_INVENTORY_NEW_COLUMNS", false, _ldClient, () => LdUser);
            CLA3541_Dataset_Details_Tabs = new BooleanFeatureFlagAmbientContext("CLA3541_DatasetDetailsTabs", false, _ldClient, () => LdUser);
            CLA3605_AllowSchemaParquetUpdate = new BooleanFeatureFlagAmbientContext("CLA3605_AllowSchemaParquetUpdate", false, _ldClient, () => LdUser);
            CLA3637_EXPOSE_INV_CATEGORY = new BooleanFeatureFlagAmbientContext("CLA3637_EXPOSE_INV_CATEGORY", false, _ldClient, () => LdUser);
            CLA3553_SchemaSearch = new BooleanFeatureFlagAmbientContext("CLA3553_SchemaSearch", false, _ldClient, () => LdUser);
            CLA3882_DSC_NOTIFICATION_SUBCATEGORY = new BooleanFeatureFlagAmbientContext("CLA3882_DSC_NOTIFICATION_SUBCATEGORY", false, _ldClient, () => LdUser);
            CLA3861_RefactorGetUserSecurity = new BooleanFeatureFlagAmbientContext("CLA3861_RefactorGetUserSecurity", false, _ldClient, () => LdUser);
            CLA3718_Authorization = new BooleanFeatureFlagAmbientContext("CLA3718_Authorization", false, _ldClient, () => LdUser);
            CLA4049_ALLOW_S3_FILES_DELETE = new BooleanFeatureFlagAmbientContext("CLA4049_ALLOW_S3_FILES_DELETE", false, _ldClient, () => LdUser);
            CLA4152_UploadFileFromUI = new BooleanFeatureFlagAmbientContext("CLA4152_UploadFileFromUI", false, _ldClient, () => LdUser);
            CLA1130_SHOW_ALTERNATE_EMAIL = new BooleanFeatureFlagAmbientContext("CLA1130_SHOW_ALTERNATE_EMAIL", false, _ldClient, () => LdUser);
            CLA4310_UseHttpClient = new BooleanFeatureFlagAmbientContext("CLA4310_UseHttpClient", false, _ldClient, () => LdUser);
            CLA4260_QuartermasterNamedEnvironmentTypeFilter = new StringFeatureFlagAmbientContext("CLA4260_QuartermasterNamedEnvironmentTypeFilter", "Prod", _ldClient, () => LdUser);
            CLA3756_UpdateSearchPages = new BooleanFeatureFlagAmbientContext("CLA3756_UpdateSearchPages", false, _ldClient, () => LdUser);
            CLA4258_DefaultProdSearchFilter = new BooleanFeatureFlagAmbientContext("CLA4258_DefaultProdSearchFilter", false, _ldClient, () => LdUser);
            CLA4410_StopCategoryBasedConsumptionLayerCreation = new BooleanFeatureFlagAmbientContext("CLA4410_StopCategoryBasedConsumptionLayerCreation", false, _ldClient, () => LdUser);
            CLA440_CategoryConsumptionLayerCreateLineInSand = new StringFeatureFlagAmbientContext("CLA440_CategoryConsumptionLayerCreateLineInSand", "2022-08-15", _ldClient, () => LdUser);
            CLA3878_ManageSchemasAccordion = new BooleanFeatureFlagAmbientContext("CLA3878_ManageSchemasAccordion", false, _ldClient, () => LdUser);
            CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL = new BooleanFeatureFlagAmbientContext("CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL", false, _ldClient, () => LdUser);
            CLA4411_Goldeneye_Consume_NP_Topics = new BooleanFeatureFlagAmbientContext("CLA4411_Goldeneye_Consume_NP_Topics", false, _ldClient, () => LdUser);
            CLA3945_Telematics = new BooleanFeatureFlagAmbientContext("CLA3945_Telematics", false, _ldClient, () => LdUser);
            CLA2868_APIPaginationSupport = new BooleanFeatureFlagAmbientContext("CLA2868_APIPaginationSupport", false, _ldClient, () => LdUser);
            CLA1797_DatasetSchemaMigration = new BooleanFeatureFlagAmbientContext("CLA1797_DatasetSchemaMigration", false, _ldClient, () => LdUser);
            CLA4553_PlatformActivity = new BooleanFeatureFlagAmbientContext("CLA4553_PlatformActivity", false, _ldClient, () => LdUser);
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
        public IFeatureFlag<bool> CLA3819_EgressEdgeMigration { get; } = new BooleanFeatureFlag("CLA3819_EgressEdgeMigration", databaseRepo_longCache);

        #endregion

    }
}
