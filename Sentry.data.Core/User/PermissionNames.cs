namespace Sentry.data.Core
{
    //Represents permission names from Obsidian
    public class PermissionNames
    {
        public const string UseApp = "UseApp";
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        // JCG TODO: Revisit removing unused App_DataMgmt_ApproveItem
        public const string DatasetView = "DatasetView";
        public const string DatasetAsset = "DataAssetView";
        public const string ApproveItems = "App_DataMgmt_ApproveItems";
        // JCG TODO: Revisit removing unused App_DataMgmt_AddItems
        public const string AddItems = "App_DataMgmt_AddItems";
        public const string DwnldSensitive = "DatasetSensDwnld";
        public const string DwnldNonSensitive = "DatasetNonSensDwnld";
        public const string Upload = "DatasetUpload";
        public const string DatasetEdit = "DatasetModify";
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        // JCG TODO: Revisit removing unused App_DataMgmt_UserSwitch
        public const string UserSwitch = "UserSwitch";
        public const string ManageDataFileConfigs = "DatasetMngConfig";
        public const string ManageAssetNotifications = "DataAssetMngAlert";
        public const string QueryToolUser = "QueryToolUser";
        public const string QueryToolPowerUser = "QueryToolPowerUser";
        public const string QueryToolAdmin = "QueryToolAdmin";
    }
}
