using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate.Util;
using Sentry.ChangeManagement;
using Sentry.ChangeManagement.Sentry;
using Sentry.data.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class JsmTicketProviderTests
    {
        [TestMethod]
        public async Task CreateTicketAsync_AccessRequest_AwsArn_TicketId()
        {
            DateTime now = DateTime.Now;
            int endDay = now.AddDays(14).Day;

            AccessRequest accessRequest = new AccessRequest
            {
                Type = AccessRequestType.AwsArn,
                IsAddingPermission = true,
                AwsArn = "aws:arn",
                Scope = AccessScope.Asset,
                SaidKeyCode = "SAID",
                Permissions = new List<Permission>
                {
                    new Permission { PermissionName = "Perm1", PermissionDescription = "Perm 1" },
                    new Permission { PermissionName = "Perm2", PermissionDescription = "Perm 2" }
                },
                BusinessReason = "For the business",
                RequestorsId = "000000",
                RequestorsName = "Foo bar",
                ApproverId = "000001"
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange { ChangeID = "ITCM-001" };
            client.Setup(x => x.NewSentryChange(It.IsAny<SentryChange>())).ReturnsAsync(change).Callback<SentryChange>(x =>
            {
                Assert.AreEqual("Access request for AWS ARN aws:arn", x.Title);
                Assert.AreEqual(GetAwsArnDescriptionResult(), x.Description);
                Assert.IsTrue(x.PlannedStart >= now);
                Assert.AreEqual(endDay, x.PlannedEnd.Day);
                Assert.AreEqual(endDay, x.CompletionDate.Day);
                Assert.AreEqual(JsmAssignmentGroup.BI_PORTAL_ADMIN, x.AssignedTeam);
                Assert.AreEqual("NA", x.ImplementationNotes);
                Assert.AreEqual("NA", x.ImplementationPlan);
                Assert.AreEqual(1, x.Approvers.Count);
                Assert.AreEqual("000001", x.Approvers.First().ApproverID);
            });

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            string result = await ticketProvider.CreateTicketAsync(accessRequest);

            Assert.AreEqual("ITCM-001", result);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CreateTicketAsync_AccessRequest_SnowflakeAccount_TicketId()
        {
            DateTime now = DateTime.Now;
            int endDay = now.AddDays(14).Day;

            AccessRequest accessRequest = new AccessRequest
            {
                Type = AccessRequestType.SnowflakeAccount,
                IsAddingPermission = true,
                SnowflakeAccount = "snowaccount",
                Scope = AccessScope.Dataset,
                SecurableObjectName = "Dataset Name",
                SecurableObjectNamedEnvironment = "DEV",
                Permissions = new List<Permission>
                {
                    new Permission { PermissionName = "Perm1", PermissionDescription = "Perm 1" },
                    new Permission { PermissionName = "Perm2", PermissionDescription = "Perm 2" }
                },
                BusinessReason = "For the business",
                RequestorsId = "000000",
                RequestorsName = "Foo bar",
                ApproverId = "000001"
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange { ChangeID = "ITCM-001" };
            client.Setup(x => x.NewSentryChange(It.IsAny<SentryChange>())).ReturnsAsync(change).Callback<SentryChange>(x =>
            {
                Assert.AreEqual("Access request for Snowflake Account snowaccount", x.Title);
                Assert.AreEqual(GetSnowflakeAccountDescriptionResult(), x.Description);
                Assert.IsTrue(x.PlannedStart >= now);
                Assert.AreEqual(endDay, x.PlannedEnd.Day);
                Assert.AreEqual(endDay, x.CompletionDate.Day);
                Assert.AreEqual(JsmAssignmentGroup.BI_PORTAL_ADMIN, x.AssignedTeam);
                Assert.AreEqual("NA", x.ImplementationNotes);
                Assert.AreEqual("NA", x.ImplementationPlan);
                Assert.AreEqual(1, x.Approvers.Count);
                Assert.AreEqual("000001", x.Approvers.First().ApproverID);
            });

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            string result = await ticketProvider.CreateTicketAsync(accessRequest);

            Assert.AreEqual("ITCM-001", result);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CreateTicketAsync_AccessRequest_Inheritance_TicketId()
        {
            DateTime now = DateTime.Now;
            int endDay = now.AddDays(14).Day;

            AccessRequest accessRequest = new AccessRequest
            {
                Type = AccessRequestType.Inheritance,
                IsAddingPermission = true,
                SnowflakeAccount = "snowaccount",
                Scope = AccessScope.Dataset,
                SecurableObjectName = "Dataset Name",
                SecurableObjectNamedEnvironment = "DEV",
                SaidKeyCode = "SAID",
                Permissions = new List<Permission>
                {
                    new Permission { PermissionName = "Perm1", PermissionDescription = "Perm 1" },
                    new Permission { PermissionName = "Perm2", PermissionDescription = "Perm 2" }
                },
                BusinessReason = "For the business",
                RequestorsId = "000000",
                RequestorsName = "Foo bar",
                ApproverId = "000001"
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange { ChangeID = "ITCM-001" };
            client.Setup(x => x.NewSentryChange(It.IsAny<SentryChange>())).ReturnsAsync(change).Callback<SentryChange>(x =>
            {
                Assert.AreEqual("Inheritance enable request for Dataset Name", x.Title);
                Assert.AreEqual(GetInheritanceDescriptionResult(), x.Description);
                Assert.IsTrue(x.PlannedStart >= now);
                Assert.AreEqual(endDay, x.PlannedEnd.Day);
                Assert.AreEqual(endDay, x.CompletionDate.Day);
                Assert.AreEqual(JsmAssignmentGroup.BI_PORTAL_ADMIN, x.AssignedTeam);
                Assert.AreEqual("NA", x.ImplementationNotes);
                Assert.AreEqual("NA", x.ImplementationPlan);
                Assert.AreEqual(1, x.Approvers.Count);
                Assert.AreEqual("000001", x.Approvers.First().ApproverID);
            });

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            string result = await ticketProvider.CreateTicketAsync(accessRequest);

            Assert.AreEqual("ITCM-001", result);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CreateTicketAsync_AccessRequest_Default_TicketId()
        {
            DateTime now = DateTime.Now;
            int endDay = now.AddDays(14).Day;

            AccessRequest accessRequest = new AccessRequest
            {
                Type = AccessRequestType.Default,
                IsAddingPermission = true,
                AdGroupName = "AD_GROUP",
                Scope = AccessScope.Asset,
                SaidKeyCode = "SAID",
                Permissions = new List<Permission>
                {
                    new Permission { PermissionName = "Perm1", PermissionDescription = "Perm 1" },
                    new Permission { PermissionName = "Perm2", PermissionDescription = "Perm 2" }
                },
                BusinessReason = "For the business",
                RequestorsId = "000000",
                RequestorsName = "Foo bar",
                ApproverId = "000001"
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange { ChangeID = "ITCM-001" };
            client.Setup(x => x.NewSentryChange(It.IsAny<SentryChange>())).ReturnsAsync(change).Callback<SentryChange>(x =>
            {
                Assert.AreEqual("Access request for AD Group AD_GROUP", x.Title);
                Assert.AreEqual(GetDefaultDescriptionResult(), x.Description);
                Assert.IsTrue(x.PlannedStart >= now);
                Assert.AreEqual(endDay, x.PlannedEnd.Day);
                Assert.AreEqual(endDay, x.CompletionDate.Day);
                Assert.AreEqual(JsmAssignmentGroup.BI_PORTAL_ADMIN, x.AssignedTeam);
                Assert.AreEqual("NA", x.ImplementationNotes);
                Assert.AreEqual("NA", x.ImplementationPlan);
                Assert.AreEqual(1, x.Approvers.Count);
                Assert.AreEqual("000001", x.Approvers.First().ApproverID);
            });

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            string result = await ticketProvider.CreateTicketAsync(accessRequest);

            Assert.AreEqual("ITCM-001", result);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CreateTicketAsync_CatchException()
        {
            AccessRequest accessRequest = new AccessRequest
            {
                Type = AccessRequestType.Inheritance,
                IsAddingPermission = true,
                SnowflakeAccount = "snowaccount",
                Scope = AccessScope.Dataset,
                SecurableObjectName = "Dataset Name",
                SecurableObjectNamedEnvironment = "DEV",
                SaidKeyCode = "SAID",
                Permissions = new List<Permission>
                {
                    new Permission { PermissionName = "Perm1", PermissionDescription = "Perm 1" },
                    new Permission { PermissionName = "Perm2", PermissionDescription = "Perm 2" }
                },
                BusinessReason = "For the business",
                RequestorsId = "000000",
                RequestorsName = "Foo bar",
                ApproverId = "000001"
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            client.Setup(x => x.NewSentryChange(It.IsAny<SentryChange>())).ThrowsAsync(new Exception());

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            string result = await ticketProvider.CreateTicketAsync(accessRequest);

            Assert.AreEqual(string.Empty, result);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task RetrieveTicketAsync_Id_Approved_ChangeTicket()
        {
            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange 
            { 
                ChangeID = "ITCM-001",
                Status = JsmChangeStatus.AWAITING_IMPLEMENTATION,
                Approvers = new List<SentryApprover>
                {
                    new SentryApprover { ApproverID = "000001" }
                }
            };
            client.Setup(x => x.GetChange("ITCM-001")).ReturnsAsync(change);

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            ChangeTicket result = await ticketProvider.RetrieveTicketAsync("ITCM-001");

            Assert.AreEqual("ITCM-001", result.TicketId);
            Assert.AreEqual("000001", result.ApprovedById);
            Assert.AreEqual(ChangeTicketStatus.APPROVED, result.TicketStatus);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task RetrieveTicketAsync_Id_Denied_ChangeTicket()
        {
            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange
            {
                ChangeID = "ITCM-001",
                Status = JsmChangeStatus.DECLINED,
                Approvers = new List<SentryApprover>
                {
                    new SentryApprover { ApproverID = "000001" }
                }
            };
            client.Setup(x => x.GetChange("ITCM-001")).ReturnsAsync(change);

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            ChangeTicket result = await ticketProvider.RetrieveTicketAsync("ITCM-001");

            Assert.AreEqual("ITCM-001", result.TicketId);
            Assert.AreEqual("000001", result.RejectedById);
            Assert.AreEqual("Approver declined", result.RejectedReason);
            Assert.AreEqual(ChangeTicketStatus.DENIED, result.TicketStatus);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task RetrieveTicketAsync_Id_Withdrawn_ChangeTicket()
        {
            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange
            {
                ChangeID = "ITCM-001",
                Status = JsmChangeStatus.CANCELED,
                Approvers = new List<SentryApprover>
                {
                    new SentryApprover { ApproverID = "000001" }
                }
            };
            client.Setup(x => x.GetChange("ITCM-001")).ReturnsAsync(change);

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            ChangeTicket result = await ticketProvider.RetrieveTicketAsync("ITCM-001");

            Assert.AreEqual("ITCM-001", result.TicketId);
            Assert.AreEqual("Ticket cancelled", result.RejectedReason);
            Assert.AreEqual(ChangeTicketStatus.WITHDRAWN, result.TicketStatus);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task RetrieveTicketAsync_Id_Pending_ChangeTicket()
        {
            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            SentryChange change = new SentryChange
            {
                ChangeID = "ITCM-001",
                Status = JsmChangeStatus.INDIVIDUAL_AUTHORIZE,
                Approvers = new List<SentryApprover>
                {
                    new SentryApprover { ApproverID = "000001" }
                }
            };
            client.Setup(x => x.GetChange("ITCM-001")).ReturnsAsync(change);

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            ChangeTicket result = await ticketProvider.RetrieveTicketAsync("ITCM-001");

            Assert.AreEqual("ITCM-001", result.TicketId);
            Assert.AreEqual(ChangeTicketStatus.PENDING, result.TicketStatus);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task RetrieveTicketAsync_CatchException()
        {
            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            client.Setup(x => x.GetChange("ITCM-001")).ThrowsAsync(new Exception());

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            ChangeTicket result = await ticketProvider.RetrieveTicketAsync("ITCM-001");

            Assert.IsNull(result);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CloseTicketAsync_PendingTicket()
        {
            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketId = "ITCM-001",
                TicketStatus = ChangeTicketStatus.PENDING
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            client.Setup(x => x.CloseChange("ITCM-001", "Request cancelled", CloseStatus.CANCELLED)).Returns(Task.CompletedTask);

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            await ticketProvider.CloseTicketAsync(changeTicket);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CloseTicketAsync_ApprovedTicket()
        {
            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketId = "ITCM-001",
                TicketStatus = ChangeTicketStatus.APPROVED
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            client.Setup(x => x.CloseChange("ITCM-001", "Request approved", CloseStatus.COMPLETED)).Returns(Task.CompletedTask);

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            await ticketProvider.CloseTicketAsync(changeTicket);

            client.VerifyAll();
        }

        [TestMethod]
        public async Task CloseTicketAsync_DeniedTicket()
        {
            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketId = "ITCM-001",
                TicketStatus = ChangeTicketStatus.DENIED
            };

            JsmTicketProvider ticketProvider = new JsmTicketProvider(null);

            await ticketProvider.CloseTicketAsync(changeTicket);
        }

        [TestMethod]
        public async Task CloseTicketAsync_CatchException()
        {
            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketId = "ITCM-001",
                TicketStatus = ChangeTicketStatus.APPROVED
            };

            Mock<ISentryChangeManagementClient> client = new Mock<ISentryChangeManagementClient>(MockBehavior.Strict);
            client.Setup(x => x.CloseChange("ITCM-001", "Request approved", CloseStatus.COMPLETED)).ThrowsAsync(new Exception());

            JsmTicketProvider ticketProvider = new JsmTicketProvider(client.Object);

            await ticketProvider.CloseTicketAsync(changeTicket);

            client.VerifyAll();
        }

        #region Helpers
        private string GetAwsArnDescriptionResult()
        {
            return @"Please grant the AWS ARN *aws:arn* the following permissions to SAID data.
- Perm1 \- Perm 1
- Perm2 \- Perm 2

*Business Reason:* For the business
*Requestor:* 000000 \- Foo bar
*DSC Environment:* localhost.sentry.com:44321";
        }

        private string GetSnowflakeAccountDescriptionResult()
        {
            return @"Please grant the Snowflake Account *snowaccount* the following permissions to Dataset Name (DEV) data.
- Perm1 \- Perm 1
- Perm2 \- Perm 2

*Business Reason:* For the business
*Requestor:* 000000 \- Foo bar
*DSC Environment:* localhost.sentry.com:44321";
        }

        private string GetInheritanceDescriptionResult()
        {
            return @"Please enable inheritance for dataset *Dataset Name* from Data.Sentry.com. Enabling inheritance will allow the dataset to inherit permissions from its parent asset SAID. When approved, users with access to SAID in Data.Sentry.com will have access to Dataset Name data.
- Perm1 \- Perm 1
- Perm2 \- Perm 2

For more information on Authorization in DSC \- [Auth Guide|https://confluence.sentry.com/pages/viewpage.action?pageId=361734893]

*Said Asset:* SAID
*Business Reason:* For the business
*Requestor:* 000000 \- Foo bar
*DSC Environment:* localhost.sentry.com:44321";
        }

        private string GetDefaultDescriptionResult()
        {
            return @"Please grant *AD_GROUP* the following permissions to SAID in Data.sentry.com.
- Perm1 \- Perm 1
- Perm2 \- Perm 2

*Said Asset:* SAID
*Business Reason:* For the business
*Requestor:* 000000 \- Foo bar
*DSC Environment:* localhost.sentry.com:44321";
        }
        #endregion
    }
}
