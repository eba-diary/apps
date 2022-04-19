﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhino.Mocks;
using Sentry.data.Core.GlobalEnums;
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

            ticket1.Permissions.Add(previewPermission1);
            ticket2.Permissions.Add(previewPermission2);
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

            ticket1.Permissions.Add(previewPermission1);
            ticket2.Permissions.Add(previewPermission2);
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
            ticket1.Permissions.Add(viewFullDatasetPermission1);
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
            ticket1.Permissions.Add(previewPermission1);
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
            parentTicket.Permissions.Add(parentPermission);
            parentSecurity.Tickets.Add(parentTicket);
            var parentSecurable = new Asset() { Security = parentSecurity };

            //Create security ticket for AD group granting CanManageSchema permission
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, string.Empty);
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, InheritParentPermissions(), true);
            ticket1.Permissions.Add(previewPermission1);
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

            ticket.Permissions.Add(previewPermission);
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

            ticket.Permissions.Add(previewPermission);
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

            ticket.Permissions.Add(previewPermission);
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

            ticket.Permissions.Add(previewPermission);
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

            ticket.Permissions.Add(previewPermission);
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

            ticket.Permissions.Add(previewPermission);
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

            ticket1.Permissions.Add(previewPermission1);
            ticket2.Permissions.Add(previewPermission2);
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

            ticket1.Permissions.Add(previewPermission1);
            ticket2.Permissions.Add(previewPermission2);
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

            ticket1.Permissions.Add(previewPermission1);
            ticket1.Permissions.Add(previewPermission2);
            ticket1.Permissions.Add(previewPermission3);
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
            parentTicket.Permissions.Add(parentPermission);
            parentSecurity.Tickets.Add(parentTicket);
            var parentSecurable = new Asset() { Security = parentSecurity };

            //Create security ticket for AD group granting CanManageSchema permission
            Security security = BuildBaseSecurity();
            SecurityTicket ticket1 = BuildBaseTicket(security, string.Empty);
            SecurityPermission previewPermission1 = BuildBasePermission(ticket1, InheritParentPermissions(), approvalStatus);
            ticket1.Permissions.Add(previewPermission1);
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
        /// Can Preview Dataset.  no user available.
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
        /// Can Preview Dataset.  no user available.
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
        /// Can Preview Dataset.  no user available.
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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, null, ds);

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
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null);

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
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null);

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
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null);

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
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null);

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
            SecurityService.BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, null);

            // Assert
            Assert.IsTrue(us.CanViewData);
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
            security.Tickets.First().Permissions.First().IsEnabled = false;
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
            security.Tickets.First().Permissions.First().IsEnabled = false;
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
            security.Tickets.First().Permissions.First().IsEnabled = false;
            security.Tickets.First().Permissions.First().RemovedDate = DateTime.Now;
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
            var securityService = new SecurityService(null, null, null);

            // Act
            var actual = securityService.GetSecurablePermissions(securable.Object);

            // Assert
            Assert.AreEqual(2,actual.Count());
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
            var securityService = new SecurityService(null, null, null);

            // Act
            var actual = securityService.GetSecurablePermissions(securable.Object);

            // Assert
            Assert.AreEqual(4, actual.Count()); //2 from parent and 2 from itself
        }

        #endregion

        #region "Private helpers"
        private Security BuildBaseSecurity(string CreateById = null)
        {
            return new Security()
            {
                SecurityId = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                EnabledDate = DateTime.Now,
                CreatedById = CreateById,
                SecurableEntityName = GlobalConstants.SecurableEntityName.DATASET,
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
                Permissions = new List<SecurityPermission>()
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
