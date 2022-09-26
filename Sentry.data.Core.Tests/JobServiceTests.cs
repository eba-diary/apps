using Hangfire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.RetrieverJobOptions;

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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);

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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);
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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);
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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);
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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);
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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);

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

            RetrieverJob job = MockClasses.GetMockRetrieverJob(null, new FtpSource(), null);
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

        [TestMethod]
        public void CreateAndSaveRetrieverJob_NoExecutionParameters()
        {
            RetrieverJobDto dto = new RetrieverJobDto()
            {
                IsCompressed = true,
                CompressionType = "CompType",
                FileNameExclusionList = new List<string>() { "FileName2" },
                RequestMethod = HttpMethods.get,
                SearchCriteria = "SearchCriteria",
                CreateCurrentFile = true,
                TargetFileName = "FileName",
                DataSourceId = 1,
                FileSchema = 2,
                DataFlow = 3,
                RelativeUri = "RelativeUri",
                Schedule = "* * * * *"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.Setup(x => x.GetById<DataFlow>(3)).Returns(new DataFlow() { Id = 31 });
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsNull(job.DatasetConfig);
            Assert.IsNotNull(job.DataSource);
            Assert.IsInstanceOfType(job.DataSource, typeof(HTTPSSource));
            Assert.AreEqual(11, job.DataSource.Id);
            Assert.IsNotNull(job.FileSchema);
            Assert.AreEqual(21, job.FileSchema.SchemaId);
            Assert.IsNotNull(job.DataFlow);
            Assert.AreEqual(31, job.DataFlow.Id);
            Assert.IsFalse(job.IsGeneric);
            Assert.AreEqual("RelativeUri", job.RelativeUri);
            Assert.AreEqual("* * * * *", job.Schedule);
            Assert.AreEqual("Central Standard Time", job.TimeZone);
            Assert.AreEqual(ObjectStatusEnum.Active, job.ObjectStatus);
            Assert.IsNull(job.DeleteIssuer);
            Assert.AreEqual(DateTime.MaxValue, job.DeleteIssueDTM);
            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.IsNotNull(job.JobOptions);

            RetrieverJobOptions jobOptions = job.JobOptions;
            Assert.IsTrue(jobOptions.IsRegexSearch);
            Assert.IsFalse(jobOptions.OverwriteDataFile);
            Assert.AreEqual("SearchCriteria", jobOptions.SearchCriteria);
            Assert.IsTrue(jobOptions.CreateCurrentFile);
            Assert.AreEqual(FtpPattern.NoPattern, jobOptions.FtpPattern);
            Assert.AreEqual("FileName", jobOptions.TargetFileName);

            Assert.IsNotNull(jobOptions.CompressionOptions);

            Compression compression = jobOptions.CompressionOptions;
            Assert.IsTrue(compression.IsCompressed);
            Assert.AreEqual("CompType", compression.CompressionType);
            Assert.AreEqual(1, compression.FileNameExclusionList.Count);
            Assert.AreEqual("FileName2", compression.FileNameExclusionList.First());

            Assert.IsNotNull(jobOptions.HttpOptions);
            
            HttpsOptions httpsOptions = jobOptions.HttpOptions;
            Assert.IsNull(httpsOptions.Body);
            Assert.AreEqual(HttpMethods.get, httpsOptions.RequestMethod);
            Assert.AreEqual(HttpDataFormat.none, httpsOptions.RequestDataFormat);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_ActivePreviousJob()
        {
            RetrieverJobDto dto = new RetrieverJobDto()
            {
                DataSourceId = 1,
                FileSchema = 2,
                ExecutionParameters = new Dictionary<string, string>()
                {
                    { "Param1", "Value1" },
                    { "Param2", "Value2" }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 }
                },
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataFlow = new DataFlow() { SchemaId = 2 }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_PreviousJobRelativeUriMatch()
        {
            RetrieverJobDto dto = new RetrieverJobDto()
            {
                DataSourceId = 1,
                FileSchema = 2,
                RelativeUri = "RelativeUri",
                ExecutionParameters = new Dictionary<string, string>()
                {
                    { "Param1", "Value1" },
                    { "Param2", "Value2" }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUriNoMatch"
                },
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri"
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsTrue(job.ExecutionParameters.Any());
            Assert.IsTrue(job.ExecutionParameters.ContainsKey("Param1"));
            Assert.AreEqual("Value1", job.ExecutionParameters["Param1"]);
            Assert.IsTrue(job.ExecutionParameters.ContainsKey("Param2"));
            Assert.AreEqual("Value2", job.ExecutionParameters["Param2"]);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_PreviousJobRelativeUriNoMatch()
        {
            RetrieverJobDto dto = new RetrieverJobDto()
            {
                DataSourceId = 1,
                FileSchema = 2,
                RelativeUri = "RelativeUri",
                ExecutionParameters = new Dictionary<string, string>()
                {
                    { "Param1", "Value1" },
                    { "Param2", "Value2" }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri"
                },
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUriNoMatch"
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }
    }
}
