using System.Web.Mvc;
using System.Web.SessionState;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class LayoutController : BaseController
    {
        private readonly IDataAssetContext _dataAssetContext;
        private readonly IDatasetContext _datasetContext;

        public LayoutController(IDataAssetContext dataAssetContext, IDatasetContext dsContext)
        {
            _dataAssetContext = dataAssetContext;
            _datasetContext = dsContext;
        }

        [ChildActionOnly()]
        public ActionResult GetHeader(/*bool hasMenu*/)
        {
            HeaderModel headerModel = new HeaderModel();
            headerModel.ShowAdminControls = SharedContext.CurrentUser.CanUserSwitch;
            headerModel.CanUserSwitch = (SharedContext.CurrentUser.CanUserSwitch && Configuration.Config.GetHostSetting("ShowUserChoice").ToLower() == "true");
            headerModel.CurrentUserName = SharedContext.CurrentUser.DisplayName;
            headerModel.CanUseApp = SharedContext.CurrentUser.CanUseApp;
            headerModel.CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs;
            headerModel.CanManageAssetAlerts = SharedContext.CurrentUser.CanManageAssetAlerts;
            headerModel.CanViewDataset = SharedContext.CurrentUser.CanViewDataset;
            headerModel.CanViewDataAsset = SharedContext.CurrentUser.CanViewDataAsset;
            headerModel.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            headerModel.CanUpload = SharedContext.CurrentUser.CanUpload;
            headerModel.CanQueryTool = SharedContext.CurrentUser.CanQueryTool;
            headerModel.CanQueryToolPowerUser = SharedContext.CurrentUser.CanQueryToolPowerUser;
            headerModel.CanViewReports = SharedContext.CurrentUser.CanViewReports;
            headerModel.CanManageReports = SharedContext.CurrentUser.CanManageReports;
            //headerModel.AdminUser = SharedContext.CurrentUser.AdminUser;

            if (SharedContext.CurrentUser.GetType() == typeof(ImpersonatedApplicationUser))
            {
                headerModel.IsImpersonating = true;
                headerModel.RealUserName = SharedContext.CurrentRealUser.DisplayName;
            }
            else
            {
                headerModel.IsImpersonating = false;
            }
                        
            headerModel.EnvironmentName = Sentry.Configuration.Config.GetHostSetting("EnvironmentName");
            headerModel.AssociatePhotoUrl = "http://sentryphoto.sentry.com/associate/" + SharedContext.CurrentUser.AssociateId + "/height/25px";

            var das = new List<DataAsset>(_dataAssetContext.GetDataAssets());
            ViewBag.DataAssets = das.Select(x => new Models.AssetUIModel(x)).ToList();
            ViewBag.BusinessIntelligenceCategories = _datasetContext.Categories.ToList().Where(w => w.ObjectType == "RPT").OrderBy(o => o.Name).Select(x => new Models.BusinessIntelligenceUIModel(x)).ToList();

            return PartialView("_Header", headerModel);
        }

        private class AppVersion
        {
            private static readonly Version _applicationVersion = Assembly.GetExecutingAssembly().GetName().Version;

            public static Version ApplicationVersion
            {
                get { return _applicationVersion; }
            }
        }

        [ChildActionOnly()]
        public ActionResult GetFooter()
        {
            FooterModel footerModel = new FooterModel();
            
            // Replace the following if necessary to load the correct version number for your app
            footerModel.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return PartialView("_Footer", footerModel);
        }
    }
}
