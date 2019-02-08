
namespace Sentry.data.Web
{
    public class UserSecurityModel
    {

        public UserSecurityModel() { }

        public bool CanPreviewDataset { get; set; }
        public bool CanViewFullDataset { get; set; }
        public bool CanQueryDataset { get; set; }
        public bool CanConnectToDataset { get; set; }
        public bool CanUploadToDataset { get; set; }


        public bool CanEditDataset { get; set; }
        public bool CanCreateDataset { get; set; }


        public bool CanEditReport { get; set; }
        public bool CanCreateReport { get; set; }
    }
}