using System;

namespace Sentry.data.Web
{
    public class HeaderModel
    {
        public Boolean ShowAdminControls { get; set; }
        public Boolean CanUseApp { get; set; }
        public Boolean CanUserSwitch { get; set; }
        public string CurrentUserName { get; set; }
        public Boolean IsImpersonating { get; set; }
        public string RealUserName { get; set; }
        public string AssociatePhotoUrl { get; set; }
        public string EnvironmentName { get; set; }

        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        public Boolean CanApproveAssets { get; set; }
        public Boolean CanEditDataset { get; set; }
        public Boolean CanManageAssetAlerts { get; set; }
        public Boolean CanViewOnly { get; set; }
        public Boolean CanViewDataset { get; set; }
        public Boolean CanViewDataAsset { get; set; }
        public Boolean CanViewReports { get; set; }
        public Boolean CanManageReports { get; set; }
        public Boolean CanViewBusinessArea { get; set; }
        public Boolean CanViewDataInventory { get; set; }
        public Boolean DisplayDataflowMetadata { get; set; }
        public bool DirectToSearchPages { get; set; }
    }
}
