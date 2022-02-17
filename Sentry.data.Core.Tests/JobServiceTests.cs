using Hangfire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class JobServiceTests
    {
        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Does_Not_Call_Save_Changes()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(job.Id)).Returns(job);

            Mock<IRecurringJobManager> jobManager = mr.Create<IRecurringJobManager>();
            jobManager.Setup(x => x.RemoveIfExists(It.IsAny<string>()));

            var JobService = new JobService(context.Object, null, jobManager.Object);

            // Act
            JobService.Delete(job.Id, user.Object, true);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Job_marked_Deleted_an_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(job.Id)).Returns(job);

            var JobService = new JobService(context.Object, null, null);

            // Act
            bool isSuccessfull = JobService.Delete(job.Id, user.Object, false);

            // Assert
            Assert.AreEqual(true, isSuccessfull);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Job_marked_Pending_Delete_an_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(job.Id)).Returns(job);

            var JobService = new JobService(context.Object, null, null);

            // Act
            bool isSuccessfull = JobService.Delete(job.Id, user.Object, true);

            // Assert
            Assert.AreEqual(true, isSuccessfull);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Returns_False_When_Job_Not_Found_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(It.IsAny<int>())).Returns((RetrieverJob)null);

            var JobService = new JobService(context.Object, null, null);

            // Act
            bool isSuccessfull = JobService.Delete(1, user.Object, true);

            // Assert
            Assert.AreEqual(false, isSuccessfull);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Returns_False_When_Job_Not_Found_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(It.IsAny<int>())).Returns((RetrieverJob)null);

            var JobService = new JobService(context.Object, null, null);

            // Act
            bool isSuccessfull = JobService.Delete(1, user.Object, false);

            // Assert
            Assert.AreEqual(false, isSuccessfull);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Calls_Job_Manager_To_Remove_Associated_HangFire_Job_When_LogicalDelete_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(job.Id)).Returns(job);

            Mock<IRecurringJobManager> jobManager = mr.Create<IRecurringJobManager>();
            jobManager.Setup(x => x.RemoveIfExists(It.IsAny<string>()));

            var JobService = new JobService(context.Object, null, jobManager.Object);

            // Act
            JobService.Delete(job.Id, user.Object, true);

            // Assert
            jobManager.Verify(x => x.RemoveIfExists(It.IsAny<string>()), Times.Once);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void Delete_Calls_Job_Manager_To_Remove_Associated_HangFire_Job_When_LogicalDelete_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            RetrieverJob job = MockClasses.GetMockRetrieverJob();
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<RetrieverJob>(job.Id)).Returns(job);

            Mock<IRecurringJobManager> jobManager = mr.Create<IRecurringJobManager>();
            jobManager.Setup(x => x.RemoveIfExists(It.IsAny<string>()));

            var JobService = new JobService(context.Object, null, jobManager.Object);

            // Act
            JobService.Delete(job.Id, user.Object, false);

            // Assert
            jobManager.Verify(x => x.RemoveIfExists(It.IsAny<string>()), Times.Once);
        }


    }
}
