using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Repo;
using Sentry.FeatureFlags.Sql;
using Sentry.FeatureFlags.LaunchDarkly;
using System;
using System.Linq;
using LaunchDarkly.Sdk.Server.Interfaces;
using LaunchDarkly.Sdk;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    /// <summary>
    /// Feature Flag class for DSC - this class holds all the feature flags that the application needs to evaluate
    /// </summary>
    public class DataFeatures : IDataFeatures
    {
        private readonly IUserService _userService;
        private readonly ILdClient _ldClient;

        // LaunchDarkly feature flags - property definitions
        public IFeatureFlag<bool> CLA4553_PlatformActivity { get;  }
        public IFeatureFlag<bool> CLA5112_PlatformActivity_TotalFiles_ViewPage { get; }
        public IFeatureFlag<bool> CLA1656_DataFlowEdit_ViewEditPage { get; }
        public IFeatureFlag<bool> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        public IFeatureFlag<bool> CLA3329_Expose_HR_Category { get; }
        public IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS { get; }
        public IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS { get; }
        public IFeatureFlag<bool> CLA3497_UniqueLivySessionName { get; }
        public IFeatureFlag<bool> CLA3605_AllowSchemaParquetUpdate { get; }
        public IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY { get; }
        public IFeatureFlag<bool> CLA3553_SchemaSearch { get; }
        public IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY { get; }
        public IFeatureFlag<bool> CLA4049_ALLOW_S3_FILES_DELETE { get; }
        public IFeatureFlag<bool> CLA4152_UploadFileFromUI { get; }
        public IFeatureFlag<bool> CLA1130_SHOW_ALTERNATE_EMAIL { get; }
        public IFeatureFlag<bool> CLA4310_UseHttpClient { get; }
        public IFeatureFlag<string> CLA4260_QuartermasterNamedEnvironmentTypeFilter { get; }
        public IFeatureFlag<bool> CLA3756_UpdateSearchPages { get; }
        public IFeatureFlag<bool> CLA4258_DefaultProdSearchFilter { get; }
        public IFeatureFlag<bool> CLA3878_ManageSchemasAccordion { get; }
        public IFeatureFlag<bool> CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL { get; }
        public IFeatureFlag<bool> CLA4411_Goldeneye_Consume_NP_Topics { get; }
        public IFeatureFlag<bool> CLA3945_Telematics { get; }
        public IFeatureFlag<bool> CLA2868_APIPaginationSupport { get; }
        public IFeatureFlag<bool> CLA1797_DatasetSchemaMigration { get; }
        public IFeatureFlag<bool> CLA4485_DropCompaniesFile { get; }
        public IFeatureFlag<bool> CLA2869_AllowMotiveJobs { get; }
        public IFeatureFlag<bool> CLA4931_SendMotiveEmail { get; }
        public IFeatureFlag<bool> CLA4925_ParquetFileType { get; }
        public IFeatureFlag<bool> CLA4912_API { get; }
        public IFeatureFlag<bool> CLA5024_PublishReprocessingEvents { get; }
        public IFeatureFlag<bool> CLA4993_JSMTicketProvider { get; }
        public IFeatureFlag<bool> CLA4789_ImprovedSearchCapability { get; }
        public IFeatureFlag<bool> CLA3214_VariantDataType { get; }

        public DataFeatures(IUserService userService, ILdClient ldClient)
        {
            _userService = userService;
            _ldClient = ldClient;

            // LaunchDarkly feature flags - property initialization
            CLA4553_PlatformActivity = new BooleanFeatureFlagAmbientContext("CLA4553_PlatformActivity", false, _ldClient, () => LdUser);
            CLA5112_PlatformActivity_TotalFiles_ViewPage = new BooleanFeatureFlagAmbientContext("CLA5112_PlatformActivity_TotalFiles_ViewPage", false, _ldClient, () => LdUser);
            CLA1656_DataFlowEdit_ViewEditPage = new BooleanFeatureFlagAmbientContext("CLA1656_DataFlowEdit_ViewEditPage", false, _ldClient, () => LdUser);
            CLA1656_DataFlowEdit_SubmitEditPage = new BooleanFeatureFlagAmbientContext("CLA1656_DataFlowEdit_SubmitEditPage", false, _ldClient, () => LdUser);
            CLA3329_Expose_HR_Category = new BooleanFeatureFlagAmbientContext("CLA3329_Expose_HR_Category", false, _ldClient, () => LdUser);
            CLA2838_DSC_ANOUNCEMENTS = new BooleanFeatureFlagAmbientContext("CLA2838_DSC_ANOUNCEMENTS", false, _ldClient, () => LdUser);
            CLA3497_UniqueLivySessionName = new BooleanFeatureFlagAmbientContext("CLA3497_UniqueLivySessionName", false, _ldClient, () => LdUser);
            CLA3550_DATA_INVENTORY_NEW_COLUMNS = new BooleanFeatureFlagAmbientContext("CLA3550_DATA_INVENTORY_NEW_COLUMNS", false, _ldClient, () => LdUser);
            CLA3605_AllowSchemaParquetUpdate = new BooleanFeatureFlagAmbientContext("CLA3605_AllowSchemaParquetUpdate", false, _ldClient, () => LdUser);
            CLA3637_EXPOSE_INV_CATEGORY = new BooleanFeatureFlagAmbientContext("CLA3637_EXPOSE_INV_CATEGORY", false, _ldClient, () => LdUser);
            CLA3553_SchemaSearch = new BooleanFeatureFlagAmbientContext("CLA3553_SchemaSearch", false, _ldClient, () => LdUser);
            CLA3882_DSC_NOTIFICATION_SUBCATEGORY = new BooleanFeatureFlagAmbientContext("CLA3882_DSC_NOTIFICATION_SUBCATEGORY", false, _ldClient, () => LdUser);
            CLA4049_ALLOW_S3_FILES_DELETE = new BooleanFeatureFlagAmbientContext("CLA4049_ALLOW_S3_FILES_DELETE", false, _ldClient, () => LdUser);
            CLA4152_UploadFileFromUI = new BooleanFeatureFlagAmbientContext("CLA4152_UploadFileFromUI", false, _ldClient, () => LdUser);
            CLA1130_SHOW_ALTERNATE_EMAIL = new BooleanFeatureFlagAmbientContext("CLA1130_SHOW_ALTERNATE_EMAIL", false, _ldClient, () => LdUser);
            CLA4310_UseHttpClient = new BooleanFeatureFlagAmbientContext("CLA4310_UseHttpClient", false, _ldClient, () => LdUser);
            CLA4260_QuartermasterNamedEnvironmentTypeFilter = new StringFeatureFlagAmbientContext("CLA4260_QuartermasterNamedEnvironmentTypeFilter", "Prod", _ldClient, () => LdUser);
            CLA3756_UpdateSearchPages = new BooleanFeatureFlagAmbientContext("CLA3756_UpdateSearchPages", false, _ldClient, () => LdUser);
            CLA4258_DefaultProdSearchFilter = new BooleanFeatureFlagAmbientContext("CLA4258_DefaultProdSearchFilter", false, _ldClient, () => LdUser);
            CLA3878_ManageSchemasAccordion = new BooleanFeatureFlagAmbientContext("CLA3878_ManageSchemasAccordion", false, _ldClient, () => LdUser);
            CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL = new BooleanFeatureFlagAmbientContext("CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL", false, _ldClient, () => LdUser);
            CLA4411_Goldeneye_Consume_NP_Topics = new BooleanFeatureFlagAmbientContext("CLA4411_Goldeneye_Consume_NP_Topics", false, _ldClient, () => LdUser);
            CLA3945_Telematics = new BooleanFeatureFlagAmbientContext("CLA3945_Telematics", false, _ldClient, () => LdUser);
            CLA2868_APIPaginationSupport = new BooleanFeatureFlagAmbientContext("CLA2868_APIPaginationSupport", false, _ldClient, () => LdUser);
            CLA1797_DatasetSchemaMigration = new BooleanFeatureFlagAmbientContext("CLA1797_DatasetSchemaMigration", false, _ldClient, () => LdUser);
            CLA4485_DropCompaniesFile = new BooleanFeatureFlagAmbientContext("CLA4485_DropCompaniesFile", false, _ldClient, () => LdUser);
            CLA2869_AllowMotiveJobs = new BooleanFeatureFlagAmbientContext("CLA2869_AllowMotiveJobs", false, _ldClient, () => LdUser);
            CLA4931_SendMotiveEmail = new BooleanFeatureFlagAmbientContext("CLA4931_SendMotiveEmail", false, _ldClient, () => LdUser);
            CLA4925_ParquetFileType = new BooleanFeatureFlagAmbientContext("CLA4925_ParquetFileType", false, _ldClient, () => LdUser);
            CLA4912_API = new BooleanFeatureFlagAmbientContext("CLA4912_API", false, _ldClient, () => LdUser);
            CLA5024_PublishReprocessingEvents = new BooleanFeatureFlagAmbientContext("CLA5024_PublishReprocessingEvents", false, _ldClient, () => LdUser);
            CLA4993_JSMTicketProvider = new BooleanFeatureFlagAmbientContext("CLA4993_JSMTicketProvider", false, _ldClient, () => LdUser);
            CLA4789_ImprovedSearchCapability = new BooleanFeatureFlagAmbientContext("CLA4789_ImprovedSearchCapability", false, _ldClient, () => LdUser);
            CLA3214_VariantDataType = new BooleanFeatureFlagAmbientContext("CLA3214_VariantDataType", false, _ldClient, () => LdUser);
        }

        /// <summary>
        /// This property builds the LdUser object that LaunchDarkly uses to evaluate feature flags
        /// </summary>
        private User LdUser
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
