using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class EventServiceTests
    {
        [TestMethod]
        public void PublishSuccessEventByConfigId_SyncSchema_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.SYNC_DATASET_SCHEMA };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            DatasetFileConfig dfc = new DatasetFileConfig() { ParentDataset = new Dataset() { DatasetId = 2 } };
            context.Setup(x => x.GetById<DatasetFileConfig>(1)).Returns(dfc);

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("Reason", x.Reason);
                Assert.AreEqual(1, x.DataConfig);
                Assert.AreEqual(2, x.Dataset);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
            });
            context.Setup(x => x.SaveChanges(true));

            EventService eventService = new EventService(context.Object, userService.Object);
            eventService.PublishSuccessEventByConfigId(GlobalConstants.EventType.SYNC_DATASET_SCHEMA, "Reason", 1).Wait();

            mockRepository.VerifyAll();
        }
    }
}
