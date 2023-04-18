using Moq;
using Sentry.data.Core;

namespace Sentry.data.Web.Tests
{
    public static class MockUsers
    {

        public static Mock<IApplicationUser> App_DataMgmt_Admin_User()
        {
            Mock<IApplicationUser> adminUser = new Mock<IApplicationUser>();
            adminUser.Setup(x => x.AssociateId).Returns("012345");
            adminUser.Setup(x => x.DisplayName).Returns("Nye, Bill");

            adminUser.Setup(x => x.CanModifyDataset).Returns(true);
            adminUser.Setup(x => x.CanManageAssetAlerts).Returns(true);
            adminUser.Setup(x => x.CanModifyDataset).Returns(true);

            adminUser.Setup(x => x.CanUseApp).Returns(true);
            adminUser.Setup(x => x.CanUserSwitch).Returns(true);

            adminUser.Setup(x => x.CanViewDataAsset).Returns(true);
            adminUser.Setup(x => x.CanViewDataset).Returns(true);

            return adminUser;
        }

        public static Mock<IApplicationUser> User_Who_Not_Authenticated()
        {
            Mock<IApplicationUser> user = new Mock<IApplicationUser>();

            user.Setup(x => x.AssociateId).Returns("012345");
            user.Setup(x => x.DisplayName).Returns("Nye, Bill");

            user.Setup(x => x.CanModifyDataset).Returns(false);
            user.Setup(x => x.CanManageAssetAlerts).Returns(false);
            user.Setup(x => x.CanModifyDataset).Returns(false);

            user.Setup(x => x.CanUseApp).Returns(false);
            user.Setup(x => x.CanUserSwitch).Returns(false);

            user.Setup(x => x.CanViewDataAsset).Returns(false);
            user.Setup(x => x.CanViewDataset).Returns(false);

            return user;
        }

        public static Mock<IApplicationUser> App_DataMgmt_MgAlert()
        {
            Mock<IApplicationUser> user = new Mock<IApplicationUser>();

            user.Setup(x => x.AssociateId).Returns("012345");
            user.Setup(x => x.DisplayName).Returns("Nye, Bill");

            user.Setup(x => x.CanModifyDataset).Returns(false);
            user.Setup(x => x.CanModifyDataset).Returns(false);

            user.Setup(x => x.CanUseApp).Returns(true);
            user.Setup(x => x.CanUserSwitch).Returns(false);

            user.Setup(x => x.CanViewDataAsset).Returns(true);
            user.Setup(x => x.CanViewDataset).Returns(true);

            user.Setup(x => x.CanManageAssetAlerts).Returns(true);
            return user;
        }

        public static Mock<IApplicationUser> App_DataMgmt_MngDS()
        {
            Mock<IApplicationUser> user = new Mock<IApplicationUser>();

            user.Setup(x => x.AssociateId).Returns("012345");
            user.Setup(x => x.DisplayName).Returns("Nye, Bill");


            user.Setup(x => x.CanUseApp).Returns(true);
            user.Setup(x => x.CanUserSwitch).Returns(false);

            user.Setup(x => x.CanViewDataAsset).Returns(true);
            user.Setup(x => x.CanViewDataset).Returns(true);

            user.Setup(x => x.CanManageAssetAlerts).Returns(true);

            user.Setup(x => x.CanModifyDataset).Returns(true);
            user.Setup(x => x.CanModifyDataset).Returns(true);

            return user;
        }

        public static Mock<IApplicationUser> App_DataMgmt_Upld()
        {
            Mock<IApplicationUser> user = new Mock<IApplicationUser>();

            user.Setup(x => x.AssociateId).Returns("012345");
            user.Setup(x => x.DisplayName).Returns("Nye, Bill");

            user.Setup(x => x.CanModifyDataset).Returns(false);
            user.Setup(x => x.CanManageAssetAlerts).Returns(false);
            user.Setup(x => x.CanModifyDataset).Returns(false);

            user.Setup(x => x.CanUseApp).Returns(true);
            user.Setup(x => x.CanUserSwitch).Returns(false);

            user.Setup(x => x.CanViewDataAsset).Returns(true);
            user.Setup(x => x.CanViewDataset).Returns(true);

            return user;
        }

        public static Mock<IApplicationUser> App_DataMgmt_User()
        {
            Mock<IApplicationUser> user = new Mock<IApplicationUser>();

            user.Setup(x => x.AssociateId).Returns("012345");
            user.Setup(x => x.DisplayName).Returns("Nye, Bill");

            user.Setup(x => x.CanModifyDataset).Returns(false);
            user.Setup(x => x.CanManageAssetAlerts).Returns(false);
            user.Setup(x => x.CanModifyDataset).Returns(false);

            user.Setup(x => x.CanUseApp).Returns(true);
            user.Setup(x => x.CanUserSwitch).Returns(false);

            user.Setup(x => x.CanViewDataAsset).Returns(true);
            user.Setup(x => x.CanViewDataset).Returns(true);

            return user;
        }
    }
}
