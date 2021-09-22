using Sentry.FeatureFlags;

namespace Sentry.data.Core
{
    public interface IDataFeatures
    {
        IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 { get; }
        IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 { get; }
        IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 { get; }
        IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 { get; }
        IFeatureFlag<bool> Dale_Expose_EditOwnerVerified_CLA_1911 { get; }
        IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 { get; }
        IFeatureFlag<string> CLA2671_RefactorEventsToJava { get; }
        IFeatureFlag<string> CLA2671_RefactoredDataFlows { get; }
        IFeatureFlag<bool> Expose_HR_Category_CLA_3329 { get; }
        IFeatureFlagRequiringContext<bool, string> CLA1656_DataFlowEdit_ViewEditPage { get; }
        IFeatureFlagRequiringContext<bool, string> CLA1656_DataFlowEdit_SubmitEditPage { get; }
        IFeatureFlag<bool> CLA3240_UseDropLocationV2 { get; }
        IFeatureFlag<bool> CLA3241_DisableDfsDropLocation { get; }
        IFeatureFlag<bool> CLA3332_ConsolidatedDataFlows { get; }

    }
}
