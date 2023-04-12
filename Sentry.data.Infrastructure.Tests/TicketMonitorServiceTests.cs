using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class TicketMonitorServiceTests
    {
        [TestMethod]
        public async Task CheckTicketStatus_Approve()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<ITicketProvider> ticketProvider = mr.Create<ITicketProvider>();

            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketStatus = ChangeTicketStatus.APPROVED,
                ApprovedById = "000001"
            };

            ticketProvider.Setup(x => x.RetrieveTicketAsync("ID")).ReturnsAsync(changeTicket);
            ticketProvider.Setup(x => x.CloseTicketAsync(changeTicket)).Returns(Task.CompletedTask);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4993_JSMTicketProvider.GetValue()).Returns(true);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            List<SecurityTicket> securityTickets = new List<SecurityTicket>
            {
                new SecurityTicket { TicketStatus = ChangeTicketStatus.PENDING, TicketId = "ID" }
            };
            datasetContext.SetupGet(x => x.HpsmTickets).Returns(securityTickets.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.ApproveTicket(securityTickets.First(), "000001")).Returns(Task.CompletedTask);

            Mock<IContainer> nestedContainer = mr.Create<IContainer>();
            nestedContainer.Setup(x => x.GetInstance<IDataFeatures>()).Returns(dataFeatures.Object);
            nestedContainer.Setup(x => x.GetInstance<ITicketProvider>("JSM")).Returns(ticketProvider.Object);
            nestedContainer.Setup(x => x.GetInstance<IDatasetContext>()).Returns(datasetContext.Object);
            nestedContainer.Setup(x => x.GetInstance<ISecurityService>()).Returns(securityService.Object);
            nestedContainer.Setup(x => x.Dispose());

            Mock<IContainer> container = mr.Create<IContainer>();
            container.Setup(x => x.GetNestedContainer()).Returns(nestedContainer.Object);

            Bootstrapper.InitForUnitTest(container.Object);

            TicketMonitorService ticketMonitorService = new TicketMonitorService();

            await ticketMonitorService.CheckTicketStatus();
            
            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CheckTicketStatus_Deny()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<ITicketProvider> ticketProvider = mr.Create<ITicketProvider>();

            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketStatus = ChangeTicketStatus.DENIED,
                RejectedById = "000001",
                RejectedReason = "Denied"
            };

            ticketProvider.Setup(x => x.RetrieveTicketAsync("ID")).ReturnsAsync(changeTicket);
            ticketProvider.Setup(x => x.CloseTicketAsync(changeTicket)).Returns(Task.CompletedTask);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4993_JSMTicketProvider.GetValue()).Returns(true);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            List<SecurityTicket> securityTickets = new List<SecurityTicket>
            {
                new SecurityTicket { TicketStatus = ChangeTicketStatus.PENDING, TicketId = "ID" }
            };
            datasetContext.SetupGet(x => x.HpsmTickets).Returns(securityTickets.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.CloseTicket(securityTickets.First(), "000001", "Denied", ChangeTicketStatus.DENIED));

            Mock<IContainer> nestedContainer = mr.Create<IContainer>();
            nestedContainer.Setup(x => x.GetInstance<IDataFeatures>()).Returns(dataFeatures.Object);
            nestedContainer.Setup(x => x.GetInstance<ITicketProvider>("JSM")).Returns(ticketProvider.Object);
            nestedContainer.Setup(x => x.GetInstance<IDatasetContext>()).Returns(datasetContext.Object);
            nestedContainer.Setup(x => x.GetInstance<ISecurityService>()).Returns(securityService.Object);
            nestedContainer.Setup(x => x.Dispose());

            Mock<IContainer> container = mr.Create<IContainer>();
            container.Setup(x => x.GetNestedContainer()).Returns(nestedContainer.Object);

            Bootstrapper.InitForUnitTest(container.Object);

            TicketMonitorService ticketMonitorService = new TicketMonitorService();

            await ticketMonitorService.CheckTicketStatus();

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CheckTicketStatus_Withdraw()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<ITicketProvider> ticketProvider = mr.Create<ITicketProvider>();

            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketStatus = ChangeTicketStatus.WITHDRAWN,
                RejectedById = "000001",
                RejectedReason = "Withdrawn"
            };

            ticketProvider.Setup(x => x.RetrieveTicketAsync("ID")).ReturnsAsync(changeTicket);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4993_JSMTicketProvider.GetValue()).Returns(true);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            List<SecurityTicket> securityTickets = new List<SecurityTicket>
            {
                new SecurityTicket { TicketStatus = ChangeTicketStatus.PENDING, TicketId = "ID" }
            };
            datasetContext.SetupGet(x => x.HpsmTickets).Returns(securityTickets.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.CloseTicket(securityTickets.First(), "000001", "Withdrawn", ChangeTicketStatus.WITHDRAWN));

            Mock<IContainer> nestedContainer = mr.Create<IContainer>();
            nestedContainer.Setup(x => x.GetInstance<IDataFeatures>()).Returns(dataFeatures.Object);
            nestedContainer.Setup(x => x.GetInstance<ITicketProvider>("JSM")).Returns(ticketProvider.Object);
            nestedContainer.Setup(x => x.GetInstance<IDatasetContext>()).Returns(datasetContext.Object);
            nestedContainer.Setup(x => x.GetInstance<ISecurityService>()).Returns(securityService.Object);
            nestedContainer.Setup(x => x.Dispose());

            Mock<IContainer> container = mr.Create<IContainer>();
            container.Setup(x => x.GetNestedContainer()).Returns(nestedContainer.Object);

            Bootstrapper.InitForUnitTest(container.Object);

            TicketMonitorService ticketMonitorService = new TicketMonitorService();

            await ticketMonitorService.CheckTicketStatus();

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CheckTicketStatus_Pending()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<ITicketProvider> ticketProvider = mr.Create<ITicketProvider>();

            ChangeTicket changeTicket = new ChangeTicket
            {
                TicketStatus = ChangeTicketStatus.PENDING
            };

            ticketProvider.Setup(x => x.RetrieveTicketAsync("ID")).ReturnsAsync(changeTicket);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4993_JSMTicketProvider.GetValue()).Returns(true);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            List<SecurityTicket> securityTickets = new List<SecurityTicket>
            {
                new SecurityTicket { TicketStatus = ChangeTicketStatus.PENDING, TicketId = "ID" }
            };
            datasetContext.SetupGet(x => x.HpsmTickets).Returns(securityTickets.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();

            Mock<IContainer> nestedContainer = mr.Create<IContainer>();
            nestedContainer.Setup(x => x.GetInstance<IDataFeatures>()).Returns(dataFeatures.Object);
            nestedContainer.Setup(x => x.GetInstance<ITicketProvider>("JSM")).Returns(ticketProvider.Object);
            nestedContainer.Setup(x => x.GetInstance<IDatasetContext>()).Returns(datasetContext.Object);
            nestedContainer.Setup(x => x.GetInstance<ISecurityService>()).Returns(securityService.Object);
            nestedContainer.Setup(x => x.Dispose());

            Mock<IContainer> container = mr.Create<IContainer>();
            container.Setup(x => x.GetNestedContainer()).Returns(nestedContainer.Object);

            Bootstrapper.InitForUnitTest(container.Object);

            TicketMonitorService ticketMonitorService = new TicketMonitorService();

            await ticketMonitorService.CheckTicketStatus();

            mr.VerifyAll();
        }
    }
}
