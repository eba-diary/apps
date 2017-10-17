using System;

namespace Sentry.data.Web
{
    public class HeaderModel
    {
        public Boolean CanUseApp { get; set; }
        public Boolean CanUserSwitch { get; set; }
        public string CurrentUserName { get; set; }
        public Boolean IsImpersonating { get; set; }
        public string RealUserName { get; set; }
        public string AssociatePhotoUrl { get; set; }
        public string EnvironmentName { get; set; }
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        public Boolean CanApproveAssets { get; set; }
        public Boolean CanManageConfigs { get; set; }
        //public bool HasMenu { get; set; }
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
