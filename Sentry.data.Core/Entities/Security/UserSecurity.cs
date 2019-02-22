
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

        public bool CanModifyNotifications { get; set; }



        //Obsidian kept permissions
        public bool CanEditDataset { get; set; }
        public bool CanCreateDataset { get; set; }


        public bool CanEditReport { get; set; }
        public bool CanCreateReport { get; set; }


    }
}
