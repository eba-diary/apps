using Sentry.FeatureFlags;

namespace Sentry.data.Core
{
    public interface IDataFeatures
    {
        IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; }
        IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; }
        IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; }
        IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; }
        IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 { get; }
        IFeatureFlag<bool> CLA3241_DisableDfsDropLocation { get; }
        IFeatureFlag<bool> CLA3329_Expose_HR_Category { get; }
        IFeatureFlag<bool> CLA1656_DataFlowEdit_ViewEditPage { get; }
        IFeatureFlag<bool> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS { get; }
        IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS { get; }
        IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY { get; }
        IFeatureFlag<bool> CLA3553_SchemaSearch { get; }
        IFeatureFlag<bool> CLA3819_EgressEdgeMigration { get; }
        IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY { get; }
        IFeatureFlag<bool> CLA4049_ALLOW_S3_FILES_DELETE { get; }
        IFeatureFlag<bool> CLA4152_UploadFileFromUI { get; }
        IFeatureFlag<bool> CLA1130_SHOW_ALTERNATE_EMAIL { get; }
        IFeatureFlag<bool> CLA4310_UseHttpClient { get; }
        IFeatureFlag<string> CLA4260_QuartermasterNamedEnvironmentTypeFilter { get; }
        IFeatureFlag<bool> CLA3756_UpdateSearchPages { get; }
        IFeatureFlag<bool> CLA4258_DefaultProdSearchFilter { get; }
        IFeatureFlag<bool> CLA3878_ManageSchemasAccordion { get; }
        IFeatureFlag<bool> CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL { get; }
        IFeatureFlag<bool> CLA4411_Goldeneye_Consume_NP_Topics { get; }
        IFeatureFlag<bool> CLA4553_PlatformActivity { get; }
        IFeatureFlag<bool> CLA5112_PlatformActivity_TotalFiles_ViewPage { get; }
        IFeatureFlag<bool> CLA1797_DatasetSchemaMigration { get; }
        IFeatureFlag<bool> CLA4925_ParquetFileType { get; }
        IFeatureFlag<bool> CLA4912_API { get; }
        IFeatureFlag<bool> CLA5024_PublishReprocessingEvents { get; }
        IFeatureFlag<bool> CLA4993_JSMTicketProvider { get; }
        IFeatureFlag<bool> CLA4789_ImprovedSearchCapability { get; }
        IFeatureFlag<bool> CLA3214_VariantDataType { get; }
        IFeatureFlag<bool> CLA5211_SendNewSnowflakeEvents { get; }
        IFeatureFlag<bool> CLA4870_DSCAssistance { get; }
        IFeatureFlag<bool> CLA5260_PlatformActivity_FileFailures_ViewPage { get; }
    }
}
