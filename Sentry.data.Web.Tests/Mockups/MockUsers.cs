using Rhino.Mocks;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests
{
    public static class MockUsers
    {

        public static IApplicationUser App_DataMgmt_Admin_User()
        {
            var admin = MockRepository.GenerateStub<IApplicationUser>();

            admin.Stub(x => x.EmailAddress).Return("user2@b.com");

            admin.Stub(x => x.CanApproveItems).Return(true);
            admin.Stub(x => x.CanDwnldNonSensitive).Return(true);
            admin.Stub(x => x.CanDwnldSenstive).Return(true);
            admin.Stub(x => x.CanEditDataset).Return(true);
            admin.Stub(x => x.CanManageAssetAlerts).Return(true);
            admin.Stub(x => x.CanManageConfigs).Return(true);

            admin.Stub(x => x.CanQueryTool).Return(true);
            admin.Stub(x => x.CanQueryToolPowerUser).Return(true);

            admin.Stub(x => x.CanUpload).Return(true);
            admin.Stub(x => x.CanUseApp).Return(true);
            admin.Stub(x => x.CanUserSwitch).Return(true);

            admin.Stub(x => x.CanViewDataAsset).Return(true);
            admin.Stub(x => x.CanViewDataset).Return(true);

            return admin;
        }

        public static IApplicationUser User_Who_Not_Authenticated()
        {
            var user = MockRepository.GenerateStub<IApplicationUser>();

            user.Stub(x => x.EmailAddress).Return("user2@b.com");

            user.Stub(x => x.CanApproveItems).Return(false);
            user.Stub(x => x.CanDwnldNonSensitive).Return(false);
            user.Stub(x => x.CanDwnldSenstive).Return(false);
            user.Stub(x => x.CanEditDataset).Return(false);
            user.Stub(x => x.CanManageAssetAlerts).Return(false);
            user.Stub(x => x.CanManageConfigs).Return(false);

            user.Stub(x => x.CanQueryTool).Return(false);
            user.Stub(x => x.CanQueryToolPowerUser).Return(false);

            user.Stub(x => x.CanUpload).Return(false);
            user.Stub(x => x.CanUseApp).Return(false);
            user.Stub(x => x.CanUserSwitch).Return(false);

            user.Stub(x => x.CanViewDataAsset).Return(false);
            user.Stub(x => x.CanViewDataset).Return(false);

            return user;
        }

        public static IApplicationUser App_DataMgmt_MgAlert(IApplicationUser user = null)
        {
            if (user == null)
            {
                user = MockRepository.GenerateStub<IApplicationUser>();

                user.Stub(x => x.EmailAddress).Return("user2@b.com");
            }

            user.Stub(x => x.CanManageAssetAlerts).Return(true);
            return user;
        }

        public static IApplicationUser App_DataMgmt_MngDS(IApplicationUser user = null)
        {
            if (user == null)
            {
                user = MockRepository.GenerateStub<IApplicationUser>();

                user.Stub(x => x.EmailAddress).Return("user2@b.com");
            }

            user.Stub(x => x.CanEditDataset).Return(true);
            user.Stub(x => x.CanManageConfigs).Return(true);

            return user;
        }

        public static IApplicationUser App_DataMgmt_Upld()
        {
            var user = MockRepository.GenerateStub<IApplicationUser>();

            user.Stub(x => x.AssociateId).Return("082698");
            user.Stub(x => x.DisplayName).Return("Quaschnick, Andrew");

            user.Stub(x => x.CanApproveItems).Return(false);
            user.Stub(x => x.CanDwnldNonSensitive).Return(true);
            user.Stub(x => x.CanDwnldSenstive).Return(false);
            user.Stub(x => x.CanEditDataset).Return(false);
            user.Stub(x => x.CanManageAssetAlerts).Return(false);
            user.Stub(x => x.CanManageConfigs).Return(false);

            user.Stub(x => x.CanQueryTool).Return(false);
            user.Stub(x => x.CanQueryToolPowerUser).Return(false);

            user.Stub(x => x.CanUpload).Return(true);
            user.Stub(x => x.CanUseApp).Return(true);
            user.Stub(x => x.CanUserSwitch).Return(false);

            user.Stub(x => x.CanViewDataAsset).Return(true);
            user.Stub(x => x.CanViewDataset).Return(true);

            return user;
        }

        public static IApplicationUser App_DataMgmt_User()
        {
            var user = MockRepository.GenerateStub<IApplicationUser>();

            user.Stub(x => x.AssociateId).Return("072984");
            user.Stub(x => x.DisplayName).Return("Quaschnick, Andrew");

            user.Stub(x => x.CanApproveItems).Return(false);
            user.Stub(x => x.CanDwnldNonSensitive).Return(true);
            user.Stub(x => x.CanDwnldSenstive).Return(false);
            user.Stub(x => x.CanEditDataset).Return(false);
            user.Stub(x => x.CanManageAssetAlerts).Return(false);
            user.Stub(x => x.CanManageConfigs).Return(false);

            user.Stub(x => x.CanQueryTool).Return(false);
            user.Stub(x => x.CanQueryToolPowerUser).Return(false);

            user.Stub(x => x.CanUpload).Return(false);
            user.Stub(x => x.CanUseApp).Return(true);
            user.Stub(x => x.CanUserSwitch).Return(false);

            user.Stub(x => x.CanViewDataAsset).Return(true);
            user.Stub(x => x.CanViewDataset).Return(true);

            return user;
        }
    }
}
