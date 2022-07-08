
namespace Sentry.data.Core
{
    public class UserSecurity
    {

        public UserSecurity() { }

        //DSC kept permissions
        public bool CanPreviewDataset { get; set; }
        public bool CanViewFullDataset { get; set; }
        public bool CanQueryDataset { get; set; }
        public bool CanUploadToDataset { get; set; }
        public bool CanUseDataSource { get; set; }

        public bool CanModifyNotifications { get; set; }



        //Obsidian kept permissions
        public bool CanEditDataset { get; set; }
        public bool CanCreateDataset { get; set; }

        public bool CanEditReport { get; set; }
        public bool CanCreateReport { get; set; }

        public bool CanEditDataSource { get; set; }
        public bool CanCreateDataSource { get; set; }

        public bool ShowAdminControls { get; set; }
        public bool CanManageSchema { get; set; }

        //Derived Permissions
        // Based on more than one other permission to contain logic within single property
        public bool CanCreateDataFlow { get; set; }
        public bool CanModifyDataflow { get; set; }
        public bool CanViewData { get; set; }
        public bool CanDeleteDatasetFile { get; set; }
    }
}
