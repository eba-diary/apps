using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhino.Mocks;
using Sentry.data.Core.DTO.Security;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SecurityServiceTests : BaseCoreUnitTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            TestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }


        #region "CanEditDataset"

        [TestMethod]
        public void Security_Secured_NotOwner_Admin()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            SecurityTicket ticket2 = BuildBaseTicket(security, "MyAdGroupName2");
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, CanPreviewDataset(), false);
            SecurityPermission previewPermission2 = BuildBasePermission(ticket2, CanPreviewDataset(), false);

            ticket1.AddedPermissions.Add(previewPermission1);
            ticket2.AddedPermissions.Add(previewPermission2);
            security.Tickets.Add(ticket1);
            security.Tickets.Add(ticket2);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(false).Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket2.AdGroupName)).Return(false).Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsTrue(us.CanQueryDataset);
            Assert.IsTrue(us.CanUploadToDataset);
            Assert.IsTrue(us.CanCreateDataset);
            Assert.IsTrue(us.CanCreateReport);
            Assert.IsTrue(us.CanEditDataset);
            Assert.IsTrue(us.CanEditReport);
            Assert.IsTrue(us.CanManageSchema);
            Assert.IsTrue(us.CanViewData);
            Assert.IsTrue(us.CanDeleteDatasetFile);
        }

        [TestMethod]
        public void Security_Secured_NotOwner_Admin_ExplicitPermissions()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            SecurityTicket ticket2 = BuildBaseTicket(security, "MyAdGroupName2");
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, CanPreviewDataset(), false);
            SecurityPermission previewPermission2 = BuildBasePermission(ticket2, CanPreviewDataset(), false);

            ticket1.AddedPermissions.Add(previewPermission1);
            ticket2.AddedPermissions.Add(previewPermission2);
            security.Tickets.Add(ticket1);
            security.Tickets.Add(ticket2);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();
            securable.Stub(x => x.AdminDataPermissionsAreExplicit).Return(true).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(false).Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket2.AdGroupName)).Return(false).Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();
            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsTrue(us.CanQueryDataset);
            Assert.IsTrue(us.CanUploadToDataset);
            Assert.IsTrue(us.CanCreateDataset);
            Assert.IsTrue(us.CanCreateReport);
            Assert.IsTrue(us.CanEditDataset);
            Assert.IsTrue(us.CanEditReport);
            Assert.IsTrue(us.CanManageSchema);
            Assert.IsFalse(us.CanViewData);
            Assert.IsTrue(us.CanDeleteDatasetFile);
        }

        [TestMethod]
        public void Security_AdminDataPermissionAreExplicit_Prevents_Implicit_Admin_Access()
        {
            //Arrange
            Security security = BuildBaseSecurity();

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.AdminDataPermissionsAreExplicit).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("072984").Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsFalse(us.CanViewData);
        }

        [TestMethod]
        public void Security_AdminDataPermissionAreExplicit_Admin_Access_When_Requested()
        {
            //Arrange
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            SecurityPermission viewFullDatasetPermission1 = BuildBasePermission(ticket1, CanViewFullDataset(), true);
            ticket1.AddedPermissions.Add(viewFullDatasetPermission1);
            security.Tickets.Add(ticket1);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.AdminDataPermissionsAreExplicit).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("072984").Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsTrue(us.CanViewData);
        }

        [TestMethod]
        public void Security_AdminDataPermissionAreExplicit_Allows_Implicit_Admin_Access_When_false()
        {
            //Arrange
            Security security = BuildBaseSecurity();

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.AdminDataPermissionsAreExplicit).Return(false).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("072984").Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsTrue(us.CanViewData);
        }

        /// <summary>
        /// a non owner is accessing a public dataset who also has no obsidian permissions.
        /// </summary>
        [TestMethod]
        public void Security_Public_NotOwner_NoModify()
        {
            //ARRAGE
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("078193").Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsTrue(us.CanQueryDataset);
            Assert.IsFalse(us.CanUploadToDataset);
            Assert.IsFalse(us.CanCreateDataset);
            Assert.IsFalse(us.CanCreateReport);
            Assert.IsFalse(us.CanEditDataset);
            Assert.IsFalse(us.CanEditReport);
            Assert.IsFalse(us.CanManageSchema);
            Assert.IsTrue(us.CanViewData);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }

        /// <summary>
        /// pass in a null securable item to the security service should act like a public securable.
        /// </summary>
        [TestMethod]
        public void Security_With_Null_Securable()
        {
            //ARRAGE
            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("078193").Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
            Assert.IsTrue(us.CanViewFullDataset);
            Assert.IsTrue(us.CanQueryDataset);
            Assert.IsFalse(us.CanUploadToDataset);
            Assert.IsFalse(us.CanCreateDataset);
            Assert.IsFalse(us.CanCreateReport);
            Assert.IsFalse(us.CanEditDataset);
            Assert.IsFalse(us.CanEditReport);
            Assert.IsFalse(us.CanManageSchema);
            Assert.IsTrue(us.CanViewData);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }

        /// <summary>
        /// Even though the user has the Modify Dataset permission, they should not be able to edit because they are not the owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Edit_Dataset_User_With_Modify()
        {
            //ARRAGE
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanEditDataset);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }

        /// <summary>
        /// Even though the user is the owner, they do not have the modify permission, they should not be able to edit dataset.
        /// </summary>
        [TestMethod]
        public void Security_Can_Edit_Dataset_As_Owner_Without_Modify_Permission()
        {
            //ARRAGE
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanEditDataset);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }

        #endregion

        #region CanManageSchema
        /// <summary>
        /// Even though the user has the Modify Dataset permission, they should not be able to manage schema because did not request the permission and are not owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Manage_Schema_NonSecured_User_With_Modify()
        {
            //ARRAGE
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanManageSchema);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }
        /// <summary>
        /// Even though the user has the Modify Dataset permission, they should not be able to manage schema because did not request the permission and are not owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Manage_Schema_Secured_User_With_Modify()
        {
            //ARRAGE
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanManageSchema);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }

        /// <summary>
        /// Even though the user is the owner, they do not have the modify permission, they should not be able to manage schema.
        /// </summary>
        [TestMethod]
        public void Security_Can_Manage_Schema_Secured_As_Owner_Without_Modify_Permission()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanManageSchema);
            Assert.IsFalse(us.CanDeleteDatasetFile);
        }

        [TestMethod]
        public void Security_Can_Manage_Schema_NonSecured_As_ServiceAccount_With_CanManageSchema_Permission()
        {
            //ARRAGE
            //Security security = BuildBaseSecurity();
            //ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            //securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            //securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            //securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            //securable.Stub(x => x.Security).Return(security).Repeat.Any();

            //IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            //user.Stub(x => x.AssociateId).Return("BT_ICCM_I_QUAL_V1").Repeat.Any();
            //user.Stub(x => x.Can)

            //Create security ticket for AD group granting CanManageSchema permission
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyServiceAccountGroup");
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, CanManageSchema(), true);
            ticket1.AddedPermissions.Add(previewPermission1);
            security.Tickets.Add(ticket1);

            //mock out securable object and attach security object established above
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            //Establish user, ensure users it part of AD group and is not DSC admin
            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            //User is part of AD group
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(true).Repeat.Any();
            //User is not DSC Admin
            user.Stub(x => x.IsAdmin).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);


            //ASSERT
            Assert.IsTrue(us.CanManageSchema);
            Assert.IsTrue(us.CanDeleteDatasetFile);

        }

        /// <summary>
        /// Tests that the CanManageSchema permissions is inherited from the Parent Asset's permissions
        /// </summary>
        [TestMethod]
        public void GetUserSecurity_CanManageSchema_InheritedFromParent()
        {
            //ARRAGE

            //Create security ticket for parent asset securable
            var parentSecurity = BuildBaseSecurity();
            var parentTicket = BuildBaseTicket(parentSecurity, "MyServiceAccountGroup");
            var parentPermission = BuildBasePermission(parentTicket, CanManageSchema(), true);
            parentTicket.AddedPermissions.Add(parentPermission);
            parentSecurity.Tickets.Add(parentTicket);
            var parentSecurable = new Asset() { Security = parentSecurity };

            //Create security ticket for AD group granting CanManageSchema permission
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, string.Empty);
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, InheritParentPermissions(), true);
            ticket1.AddedPermissions.Add(previewPermission1);
            security.Tickets.Add(ticket1);

            //mock out securable object and attach security object established above
            var securable = new Mock<ISecurable>();
            securable.SetupGet(x => x.IsSecured).Returns(false);
            securable.SetupGet(x => x.PrimaryContactId).Returns("123456");
            securable.SetupGet(x => x.Security).Returns(security);
            securable.SetupGet(x => x.Parent).Returns(parentSecurable);

            //Establish user, ensure users it part of AD group and is not DSC admin
            var user = new Mock<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("999999");
            user.Setup(x => x.IsInGroup(parentTicket.AdGroupName)).Returns(true);
            user.SetupGet(x => x.IsAdmin).Returns(false);

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable.Object, user.Object);

            //ASSERT
            Assert.IsTrue(us.CanManageSchema);
            Assert.IsTrue(us.CanDeleteDatasetFile);
        }

        #endregion

        #region "CanPreviewDataset"
        /// <summary>
        /// Can Preview Dataset.  Assume ticket is not approved, user is in group, user is not the owner.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_NotApproved_NotOwner_InGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), false);

            ticket.AddedPermissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  Assume ticket is approved, user is in group, user is not the owner.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Approval_NotOwner_InGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), true);

            ticket.AddedPermissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  Assume ticket is approved, user is in group, user is the owner.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Approval_Owner_InGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), true);

            ticket.AddedPermissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  Assume ticket is approved, user not in group, user is not the owner.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Approval_NotOwner_NotInGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), true);

            ticket.AddedPermissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  non secured dataset.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_NotSecured()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), true);

            ticket.AddedPermissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  dataset with no security on it.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_NullSecurity()
        {
            //ARRAGE
            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(null).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  dataset with no security on it.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_NullSecurable()
        {
            //ARRAGE
            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  no user available.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Secured_NullUser()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), true);

            ticket.AddedPermissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.Security).Return(null).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, null);

            //ASSERT
            Assert.IsFalse(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  two different tickets, one approved, one not for the same permission. not owner
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Secured_MultiAdGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            SecurityTicket ticket2 = BuildBaseTicket(security, "MyAdGroupName2");
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, CanPreviewDataset(), true);
            SecurityPermission previewPermission2 = BuildBasePermission(ticket2, CanPreviewDataset(), false);

            ticket1.AddedPermissions.Add(previewPermission1);
            ticket2.AddedPermissions.Add(previewPermission2);
            security.Tickets.Add(ticket1);
            security.Tickets.Add(ticket2);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(true).Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket2.AdGroupName)).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  two different tickets, one approved, one not for the same permission. user is only in group on not approved ticket
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Secured_MultiAdGroup_notApproved()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            SecurityTicket ticket2 = BuildBaseTicket(security, "MyAdGroupName2");
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, CanPreviewDataset(), false);
            SecurityPermission previewPermission2 = BuildBasePermission(ticket2, CanPreviewDataset(), true);

            ticket1.AddedPermissions.Add(previewPermission1);
            ticket2.AddedPermissions.Add(previewPermission2);
            security.Tickets.Add(ticket1);
            security.Tickets.Add(ticket2);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(true).Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket2.AdGroupName)).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  all tickets approved for all permissions but canPreview 
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Secured_HasAllPremissionButCanPreview()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, CanViewFullDataset(), false);
            SecurityPermission previewPermission2 = BuildBasePermission(ticket1, CanQueryDataset(), true);
            SecurityPermission previewPermission3 = BuildBasePermission(ticket1, CanUploadToDataset(), true);

            ticket1.AddedPermissions.Add(previewPermission1);
            ticket1.AddedPermissions.Add(previewPermission2);
            ticket1.AddedPermissions.Add(previewPermission3);
            security.Tickets.Add(ticket1);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanPreviewDataset);
        }

        /// <summary>
        /// Tests that even though the parent asset has "CanPreviewDataset" permission applied to it,
        /// the user's ability to Preview the dataset depends on the approval of the 
        /// Dataset's "InheritParentPermission" permission
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void GetUserSecurity_CanPreviewSchema_InheritedFromParent(bool approvalStatus)
        {
            //ARRAGE

            //Create security ticket for parent asset securable
            var parentSecurity = BuildBaseSecurity();
            var parentTicket = BuildBaseTicket(parentSecurity, "MyServiceAccountGroup");
            var parentPermission = BuildBasePermission(parentTicket, CanPreviewDataset(), true);
            parentTicket.AddedPermissions.Add(parentPermission);
            parentSecurity.Tickets.Add(parentTicket);
            var parentSecurable = new Asset() { Security = parentSecurity };

            //Create security ticket for AD group granting CanManageSchema permission
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, string.Empty);
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, InheritParentPermissions(), approvalStatus);
            ticket1.AddedPermissions.Add(previewPermission1);
            security.Tickets.Add(ticket1);

            //mock out securable object and attach security object established above
            var securable = new Mock<ISecurable>();
            securable.SetupGet(x => x.IsSecured).Returns(true);
            securable.SetupGet(x => x.PrimaryContactId).Returns("123456");
            securable.SetupGet(x => x.Security).Returns(security);
            securable.SetupGet(x => x.Parent).Returns(parentSecurable);

            //Establish user, ensure users it part of AD group and is not DSC admin
            var user = new Mock<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("999999");
            user.Setup(x => x.IsInGroup(parentTicket.AdGroupName)).Returns(true);
            user.SetupGet(x => x.IsAdmin).Returns(false);

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable.Object, user.Object);

            //ASSERT
            Assert.AreEqual(approvalStatus, us.CanPreviewDataset);
        }

        #endregion

        #region CanCreateDataflow
        /// <summary>
        /// Can create dataflow, user is Admin.
        /// </summary>
        [TestMethod]
        public void Security_CanCreateDataflow_NullSecurable_Admin()
        {
            //ARRAGE
            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsTrue(us.CanCreateDataFlow);
        }

        /// <summary>
        /// Non Admin, with no permissions, cannot create dataflow
        /// </summary>
        [TestMethod]
        public void Security_CanCreateDataflow_NullSecurable_NonAdmin_NoPermissions()
        {
            //ARRAGE
            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsFalse(us.CanCreateDataFlow);
        }

        /// <summary>
        /// Non Admin, with Modify permissions, can create dataflow
        /// </summary>
        [TestMethod]
        public void Security_CanCreateDataflow_NullSecurable_NonAdmin_With_Modify_Permissions()
        {
            //ARRAGE
            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsTrue(us.CanCreateDataFlow);
        }
        #endregion

        #region CanEditDataflow
        /// <summary>
        /// Admin User, can edit dataflow
        /// </summary>
        [TestMethod]
        public void Security_CanModifyDataflow_NullSecurable_Admin()
        {
            //ARRAGE
            Moq.MockRepository mr = new Moq.MockRepository(MockBehavior.Strict);

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsTrue(us.CanModifyDataflow);
        }

        /// <summary>
        /// Admin User, can edit dataflow
        /// </summary>
        [TestMethod]
        public void Security_CanModifyDataflow_NullSecurable_User()
        {
            //ARRAGE
            Moq.MockRepository mr = new Moq.MockRepository(MockBehavior.Strict);

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsAdmin).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsFalse(us.CanModifyDataflow);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Security_CanModifyDataflow_Securable_Admin()
        {
            //ARRAGE
            Moq.MockRepository mr = new Moq.MockRepository(MockBehavior.Loose);
            Security security = BuildBaseSecurity(securableEntityName: SecurableEntityName.DATAFLOW);
            Mock<ISecurable> securable = mr.Create<ISecurable>();
            securable.Setup(x => x.IsSecured).Returns(true);
            securable.Setup(x => x.Security).Returns(security);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("999999");
            user.Setup(x => x.IsAdmin).Returns(true);

            var ss = _container.GetInstance<ISecurityService>();

            //ACT
            UserSecurity us = ss.GetUserSecurity(securable.Object, user.Object);

            //ASSERT
            Assert.IsTrue(us.CanModifyDataflow);           
        }
        
        /// <summary>
        /// User with no permissions cannot modify dataflow
        /// </summary>
        [TestMethod]
        public void Security_CanModifyDataflow_Securable_User_With_No_Permissions()
        {
            //ARRAGE
            Moq.MockRepository mr = new Moq.MockRepository(MockBehavior.Loose);
            Mock<ISecurable> securable = mr.Create<ISecurable>();
            securable.Setup(x => x.IsSecured).Returns(false);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("999999");
            user.Setup(x => x.IsAdmin).Returns(false);

            var ss = _container.GetInstance<ISecurityService>();

            //ACT
            UserSecurity us = ss.GetUserSecurity(securable.Object, user.Object);

            //ASSERT
            Assert.IsFalse(us.CanModifyDataflow);
        }

        /// <summary>
        /// User with no permissions cannot modify dataflow
        /// </summary>
        [TestMethod]
        public void Security_CanModifyDataflow_Securable_User_With_Permission()
        {
            //ARRAGE
            Security security = BuildBaseSecurity(securableEntityName:SecurableEntityName.DATAFLOW);
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyServiceAccountGroup");
            SecurityPermission dataflowPermission = BuildBasePermission(ticket1, CanManageDataflow(), true);
            ticket1.AddedPermissions.Add(dataflowPermission);
            security.Tickets.Add(ticket1);


            //mock out securable and attach security object establihsed above
            Moq.MockRepository mr = new Moq.MockRepository(MockBehavior.Loose);
            Mock<ISecurable> securable = mr.Create<ISecurable>();
            securable.Setup(x => x.IsSecured).Returns(true);
            securable.Setup(x => x.Security).Returns(security);




            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("999999");
            user.Setup(x => x.IsAdmin).Returns(false);
            user.Setup(x => x.IsInGroup(ticket1.AdGroupName)).Returns(true);

            var ss = _container.GetInstance<ISecurityService>();

            //ACT
            UserSecurity us = ss.GetUserSecurity(securable.Object, user.Object);

            //ASSERT
            Assert.IsTrue(us.CanModifyDataflow);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Security_CanModifyDataflow_NonSecurable_NonAdmin()
        {
            //ARRAGE
            Moq.MockRepository mr = new Moq.MockRepository(MockBehavior.Loose);
            Mock<ISecurable> securable = mr.Create<ISecurable>();
            securable.Setup(x => x.IsSecured).Returns(false);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("999999");
            user.Setup(x => x.IsAdmin).Returns(false);

            var ss = _container.GetInstance<ISecurityService>();

            //ACT
            UserSecurity us = ss.GetUserSecurity(securable.Object, user.Object);

            //ASSERT
            Assert.IsFalse(us.CanModifyDataflow);
        }
        #endregion

        #region CanModifyNotifications
        /// <summary>
        /// This method tests that if there are only user-specific permission tickets, GetUserSecurity still functions properly
        /// </summary>
        [TestMethod]
        public void Security_UserOnlyPermissions_CanModifyNotifications()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, "MyAdGroupName1");
            ticket1.AdGroupName = null;
            ticket1.GrantPermissionToUserId = "999999";
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, new Permission() { PermissionCode = PermissionCodes.CAN_MODIFY_NOTIFICATIONS }, true);

            ticket1.AddedPermissions.Add(previewPermission1);
            security.Tickets.Add(ticket1);

            ISecurable securable = Rhino.Mocks.MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = Rhino.Mocks.MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.IsInGroup(null)).Throw(new NullReferenceException()); //The real service throws an exception if a null group name is passed in
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanModifyNotifications);
        }
        #endregion

        #region "MergeParentSecurity"
        /// <summary>
        /// Tests that when no parent security is provided to the MergeParentSecurity,
        /// it's just ignored
        /// </summary>
        [TestMethod]
        public void MergeParentSecurity_NoParent()
        {
            // Arrange
            UserSecurity us = new UserSecurity() { CanManageSchema = true };
            UserSecurity ps = null;

            // Act
            SecurityService.MergeParentSecurity(us, ps);

            // Assert
            Assert.IsTrue(us.CanManageSchema);
        }

        /// <summary>
        /// Tests that when parent security is provided to the MergeParentSecurity,
        /// it's permissions are merged into the user security
        /// </summary>
        [TestMethod]
        public void MergeParentSecurity_WithParent()
        {
            // Arrange
            UserSecurity us = new UserSecurity() { CanManageSchema = false };
            UserSecurity ps = new UserSecurity() { CanManageSchema = true };

            // Act
            SecurityService.MergeParentSecurity(us, ps);

            // Assert
            Assert.IsTrue(us.CanManageSchema);
        }
        #endregion

        #region "BuildOutUserSecurityForSecuredEntity"
        ///// <summary>
        ///// Tests that the "Owner" of a dataflow can manage it, 
        ///// even that permission hasn't been explicitely granted
        ///// </summary>
        //[TestMethod]
        //public void BuildOutUserSecurityForSecuredEntity_CanModifyDataflow_Owner()
        //{
        //    // Arrange
        //    var IsAdmin = false;
        //    var IsOwner = true;
        //    var userPermissions = new List<string>();
        //    var us = new UserSecurity();
        //    var df = new DataFlow();

        //    // Act
        //    SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, df);

        //    // Assert
        //    Assert.IsTrue(us.CanModifyDataflow);
        //}

        ///// <summary>
        ///// Tests that an Admin can manage dataflow, 
        ///// even that permission hasn't been explicitely granted
        ///// </summary>
        //[TestMethod]
        //public void BuildOutUserSecurityForSecuredEntity_CanModifyDataflow_Admin()
        //{
        //    // Arrange
        //    var IsAdmin = true;
        //    var IsOwner = false;
        //    var userPermissions = (new[] { PermissionCodes.CAN_PREVIEW_DATASET, PermissionCodes.CAN_VIEW_FULL_DATASET }).ToList();
        //    var us = new UserSecurity();
        //    var df = new DataFlow();

        //    // Act
        //    SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, df);

        //    // Assert
        //    Assert.IsTrue(us.CanModifyDataflow);
        //}

        /// <summary>
        /// Tests that the "Owner" of a dataset can manage its schema, 
        /// even that permission hasn't been explicitely granted
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_ManageSchema_Owner()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = true;
            var userPermissions = (new[] { PermissionCodes.CAN_PREVIEW_DATASET, PermissionCodes.CAN_VIEW_FULL_DATASET }).ToList();
            var us = new UserSecurity();
            var ds = new Dataset() { DataClassification = DataClassificationType.InternalUseOnly };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanManageSchema);
        }

        /// <summary>
        /// Tests that DSC Admins have access to view the full dataset
        /// of datasets within HR category
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_ViewFullDataset_Admin()
        {
            // Arrange
            var IsAdmin = true;
            var IsOwner = false;
            var userPermissions = (new[] { PermissionCodes.CAN_PREVIEW_DATASET }).ToList();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources" } }
            };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanViewFullDataset);
        }

        /// <summary>
        /// Tests that non-owner, non-admins get access through their explicitely granted permissions
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanModifyNotifications_User()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = (new[] { PermissionCodes.CAN_MODIFY_NOTIFICATIONS }).ToList();
            var us = new UserSecurity();
            var ds = new Dataset() { DataClassification = DataClassificationType.InternalUseOnly };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanModifyNotifications);
        }

        /// <summary>
        /// Tests that Admin can preview dataset, in hr category, with out permission request (implicit)
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanPreviewData_Is_True_For_Admin_When_IsHrData()
        {
            //Arrange
            var IsAdmin = true;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DataClassification = DataClassificationType.HighlySensitive,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET
            };
            ds.DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources" } };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Admin does not have permission to view data for dataset within Human Resource category,
        /// without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanViewData_False_For_Admin_When_IsHr()
        {
            //Arrange
            var IsAdmin = true;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DataClassification = DataClassificationType.HighlySensitive,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET
            };
            ds.DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources" } };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsFalse(us.CanViewData);
        }

        /// <summary>
        /// Admin does not have permission to view data for dataset within Human Resource category,
        /// without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanViewData_True_For_Admin_When_IsHrData_Is_False()
        {
            //Arrange
            var IsAdmin = true;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DataClassification = DataClassificationType.HighlySensitive,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET
            };
            ds.DatasetCategories = new List<Category>() { new Category() { Name = "Claim" } };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanViewData);
        }

        /// <summary>
        /// Owner has permission to view data for dataset within Human Resource category
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanViewData_Is_True_For_Owner_When_IsHrData_Is_True()
        {
            //Arrange
            var IsAdmin = false;
            var IsOwner = true;
            var userPermissions = new List<string>();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DataClassification = DataClassificationType.HighlySensitive,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET
            };
            ds.DatasetCategories = new List<Category>() { new Category() { Name = "Claim" } };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanViewData);
        }

        /// <summary>
        /// User does not have view data for dataset within Human Resources category without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanViewData_False_For_User_When_IsHrData_Is_True()
        {
            //Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DataClassification = DataClassificationType.HighlySensitive,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET
            };
            ds.DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources" } };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsFalse(us.CanViewData);
        }

        /// <summary>
        /// User can view data for dataset within Human Resources category with approved permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForSecuredEntity_CanViewData_True_For_User_When_IsHrData_Is_True()
        {
            //Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = (new[] { PermissionCodes.CAN_VIEW_FULL_DATASET }).ToList();
            var us = new UserSecurity();
            var ds = new Dataset()
            {
                DataClassification = DataClassificationType.HighlySensitive,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET
            };
            ds.DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources" } };

            // Act
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanViewData);
        }

        #endregion

        #region "BuildOutUserSecurityForUnsecuredEntity"
        /// <summary>
        /// Tests that a non-owner, non-admin can never upload to a non-secured dataset
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanUploadToDataset_User()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = (new[] { PermissionCodes.CAN_UPLOAD_TO_DATASET }).ToList();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsFalse(us.CanUploadToDataset);
        }

        /// <summary>
        /// Tests that a non-owner, non-admin can manage schema of a dataset if granted permissions explicitely
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanManageSchema_User()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = (new[] { PermissionCodes.CAN_MANAGE_SCHEMA }).ToList();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanManageSchema);
        }

        /// <summary>
        /// Tests that an owner of an unsecured dataset can manage its schema
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanManageSchema_Owner()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = true;
            var userPermissions = (new[] { PermissionCodes.CAN_PREVIEW_DATASET }).ToList();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanManageSchema);
        }

        /// <summary>
        /// Tests that an non-owner of an unsecured dataset can download data files without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanDownloadDataFile_User()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanViewData);
        }

        /// <summary>
        /// Tests that an owner of an unsecured dataset can download data files without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanDownloadDataFile_Owner()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = true;
            var userPermissions = new List<string>();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanViewData);
        }

        /// <summary>
        /// Tests that an owner of an unsecured dataflow can edit dataflow without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanModifyDataflow_Owner()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = true;
            var userPermissions = new List<string>();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanModifyDataflow);
        }

        /// <summary>
        /// Tests that an Admin can edit an unsecured dataflow without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanModifyDataflow_Admin()
        {
            // Arrange
            var IsAdmin = true;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, new MockDataFeatures());

            // Assert
            Assert.IsTrue(us.CanModifyDataflow);
        }

        /// <summary>
        /// Tests that a user cannot edit an unsecured dataflow without permission request
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityForUnsecuredEntity_CanModifyDataflow_user()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var userPermissions = new List<string>();
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, null);

            // Assert
            Assert.IsFalse(us.CanModifyDataflow);
        }
        #endregion

        #region "BuildOutUserSecurityFromObsidian"
        /// <summary>
        /// Tests that a user with the Obsidian "CanModifyDataset" permission can Create Data Flows
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityFromObsidian_CanCreateDataFlow_User()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = false;
            var user = new Mock<IApplicationUser>();
            user.SetupGet(x => x.CanModifyDataset).Returns(true);
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityFromObsidian(IsAdmin, IsOwner, user.Object, us);

            // Assert
            Assert.IsTrue(us.CanCreateDataFlow);
        }

        /// <summary>
        /// Tests that an Owner can edit a dataset
        /// </summary>
        [TestMethod]
        public void BuildOutUserSecurityFromObsidian_CanEditDataset_Owner()
        {
            // Arrange
            var IsAdmin = false;
            var IsOwner = true;
            var user = new Mock<IApplicationUser>();
            user.SetupGet(x => x.CanModifyDataset).Returns(true);
            var us = new UserSecurity();

            // Act
            SecurityService.BuildOutUserSecurityFromObsidian(IsAdmin, IsOwner, user.Object, us);

            // Assert
            Assert.IsTrue(us.CanEditDataset);
        }
        #endregion

        #region DoesSecurableInheritFromParent

        [TestMethod]
        public void DoesSecurableInheritFromParent_True()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.INHERIT_PARENT_PERMISSIONS });
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);
            securable.Setup(s => s.Parent).Returns(new Mock<ISecurable>().Object);

            // Act
            var actual = SecurityService.DoesSecurableInheritFromParent(securable.Object);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DoesSecurableInheritFromParent_False()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.CAN_MANAGE_SCHEMA });
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);

            // Act
            var actual = SecurityService.DoesSecurableInheritFromParent(securable.Object);

            // Assert
            Assert.IsFalse(actual);
        }

        #endregion

        #region GetSecurityTicketsForSecurable

        [TestMethod]
        public void GetSecurityTicketsForSecurable_IncludePending()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.CAN_MANAGE_SCHEMA, PermissionCodes.S3_ACCESS });
            security.Tickets.First().AddedPermissions.First().IsEnabled = false;
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);

            // Act
            var actual = SecurityService.GetSecurityTicketsForSecurable(securable.Object, true);

            // Assert
            Assert.AreEqual(2, actual.Count());
        }

        [TestMethod]
        public void GetSecurityTicketsForSecurable_NoPending()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.CAN_MANAGE_SCHEMA, PermissionCodes.S3_ACCESS });
            security.Tickets.First().AddedPermissions.First().IsEnabled = false;
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);

            // Act
            var actual = SecurityService.GetSecurityTicketsForSecurable(securable.Object, false);

            // Assert
            Assert.AreEqual(1, actual.Count());
        }

        [TestMethod]
        public void GetSecurityTicketsForSecurable_NoRemoved()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.CAN_MANAGE_SCHEMA, PermissionCodes.S3_ACCESS });
            security.Tickets.First().AddedPermissions.First().IsEnabled = false;
            security.Tickets.First().AddedPermissions.First().RemovedDate = DateTime.Now;
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);

            // Act
            var actual = SecurityService.GetSecurityTicketsForSecurable(securable.Object, true);

            // Assert
            Assert.AreEqual(1, actual.Count());
        }

        #endregion

        #region GetSecurablePermissions

        [TestMethod]
        public void GetSecurablePermissions_ParentButNoInheritance()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.CAN_MANAGE_SCHEMA, PermissionCodes.S3_ACCESS });
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);
            var parentSecurable = new Mock<ISecurable>();
            parentSecurable.Setup(s => s.Security).Returns(security);
            securable.Setup(s => s.Parent).Returns(parentSecurable.Object);
            var securityService = new SecurityService(null, null, null, null, null, null, null, null, null);

            // Act
            var actual = securityService.GetSecurablePermissions(securable.Object);

            // Assert
            Assert.AreEqual(2, actual.Count());
        }

        [TestMethod]
        public void GetSecurablePermissions_Inheritance()
        {
            // Arrange
            var security = MockClasses.MockSecurity(new[] { PermissionCodes.CAN_MANAGE_SCHEMA, PermissionCodes.S3_ACCESS, PermissionCodes.INHERIT_PARENT_PERMISSIONS });
            var securable = new Mock<ISecurable>();
            securable.Setup(s => s.Security).Returns(security);
            var parentSecurable = new Mock<ISecurable>();
            parentSecurable.Setup(s => s.Security).Returns(security);
            securable.Setup(s => s.Parent).Returns(parentSecurable.Object);
            var securityService = new SecurityService(null, null, null, null, null, null, null, null, null);

            // Act
            var actual = securityService.GetSecurablePermissions(securable.Object);

            // Assert
            Assert.AreEqual(4, actual.Count()); //2 from parent and 2 from itself
        }

        #endregion

        #region ApproveTicket

        /// <summary>
        /// Tests that when a security ticket is approved, and none of the associated permissions
        /// are for a dataset, no Infrastructure Event is published.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ApproveTicket_NoDatasetPermissions()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var inevService = new Mock<IInevService>();
            var ticket = new SecurityTicket()
            {
                AddedPermissions = new List<SecurityPermission>() {
                    new SecurityPermission() {
                        Permission = new Permission() { SecurableObject = SecurableEntityName.DATA_ASSET } } },
                RemovedPermissions = new List<SecurityPermission>(),
                ParentSecurity = new Security()
                {
                    SecurableEntityName = GlobalConstants.SecurableEntityName.DATASET
                }
            };
            var service = new SecurityService(context.Object, null, new MockDataFeatures(), inevService.Object, null, null, null, null, null);

            //Act
            await service.ApproveTicket(ticket, "");

            //Assert
            inevService.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that when a security ticket is approved, and the dataset associated with the security ticket
        /// can't be found, no Infrastructure Event is published.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ApproveTicket_DatasetPermissions_NoDataset()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var inevService = new Mock<IInevService>();
            var ticket = new SecurityTicket()
            {
                AddedPermissions = new List<SecurityPermission>() {
                    new SecurityPermission() {
                        Permission = new Permission() { SecurableObject = SecurableEntityName.DATASET } } },
                RemovedPermissions = new List<SecurityPermission>(),
                ParentSecurity = new Security()
                {
                    SecurableEntityName = GlobalConstants.SecurableEntityName.DATASET
                }
            };
            var service = new SecurityService(context.Object, null, new MockDataFeatures(), inevService.Object, null, null, null, null, null);

            //Act
            await Assert.ThrowsExceptionAsync<DatasetNotFoundException>(() => service.ApproveTicket(ticket, ""));

            //Assert
            inevService.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that when a security ticket is approved, if there are dataset permissions associated with the
        /// security ticket, and the dataset can be found, then the Infrastructure Event is published
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ApproveTicket_DatasetPermissions_WithDataset()
        {
            //Arrange
            var ticket = new SecurityTicket()
            {
                AddedPermissions = new List<SecurityPermission>() {
                    new SecurityPermission() {
                        Permission = new Permission() { SecurableObject = SecurableEntityName.DATASET } } },
                RemovedPermissions = new List<SecurityPermission>(),
                ParentSecurity = new Security()
                {
                    SecurableEntityName = GlobalConstants.SecurableEntityName.DATASET
                }
            };
            var dataset = new Dataset() { Security = new Security() { Tickets = new List<SecurityTicket>() { ticket } } };
            var context = new Mock<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns((new List<Dataset>() { dataset }).AsQueryable());
            var inevService = new Mock<IInevService>();
            var service = new SecurityService(context.Object, null, new MockDataFeatures(), inevService.Object, null, null, null, null, null);

            //Act
            await service.ApproveTicket(ticket, "");

            //Assert
            inevService.Verify(i => i.PublishDatasetPermissionsUpdated(dataset, ticket, It.IsAny<IList<SecurablePermission>>(), It.IsAny<IList<SecurablePermission>>()));
        }

        #endregion

        #region "GetDefaultSecurityGroupDtos"
        [TestMethod]
        public void GetDefaultSecurityGroupDtos_Test()
        {
            //Arrange
            var ds = new Dataset() { NamedEnvironmentType = NamedEnvironmentType.Prod, ShortName = nameof(Dataset.ShortName), Asset = new Asset() { SaidKeyCode = "ABCD" } };
            var securityService = new SecurityService(null, null, null, null, null, null, null, null, null);

            //Act
            var groupDtos = securityService.GetDefaultSecurityGroupDtos(ds);

            //Assert
            Assert.AreEqual(4, groupDtos.Count(g => g.SaidAssetCode == "ABCD")); //4 groups total for the asset
            Assert.AreEqual(2, groupDtos.Count(g => g.GroupType == DTO.Security.AdSecurityGroupType.Prdcr)); //2 producer groups
            Assert.AreEqual(2, groupDtos.Count(g => g.GroupType == DTO.Security.AdSecurityGroupType.Cnsmr)); //2 consumer groups
            Assert.AreEqual(2, groupDtos.Count(g => !g.IsAssetLevelGroup())); //2 dataset-level groups
            Assert.AreEqual(2, groupDtos.Count(g => g.IsAssetLevelGroup())); //2 asset-level groups
        }
        #endregion

        #region "CreateDefaultSecurityForDataset"

        /// <summary>
        /// Verify that if a Consumer permission is requested as part of <see cref="SecurityService.CreateDefaultSecurityForDataset_Internal(Dataset, List{AdSecurityGroupDto}, SecurityService.DefaultPermissions, IEnumerable{SecurityTicket}, IEnumerable{SecurityTicket})"/>
        /// that it is auto-approved.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CreateDefaultSecurityForDataset_Internal_Consumer_Test()
        {
            //Arrange
            var security = new Security() { SecurityId = Guid.NewGuid(), Tickets = new List<SecurityTicket>(), SecurableEntityName = SecurableEntityName.DATASET };
            var ds = new Dataset() { NamedEnvironmentType = NamedEnvironmentType.Prod, ShortName = nameof(Dataset.ShortName), Security = security, Asset = new Asset() { SaidKeyCode = "ABCD" } };
            var groups = new List<AdSecurityGroupDto>() { AdSecurityGroupDto.NewDatasetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Cnsmr, AdSecurityGroupEnvironmentType.NP) };
            var defaultPermissions = new SecurityService.DefaultPermissions(null, new List<Permission>() { new Permission() { PermissionCode = PermissionCodes.CAN_VIEW_FULL_DATASET, SecurableObject = SecurableEntityName.DATASET } }, null, null);
            var datasetTickets = new List<SecurityTicket>().AsEnumerable();
            var assetTickets = new List<SecurityTicket>().AsEnumerable();

            var obsidianService = new Mock<IObsidianService>();
            obsidianService.Setup(o => o.DoesGroupExist(It.IsAny<string>())).Returns(false);
            var adSecurityAdminProvider = new Mock<IAdSecurityAdminProvider>();
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Security).Returns(new List<Security>() { security }.AsQueryable());
            var securityService = new Mock<SecurityService>(context.Object, null, null, null, null, null, obsidianService.Object, adSecurityAdminProvider.Object, null) 
                { CallBase = true }; //call the real method for anything not explicitely .Setup()
            securityService.Setup(s => s.ApproveTicket(It.IsAny<SecurityTicket>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            //Act
            await securityService.Object.CreateDefaultSecurityForDataset_Internal(ds, groups, defaultPermissions, datasetTickets, assetTickets);

            //Assert
            adSecurityAdminProvider.Verify(a => a.CreateAdSecurityGroupAsync(groups[0]), Times.AtMost(2)); //verify the AD group attempted to be created
            securityService.Verify(s => s.BuildAddingPermissionTicket(It.IsAny<string>(), It.IsAny<AccessRequest>(), security), Times.AtMost(2)); //verify a ticket was built
            securityService.Verify(s => s.ApproveTicket(It.IsAny<SecurityTicket>(), It.IsAny<string>()), Times.AtMost(2)); //verify the ticket was approved
            context.Verify(c => c.SaveChanges(It.IsAny<bool>()), Times.Exactly(4));
        }

        /// <summary>
        /// Verify that if a Snowflake permission is requested as part of <see cref="SecurityService.CreateDefaultSecurityForDataset_Internal(Dataset, List{AdSecurityGroupDto}, SecurityService.DefaultPermissions, IEnumerable{SecurityTicket}, IEnumerable{SecurityTicket})"/>
        /// that it's NOT auto-approved.
        /// </summary>
        [TestMethod]
        public async Task CreateDefaultSecurityForDataset_Internal_Snowflake_Test()
        {
            //Arrange
            var security = new Security() { SecurityId = Guid.NewGuid(), Tickets = new List<SecurityTicket>(), SecurableEntityName = SecurableEntityName.DATASET };
            var ds = new Dataset() { NamedEnvironmentType = NamedEnvironmentType.Prod, ShortName = nameof(Dataset.ShortName), Security = security, Asset = new Asset() { SaidKeyCode = "ABCD" }, DataClassification = DataClassificationType.HighlySensitive };
            var groups = new List<AdSecurityGroupDto>() { AdSecurityGroupDto.NewDatasetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Cnsmr, AdSecurityGroupEnvironmentType.NP) };
            var defaultPermissions = new SecurityService.DefaultPermissions(null, null, new List<Permission>() { new Permission() { PermissionCode = PermissionCodes.SNOWFLAKE_ACCESS, SecurableObject = SecurableEntityName.DATASET } }, null);
            var datasetTickets = new List<SecurityTicket>().AsEnumerable();
            var assetTickets = new List<SecurityTicket>().AsEnumerable();

            var obsidianService = new Mock<IObsidianService>();
            obsidianService.Setup(o => o.DoesGroupExist(It.IsAny<string>())).Returns(false);
            var adSecurityAdminProvider = new Mock<IAdSecurityAdminProvider>();
            var inevService = new Mock<IInevService>();
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Security).Returns(new List<Security> { security }.AsQueryable());
            context.Setup(c => c.Datasets).Returns(new List<Dataset> { ds }.AsQueryable());
            var securityService = new Mock<SecurityService>(context.Object, null, new MockDataFeatures(), inevService.Object, null, null, obsidianService.Object, adSecurityAdminProvider.Object, null)
                { CallBase = true }; //call the real method for anything not explicitely .Setup()
            securityService.Setup(s => s.ApproveTicket(It.IsAny<SecurityTicket>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            //Act
            await securityService.Object.CreateDefaultSecurityForDataset_Internal(ds, groups, defaultPermissions, datasetTickets, assetTickets);

            //Assert
            adSecurityAdminProvider.Verify(a => a.CreateAdSecurityGroupAsync(groups[0]), Times.AtMost(1)); //verify the AD group attempted to be created
            securityService.Verify(s => s.BuildAddingPermissionTicket(It.IsAny<string>(), It.IsAny<AccessRequest>(), security), Times.AtMost(1)); //verify a ticket was built
            securityService.Verify(s => s.ApproveTicket(It.IsAny<SecurityTicket>(), It.IsAny<string>()), Times.Never); //verify the ticket was NOT approved
            context.Verify(c => c.SaveChanges(It.IsAny<bool>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task CreateDefaultSecurityForDataflow_Internal_Dataflow_test()
        {
            //Arrange
            var df_security = new Security() { SecurityId = Guid.NewGuid(), Tickets = new List<SecurityTicket>(), SecurableEntityName = SecurableEntityName.DATAFLOW };
            var df = new DataFlow() { NamedEnvironmentType = NamedEnvironmentType.Prod, Security = df_security};


            var ds_security = new Security() { SecurityId = Guid.NewGuid(), Tickets = new List<SecurityTicket>(), SecurableEntityName = SecurableEntityName.DATASET };
            var ds = new Dataset() { NamedEnvironmentType = NamedEnvironmentType.Prod, ShortName = nameof(Dataset.ShortName), Security = ds_security, Asset = new Asset() { SaidKeyCode = "ABCD" } };
            var groups = new List<AdSecurityGroupDto>() { AdSecurityGroupDto.NewDatasetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Prdcr, AdSecurityGroupEnvironmentType.NP) };
            var defaultPermissions = new SecurityService.DefaultPermissions(null, null, null, new List<Permission>() { new Permission() { PermissionCode = PermissionCodes.CAN_MANAGE_DATAFLOW, SecurableObject = SecurableEntityName.DATAFLOW } });
            var datasetTickets = new List<SecurityTicket>().AsEnumerable();
            var obsidianService = new Mock<IObsidianService>();
            obsidianService.Setup(o => o.DoesGroupExist(It.IsAny<string>())).Returns(true);
            var adSecurityAdminProvider = new Mock<IAdSecurityAdminProvider>();
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Security).Returns(new List<Security>() { df_security, ds_security }.AsQueryable());
            var securityService = new Mock<SecurityService>(context.Object, null, null, null, null, null, obsidianService.Object, adSecurityAdminProvider.Object, null)
            { CallBase = true }; //call the real method for anything not explicitely .Setup()
            securityService.Setup(s => s.ApproveTicket(It.IsAny<SecurityTicket>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            await securityService.Object.CreateDefaultSecurityForDataflow_Internal(df, groups, defaultPermissions, datasetTickets);

            securityService.Verify(s => s.BuildAddingPermissionTicket(It.IsAny<string>(), It.IsAny<AccessRequest>(), df_security), Times.AtMost(1)); //verify a ticket was built
            securityService.Verify(s => s.ApproveTicket(It.IsAny<SecurityTicket>(), It.IsAny<string>()), Times.AtMost(1)); //verify the ticket was approved
            context.Verify(c => c.SaveChanges(It.IsAny<bool>()), Times.Exactly(2));
        }

        #endregion

        #region "GetProducerPermissions / GetConsumerPermissions"

        [TestMethod]
        public void GetConsumerPermissions_Test()
        {
            //Arrange
            var permissions = new List<Permission>()
            {
                new Permission() { PermissionCode = PermissionCodes.CAN_VIEW_FULL_DATASET, SecurableObject = SecurableEntityName.DATASET },
                new Permission() { PermissionCode = PermissionCodes.CAN_UPLOAD_TO_DATASET, SecurableObject = SecurableEntityName.DATASET },
                new Permission() { PermissionCode = PermissionCodes.CAN_MODIFY_NOTIFICATIONS, SecurableObject = SecurableEntityName.DATA_ASSET }
            };
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Permission).Returns(permissions.AsQueryable());
            var securityService = new SecurityService(context.Object, null, null, null, null, null, null, null, null);

            //Act
            var actual = securityService.GetConsumerPermissions();

            //Assert
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(PermissionCodes.CAN_VIEW_FULL_DATASET, actual[0].PermissionCode);
        }

        [TestMethod]
        public void GetProducerPermissions_Test()
        {
            //Arrange
            var permissions = new List<Permission>()
            {
                new Permission() { PermissionCode = PermissionCodes.CAN_VIEW_FULL_DATASET, SecurableObject = SecurableEntityName.DATASET },
                new Permission() { PermissionCode = PermissionCodes.CAN_UPLOAD_TO_DATASET, SecurableObject = SecurableEntityName.DATASET },
                new Permission() { PermissionCode = PermissionCodes.CAN_MODIFY_NOTIFICATIONS, SecurableObject = SecurableEntityName.DATA_ASSET }
            };
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Permission).Returns(permissions.AsQueryable());
            var securityService = new SecurityService(context.Object, null, null, null, null, null, null, null, null);

            //Act
            var actual = securityService.GetProducerPermissions();

            //Assert
            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual.Any(a => a.PermissionCode == PermissionCodes.CAN_VIEW_FULL_DATASET));
            Assert.IsTrue(actual.Any(a => a.PermissionCode == PermissionCodes.CAN_UPLOAD_TO_DATASET));
        }

        [TestMethod]
        public void GetSnowflakePermissions_Test()
        {
            //Arrange
            var permissions = new List<Permission>()
            {
                new Permission() { PermissionCode = PermissionCodes.SNOWFLAKE_ACCESS, SecurableObject = SecurableEntityName.DATASET }
            };
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Permission).Returns(permissions.AsQueryable());
            var securityService = new SecurityService(context.Object, null, null, null, null, null, null, null, null);

            //Act
            var actual = securityService.GetSnowflakePermissions();

            //Assert
            Assert.AreEqual(1, actual.Count);
            Assert.IsTrue(actual.Any(a => a.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS));
        }

        [TestMethod]
        public void GetDataFlowPermission_Test()
        {
            //Arrange
            var permissions = new List<Permission>()
            {
                new Permission() { PermissionCode = PermissionCodes.CAN_MANAGE_DATAFLOW, SecurableObject = SecurableEntityName.DATAFLOW }
            };
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Permission).Returns(permissions.AsQueryable());
            var securityService = new SecurityService(context.Object, null, null, null, null, null, null, null, null);

            //Act
            var actual = securityService.GetDataflowPermissions();

            //Assert
            Assert.AreEqual(1, actual.Count);
            Assert.IsTrue(actual.Any(a => a.PermissionCode == PermissionCodes.CAN_MANAGE_DATAFLOW));
        }

        #endregion

        #region AdSecurityGroupDto

        [TestMethod]
        public void GetGroupName()
        {
            AdSecurityGroupDto adDto = AdSecurityGroupDto.NewDatasetGroup("DATA", "SHORTDSNAME", AdSecurityGroupType.Cnsmr, AdSecurityGroupEnvironmentType.P);

            string groupName = adDto.GetGroupName();

            Assert.AreEqual("DS_DATA_SHORTDSNAME_Cnsmr_D", groupName);
        }
        #endregion

        #region "Private helpers"


        private Security BuildBaseSecurity(string CreateById = null, string securableEntityName = SecurableEntityName.DATASET)
        {
            return new Security()
            {
                SecurityId = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                EnabledDate = DateTime.Now,
                CreatedById = CreateById,
                SecurableEntityName = securableEntityName,
                Tickets = new List<SecurityTicket>()
            };
        }

        private SecurityTicket BuildBaseTicket(Security security, string adGroupName)
        {
            return new SecurityTicket()
            {
                AdGroupName = adGroupName,
                IsAddingPermission = true,
                ParentSecurity = security,
                RequestedById = "078193",
                SecurityTicketId = Guid.NewGuid(),
                TicketStatus = GlobalConstants.HpsmTicketStatus.PENDING,
                TicketId = "C00123456",
                AddedPermissions = new List<SecurityPermission>()
            };
        }

        private SecurityPermission BuildBasePermission(SecurityTicket ticket, Permission permission, bool isEnabled)
        {
            return new SecurityPermission()
            {
                AddedDate = DateTime.Now,
                SecurityPermissionId = Guid.NewGuid(),
                AddedFromTicket = ticket,
                Permission = permission,
                IsEnabled = isEnabled
            };
        }

        private Permission CanPreviewDataset()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET,
                PermissionDescription = "Can Preview Dataset Description",
                PermissionName = "Can Preview Dataset Name",
                SecurableObject = GlobalConstants.SecurableEntityName.DATASET
            };
        }

        private Permission CanViewFullDataset()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET,
                PermissionDescription = "Can View Full Dataset Description",
                PermissionName = "Can View Full Dataset Name",
                SecurableObject = GlobalConstants.SecurableEntityName.DATASET
            };
        }

        private Permission CanQueryDataset()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.CAN_QUERY_DATASET,
                PermissionDescription = "Can Query Dataset Description",
                PermissionName = "Can Query Dataset Name",
                SecurableObject = GlobalConstants.SecurableEntityName.DATASET
            };
        }

        private Permission CanUploadToDataset()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.CAN_UPLOAD_TO_DATASET,
                PermissionDescription = "Can Upload Dataset Description",
                PermissionName = "Can Upload Dataset Name",
                SecurableObject = GlobalConstants.SecurableEntityName.DATASET
            };
        }

        private Permission CanManageSchema()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA,
                PermissionDescription = "Can Manage Schema Description",
                PermissionName = "Can Manage Schema Name",
                SecurableObject = GlobalConstants.SecurableEntityName.DATASET
            };
        }

        private Permission CanManageDataflow()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.CAN_MANAGE_DATAFLOW,
                PermissionDescription = "Can Manage DataFlow Description",
                PermissionName = "Can Manage Dataflow Name",
                SecurableObject = GlobalConstants.SecurableEntityName.DATAFLOW
            };
        }

        private Permission InheritParentPermissions()
        {
            return new Permission()
            {
                PermissionCode = GlobalConstants.PermissionCodes.INHERIT_PARENT_PERMISSIONS,
                PermissionDescription = "Inherit Parent Permissions",
                PermissionName = "Inherit Parent Permissions",
                SecurableObject = GlobalConstants.SecurableEntityName.DATASET
            };
        }

        #endregion
    }
}
