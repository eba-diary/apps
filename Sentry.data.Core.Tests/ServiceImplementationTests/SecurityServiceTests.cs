using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SecurityServiceTests : BaseCoreUnitTest
    {
        [TestInitialize]
        public void  MyTestInitialize()
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
        }

        /// <summary>
        /// a non owner is accessing a public dataser who also has no obsidian permissions.
        /// </summary>
        [TestMethod]
        public void Security_Public_NotOwner_NoModify()
        {
            //ARRAGE
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
        }

        /// <summary>
        /// pass in a null securable item to the security service should act like a public securable.
        /// </summary>
        [TestMethod]
        public void Security_With_Null_Securable()
        {
            //ARRAGE
            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
        }

        /// <summary>
        /// Even though the user has the Modify Dataset permission, they should not be able to edit because they are not the owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Edit_Dataset_User_With_Modify()
        {
            //ARRAGE
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanEditDataset);
        }
        
        
        /// <summary>
        /// user should be able to edit dataset with Modify permission and being the owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Edit_Dataset_As_Owner()
        {
            //ARRAGE
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanEditDataset);
        }

        /// <summary>
        /// Even though the user is the owner, they do not have the modify permission, they should not be able to edit dataset.
        /// </summary>
        [TestMethod]
        public void Security_Can_Edit_Dataset_As_Owner_Without_Modify_Permission()
        {
            //ARRAGE
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanManageSchema);
        }

        /// <summary>
        /// user should be able to manage schema with Modify permission and being the owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Manage_Schema_NonSecured_As_Owner_With_Modify_Permission()
        {
            //ARRAGE
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanManageSchema);
        }

        /// <summary>
        /// Even though the user is the owner, they do not have the modify permission, they should not be able to manage schema.
        /// </summary>
        [TestMethod]
        public void Security_Can_Manage_Schema_Secured_As_Owner_Without_Modify_Permission()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            //Establish user, ensure users it part of AD group and is not DSC admin
            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

        #endregion

        #region "CanPreviewDataset"
        /// <summary>
        /// Can Preview Dataset.  Assume ticket is not approved, user is in group, user is the owner.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_NotApproved_Owner_InGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), false);

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsTrue(us.CanPreviewDataset);
        }

        /// <summary>
        /// Can Preview Dataset.  Assume ticket is approved, user not in group, user is the owner.
        /// </summary>
        [TestMethod]
        public void Security_CanPreview_Approval_Owner_NotInGroup()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security, "MyAdGroupName");
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset(), true);

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket.AdGroupName)).Return(false).Repeat.Any();

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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(null).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(true).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.IsInGroup(ticket1.AdGroupName)).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanPreviewDataset);
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
            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
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
            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(true).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(null, user);

            //ASSERT
            Assert.IsTrue(us.CanCreateDataFlow);
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
            return  new SecurityPermission()
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

        #endregion
    }
}
