
namespace Sentry.data.Web
{
    public static class UserSecurityExtensions
    {

        public static UserSecurityModel ToModel(this Core.UserSecurity core)
        {
            return new UserSecurityModel()
            {
                CanPreviewDataset = core.CanPreviewDataset,
                CanViewFullDataset = core.CanViewFullDataset,
                CanQueryDataset = core.CanQueryDataset,
                CanUploadToDataset = core.CanUploadToDataset,
                CanEditDataset = core.CanEditDataset,
                CanCreateDataset = core.CanCreateDataset,
                CanEditReport = core.CanEditReport,
                CanCreateReport = core.CanCreateReport,
                CanEditDataSource = core.CanEditDataSource,
                CanCreateDataSource = core.CanCreateDataSource,
                CanUseDataSource = core.CanUseDataSource,
                ShowAdminControls = core.ShowAdminControls,
                CanManageSchema = core.CanManageSchema
            };
        }

    }
}