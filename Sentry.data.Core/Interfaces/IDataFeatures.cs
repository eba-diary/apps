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
        IFeatureFlag<bool> CLA3240_UseDropLocationV2 { get; }
        IFeatureFlag<bool> CLA3241_DisableDfsDropLocation { get; }
        IFeatureFlag<bool> CLA3332_ConsolidatedDataFlows { get; }
        IFeatureFlag<bool> CLA3048_StandardizeOnUTCTime { get; }
        IFeatureFlag<bool> CLA3497_UniqueLivySessionName { get; }
        IFeatureFlag<bool> CLA3329_Expose_HR_Category { get; }
        IFeatureFlag<bool> CLA1656_DataFlowEdit_ViewEditPage { get; }
        IFeatureFlag<bool> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS { get; }
        IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS { get; }
        IFeatureFlag<bool> CLA3541_Dataset_Details_Tabs { get; }
        IFeatureFlag<bool> CLA3605_AllowSchemaParquetUpdate { get; }
        IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY { get; }
        IFeatureFlag<bool> CLA3553_SchemaSearch { get; }
        IFeatureFlag<bool> CLA3861_RefactorGetUserSecurity { get; }
        IFeatureFlag<bool> CLA3819_EgressEdgeMigration { get; }
        IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY { get; }
        IFeatureFlag<bool> CLA3718_Authorization { get; }
        IFeatureFlag<bool> CLA4049_ALLOW_S3_FILES_DELETE { get; }
        IFeatureFlag<bool> CLA4152_UploadFileFromUI { get; }
        IFeatureFlag<bool> CLA1130_SHOW_ALTERNATE_EMAIL { get; }
        IFeatureFlag<bool> CLA4310_UseHttpClient { get; }
        IFeatureFlag<bool> CLA3756_UpdateSearchPages { get; }
    }
}
