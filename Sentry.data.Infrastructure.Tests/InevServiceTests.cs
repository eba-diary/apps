using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Infrastructure.InfrastructureEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class InevServiceTests
    {

        /// <summary>
        /// Tests that when normal DSC permissions are being requested via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// will return just those new permissions
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_AddBasicRequested_Test()
        {
            // Arrange
            var ticket = new SecurityTicket()
            {
                IsAddingPermission = true,
                RequestedById = "012345",
                AdGroupName = "DS_MOCK_CNSMR_NP",
                AddedPermissions = GetConsumerSecurityPermissions(false),
                RemovedPermissions = new List<SecurityPermission>()
            };

            // Act
            var changes = InevService.GetPermissionChanges(ticket, null);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_ADD, changes.Action);
            Assert.AreEqual(2, changes.Permissions.Count);
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET));
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET));
            Assert.IsTrue(changes.Permissions.All(p => p.Status == DatasetPermissionsUpdatedDto.PermissionDto.STATUS_REQUESTED));
        }

        /// <summary>
        /// Tests that when normal DSC permissions are being requested via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// will return just those new permissions
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_AddBasicApproved_Test()
        {
            // Arrange
            var ticket = new SecurityTicket()
            {
                IsAddingPermission = true,
                RequestedById = "012345",
                AdGroupName = "DS_MOCK_CNSMR_NP",
                AddedPermissions = GetConsumerSecurityPermissions(true),
                RemovedPermissions = new List<SecurityPermission>(),
                ApprovedDate = DateTime.Now,
            };

            // Act
            var changes = InevService.GetPermissionChanges(ticket, null);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_ADD, changes.Action);
            Assert.AreEqual(2, changes.Permissions.Count);
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET));
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET));
            Assert.IsTrue(changes.Permissions.All(p => p.Status == DatasetPermissionsUpdatedDto.PermissionDto.STATUS_ACTIVE));
        }

        /// <summary>
        /// Tests that when normal DSC permissions are being removed via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// nothing is returned when the removal has not been approved
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_RemoveBasicRequested_Test()
        {
            // Arrange
            var ticket = new SecurityTicket()
            {
                IsAddingPermission = false,
                RequestedById = "012345",
                AdGroupName = "DS_MOCK_CNSMR_NP",
                AddedPermissions = new List<SecurityPermission>(),
                RemovedPermissions = GetConsumerSecurityPermissions(true)
            };

            // Act
            var changes = InevService.GetPermissionChanges(ticket, null);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_REMOVE, changes.Action);
            Assert.AreEqual(0, changes.Permissions.Count);
        }

        /// <summary>
        /// Tests that when normal DSC permissions are being removed via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// the permissions are returned when the removal has been approved
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_RemoveBasicApproved_Test()
        {
            // Arrange
            var ticket = new SecurityTicket()
            {
                IsAddingPermission = false,
                RequestedById = "012345",
                AdGroupName = "DS_MOCK_CNSMR_NP",
                AddedPermissions = new List<SecurityPermission>(),
                RemovedPermissions = GetConsumerSecurityPermissions(false, DateTime.Now),
                ApprovedDate = DateTime.Now,
            };

            // Act
            var changes = InevService.GetPermissionChanges(ticket, null);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_REMOVE, changes.Action);
            Assert.AreEqual(2, changes.Permissions.Count);
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET));
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET));
            Assert.IsTrue(changes.Permissions.All(p => p.Status == DatasetPermissionsUpdatedDto.PermissionDto.STATUS_DISABLED));
        }

        /// <summary>
        /// Tests that when the special <see cref="GlobalConstants.PermissionCodes.INHERIT_PARENT_PERMISSIONS"/>
        /// permission has simply been requested via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// will NOT return the parent asset's permissions (not until its approved)
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_AddInheritanceRequested_Test()
        {
            // Arrange

            //the SecurityTicket that represents Inheritance being added
            SecurityTicket ticket = GetInheritanceSecurityTicket(false);

            //the SecurablePermission list that contains the parent's permissions
            var parentPermissions = GetConsumerSecurityPermissions(true).Select(
                p => new SecurablePermission()
                {
                    Scope = SecurablePermissionScope.Self,
                    SecurityPermission = p
                }).ToList(); //since inheritance isn't approved yet, perms are only on the parent

            // Act
            var changes = InevService.GetPermissionChanges(ticket, parentPermissions);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_ADD, changes.Action);
            Assert.AreEqual(0, changes.Permissions.Count);
        }

        /// <summary>
        /// Tests that when the special <see cref="GlobalConstants.PermissionCodes.INHERIT_PARENT_PERMISSIONS"/>
        /// permission has been approved via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// will return the parent asset's permissions
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_AddInheritanceApproved_Test()
        {
            // Arrange

            //the SecurityTicket that represents Inheritance being added
            var ticket = GetInheritanceSecurityTicket(true);
            ticket.ApprovedDate = DateTime.Now;

            //the SecurablePermission list that contains the parent's permissions
            var parentPermissions = GetConsumerSecurityPermissions(true).Select(
                p => new SecurablePermission()
                {
                    Scope = SecurablePermissionScope.Self,
                    SecurityPermission = p
                }).ToList();

            // Act
            var changes = InevService.GetPermissionChanges(ticket, parentPermissions);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_ADD, changes.Action);
            Assert.AreEqual(2, changes.Permissions.Count);
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET));
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET));
            Assert.IsTrue(changes.Permissions.All(p => p.Status == DatasetPermissionsUpdatedDto.PermissionDto.STATUS_ACTIVE));
        }

        /// <summary>
        /// Tests that when the special <see cref="GlobalConstants.PermissionCodes.INHERIT_PARENT_PERMISSIONS"/>
        /// permission has been requested to be removed via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// will NOT return any changes (not until its approved)
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_RemoveInheritanceRequested_Test()
        {
            // Arrange

            //the SecurityTicket that represents Inheritance being added
            SecurityTicket ticket = GetInheritanceSecurityTicket(true);
            ticket.RemovedPermissions = ticket.AddedPermissions;
            ticket.IsAddingPermission = false;
            ticket.AddedPermissions = new List<SecurityPermission>();

            //the SecurablePermission list that contains the parent's permissions
            var parentPermissions = GetConsumerSecurityPermissions(true).Select(
                p => new SecurablePermission()
                {
                    Scope = SecurablePermissionScope.Self,
                    SecurityPermission = p
                }).ToList();

            // Act
            var changes = InevService.GetPermissionChanges(ticket, parentPermissions);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_REMOVE, changes.Action);
            Assert.AreEqual(0, changes.Permissions.Count);
        }

        /// <summary>
        /// Tests that when the special <see cref="GlobalConstants.PermissionCodes.INHERIT_PARENT_PERMISSIONS"/>
        /// permission has been approved to be removed via a <see cref="SecurityTicket"/>,
        /// <see cref="InevService.GetPermissionChanges(SecurityTicket, IList{SecurablePermission})"/>
        /// will return the removed inherited permissions
        /// </summary>
        [TestMethod]
        public void GetPermissionChanges_RemoveInheritanceApproved_Test()
        {
            // Arrange

            //the SecurityTicket that represents Inheritance being added
            SecurityTicket ticket = GetInheritanceSecurityTicket(false);
            ticket.RemovedPermissions = ticket.AddedPermissions;
            ticket.IsAddingPermission = false;
            ticket.ApprovedDate = DateTime.Now;
            ticket.RemovedPermissions[0].RemovedDate = DateTime.Now;
            ticket.AddedPermissions = new List<SecurityPermission>();

            //the SecurablePermission list that contains the parent's permissions
            var parentPermissions = GetConsumerSecurityPermissions(true).Select(
                p => new SecurablePermission()
                {
                    Scope = SecurablePermissionScope.Self,
                    SecurityPermission = p
                }).ToList(); //since inheritance isn't approved yet, perms are only on the parent

            // Act
            var changes = InevService.GetPermissionChanges(ticket, parentPermissions);

            // Assert
            Assert.AreEqual(DatasetPermissionsUpdatedDto.ChangesDto.ACTION_REMOVE, changes.Action);
            Assert.AreEqual(2, changes.Permissions.Count);
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET));
            Assert.IsTrue(changes.Permissions.Any(p => p.PermissionCode == GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET));
            Assert.IsTrue(changes.Permissions.All(p => p.Status == DatasetPermissionsUpdatedDto.PermissionDto.STATUS_DISABLED));
        }



        private static SecurityTicket GetInheritanceSecurityTicket(bool enabled)
        {
            return new SecurityTicket()
            {
                IsAddingPermission = true,
                RequestedById = "012345",
                AdGroupName = "DS_MOCK_CNSMR_NP",
                AddedPermissions = new List<SecurityPermission>()
                {
                    new SecurityPermission()
                    {
                        Permission = new Permission()
                        {
                            PermissionCode = GlobalConstants.PermissionCodes.INHERIT_PARENT_PERMISSIONS,
                        },
                        IsEnabled = enabled
                    }
                },
                RemovedPermissions = new List<SecurityPermission>()
            };
        }

        private static List<SecurityPermission> GetConsumerSecurityPermissions(bool enabled, DateTime? removedDate = null)
        {
            return new List<SecurityPermission>()
                {
                    new SecurityPermission()
                    {
                        Permission = new Permission()
                        {
                            PermissionCode = GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET,
                        },
                        RemovedDate = removedDate,
                        IsEnabled = enabled
                    },
                    new SecurityPermission()
                    {
                        Permission = new Permission()
                        {
                            PermissionCode = GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET,
                        },
                        RemovedDate = removedDate,
                        IsEnabled = enabled
                    }
                };
        }
    }
}
