using System.Web.Mvc;
using System.Web.SessionState;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Diagnostics;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class LayoutController : BaseController
    {
        private IDataAssetProvider _dataAssetProvider;
        private List<DataAsset> das;

        public LayoutController(IDataAssetProvider dap)
        {
            _dataAssetProvider = dap;
            das = new List<DataAsset>(_dataAssetProvider.GetDataAssets());
        }

        [ChildActionOnly()]
        public ActionResult GetHeader(/*bool hasMenu*/)
        {
            HeaderModel headerModel = new HeaderModel();
            headerModel.CanUserSwitch = (SharedContext.CurrentUser.CanUserSwitch && Configuration.Config.GetHostSetting("ShowUserChoice").ToLower() == "true");
            headerModel.CurrentUserName = SharedContext.CurrentUser.DisplayName;
            headerModel.CanUseApp = SharedContext.CurrentUser.CanUseApp;
            headerModel.CanManageConfigs = SharedContext.CurrentUser.CanManageConfigs;
            headerModel.CanManageAssetAlerts = SharedContext.CurrentUser.CanManageAssetAlerts;
            headerModel.CanViewDataset = SharedContext.CurrentUser.CanViewDataset;
            headerModel.CanViewDataAsset = SharedContext.CurrentUser.CanViewDataAsset;
            headerModel.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            headerModel.CanUpload = SharedContext.CurrentUser.CanUpload;

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
            
            //headerModel.HasMenu = hasMenu;
            ViewBag.DataAssets = das;

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
