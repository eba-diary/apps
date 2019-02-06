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

        /// <summary>
        /// a non owner is accessing a public dataser who also has no obsidian permissions.
        /// </summary>
        [TestMethod]
        public void Security_Public_NotOwner_NoModify()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security);
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset());

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

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
        }

        /// <summary>
        /// pass in a null securable item to the security service should act like a public securable.
        /// </summary>
        [TestMethod]
        public void Security_With_Null_Securable()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security);
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset());

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

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
        }

        /// <summary>
        /// Even though the user has the Modify Dataset permission, they should not be able to edit because they are not the owner.
        /// </summary>
        [TestMethod]
        public void Security_Can_Edit_Dataset()
        {
            //ARRAGE
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security);
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset());

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("123456").Repeat.Any();
            securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

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
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security);
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset());

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();
            securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

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
            Security security = BuildBaseSecurity();
            SecurityTicket ticket = BuildBaseTicket(security);
            SecurityPermission previewPermission = BuildBasePermission(ticket, CanPreviewDataset());

            ticket.Permissions.Add(previewPermission);
            security.Tickets.Add(ticket);

            ISecurable securable = MockRepository.GenerateMock<ISecurable>();
            securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
            securable.Stub(x => x.PrimaryOwnerId).Return("999999").Repeat.Any();
            securable.Stub(x => x.PrimaryContactId).Return("123456").Repeat.Any();
            securable.Stub(x => x.Security).Return(security).Repeat.Any();

            IApplicationUser user = MockRepository.GenerateMock<IApplicationUser>();
            user.Stub(x => x.AssociateId).Return("999999").Repeat.Any();
            user.Stub(x => x.CanModifyDataset).Return(false).Repeat.Any();

            //ACT
            var ss = _container.GetInstance<ISecurityService>();
            UserSecurity us = ss.GetUserSecurity(securable, user);

            //ASSERT
            Assert.IsFalse(us.CanEditDataset);
        }




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

        private SecurityTicket BuildBaseTicket(Security security)
        {
            return new SecurityTicket()
            {
                AdGroupName = "",
                IsAddingPermission = true,
                ParentSecurity = security,
                RequestedById = "078193",
                SecurityTicketId = Guid.NewGuid(),
                TicketStatus = GlobalConstants.HpsmTicketStatus.PENDING,
                TicketId = "C00123456",
                Permissions = new List<SecurityPermission>()
            };
        }

        private SecurityPermission BuildBasePermission(SecurityTicket ticket, Permission permission)
        {
            return new SecurityPermission()
            {
                AddedDate = DateTime.Now,
                SecurityPermissionId = Guid.NewGuid(),
                AddedFromTicket = ticket,
                Permission = permission
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
    }
}
