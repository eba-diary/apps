namespace Sentry.data.Core
{
    //Represents permission names from Obsidian
    public class PermissionNames
    {
        public const string UseApp = "App_DataMgmt_UseApp";
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        // JCG TODO: Revisit removing unused App_DataMgmt_ApproveItem
        public const string ApproveItems = "App_DataMgmt_ApproveItems";
        // JCG TODO: Revisit removing unused App_DataMgmt_AddItems
        public const string AddItems = "App_DataMgmt_AddItems";
        public const string DwnldSensitve = "App_DataMgmt_SensDwnld";
        public const string DwnldNonSensitive = "App_DataMgmt_NonSensDwnld";
        public const string Upload = "App_DataMgmt_Upload";
        public const string DatasetEdit = "App_DataMgmt_Edit";
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        // JCG TODO: Revisit removing unused App_DataMgmt_UserSwitch
        public const string UserSwitch = "App_DataMgmt_UserSwitch";
        public const string ManageDataFileConfigs = "App_DataMgmt_MngConfig";
        public const string ManageAssetNotifications = "App_DataMgmt_MgAlert";
    }
}
