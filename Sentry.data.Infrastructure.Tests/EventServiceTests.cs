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

            DatasetFileConfig dfc = new DatasetFileConfig() 
            { 
                ParentDataset = new Dataset() { DatasetId = 2 },
                Schema = new FileSchema() { SchemaId = 3 }
            };
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
                Assert.AreEqual(3, x.SchemaId);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Search);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventByConfigId(GlobalConstants.EventType.SYNC_DATASET_SCHEMA, "Reason", 1).Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEventByDatasetId_Viewed_DatasetId0_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.VIEWED };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("Reason", x.Reason);
                Assert.AreEqual(0, x.Dataset);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataConfig);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Search);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED, "Reason", 0).Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEventByDatasetId_ViewedDataset_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.VIEWED_DATASET };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Dataset ds = new Dataset() { DatasetId = 1, DatasetFileConfigs = new List<DatasetFileConfig>() { new DatasetFileConfig() { ConfigId = 2 } } };
            context.Setup(x => x.GetById<Dataset>(1)).Returns(ds);

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("Reason", x.Reason);
                Assert.AreEqual(1, x.Dataset);
                Assert.AreEqual(2, x.DataConfig);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Search);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.VIEWED_DATASET, "Reason", 1).Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEventByDatasetId_CreatedDataset_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.CREATED_DATASET };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Dataset ds = new Dataset() 
            { 
                DatasetId = 1, 
                DatasetName = "DatasetName",
                DatasetFileConfigs = new List<DatasetFileConfig>() { new DatasetFileConfig() { ConfigId = 2 } },
                DatasetCategories = new List<Category>() { new Category() { Name = "Sentry" } } 
            };
            context.Setup(x => x.GetById<Dataset>(1)).Returns(ds);

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("A new dataset called DatasetName was created in Sentry", x.Reason);
                Assert.AreEqual(1, x.Dataset);
                Assert.AreEqual(2, x.DataConfig);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Search);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.CREATED_DATASET, "Reason", 1).Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEventByDataAsset_Search_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = "Search" };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("Reason", x.Reason);
                Assert.AreEqual(1, x.DataAsset);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual("LineCode", x.Line_CDE);
                Assert.AreEqual("SearchText", x.Search);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataConfig);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.Dataset);
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventByDataAsset("Search", "Reason", 1, "LineCode", "SearchText").Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEvent_DataInventorySearch_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.DATA_INVENTORY_SEARCH };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("Reason", x.Reason);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual("SearchText", x.Search);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataConfig);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.Dataset);
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEvent(GlobalConstants.EventType.DATA_INVENTORY_SEARCH, "Reason", "SearchText").Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEventByNotificationId_NotificationInfoAdd_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.NOTIFICATION_INFO_ADD };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Notification notification = new Notification();

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("Reason", x.Reason);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.AreEqual(notification, x.Notification);
                Assert.IsNull(x.DataConfig);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.Dataset);
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Search);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventByNotificationId(GlobalConstants.EventType.NOTIFICATION_INFO_ADD, "Reason", notification).Wait();

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void PublishSuccessEventBySchemaId_CreateDatasetSchema_Event()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();

            EventType eventType = new EventType() { Description = GlobalConstants.EventType.CREATE_DATASET_SCHEMA };
            context.SetupGet(x => x.EventTypes).Returns(new List<EventType>() { eventType }.AsQueryable());

            Status status = new Status() { Description = GlobalConstants.Statuses.SUCCESS };
            context.SetupGet(x => x.EventStatus).Returns(new List<Status>() { status }.AsQueryable());

            Dataset ds = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "DatasetName",
                DatasetCategories = new List<Category>() { new Category() { Name = "Sentry" } },
                DatasetFileConfigs = new List<DatasetFileConfig>() 
                { 
                    new DatasetFileConfig() 
                    { 
                        ConfigId = 3,
                        Schema =  new FileSchema()
                        {
                            SchemaId = 2,
                            Name = "SchemaName"
                        }
                    } 
                }
            };
            context.Setup(x => x.GetById<Dataset>(1)).Returns(ds);

            Mock<IApplicationUser> user = mockRepository.Create<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            context.Setup(x => x.Add(It.IsAny<Event>())).Callback<Event>(x =>
            {
                Assert.AreEqual("A new schema called SchemaName was created under DatasetName in Sentry", x.Reason);
                Assert.AreEqual(1, x.Dataset);
                Assert.AreEqual(2, x.SchemaId);
                Assert.AreEqual(3, x.DataConfig);
                Assert.AreEqual("000000", x.UserWhoStartedEvent);
                Assert.AreEqual(eventType, x.EventType);
                Assert.AreEqual(status, x.Status);
                Assert.IsNull(x.DataAsset);
                Assert.IsNull(x.DataFile);
                Assert.IsNull(x.Line_CDE);
                Assert.IsNull(x.Search);
                Assert.IsNull(x.Notification);
                Assert.IsFalse(x.IsProcessed);
            });
            context.Setup(x => x.SaveChanges(true));
            context.Setup(x => x.Dispose());

            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);

            EventService eventService = new EventService(contextGenerator.Object, userService.Object);
            eventService.PublishSuccessEventBySchemaId(GlobalConstants.EventType.CREATE_DATASET_SCHEMA, "Reason", 1, 2).Wait();

            mockRepository.VerifyAll();
        }
    }
}
