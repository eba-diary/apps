using Hangfire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Sentry.data.Core.DTO.Job;
using Sentry.data.Core.Entities.Livy;
using System;
using System.Text;

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

            var JobService = new JobService(context.Object, null, jobManager.Object, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, jobManager.Object, null, null);

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

            var JobService = new JobService(context.Object, null, jobManager.Object, null, null);

            // Act
            JobService.Delete(job.Id, user.Object, false);

            // Assert
            jobManager.Verify(x => x.RemoveIfExists(It.IsAny<string>()), Times.Once);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void AddElement_StringValue()
        {
            //Arrange
            StringBuilder builder_newValue = new StringBuilder();
            StringBuilder builder_defaultValue = new StringBuilder();
            string elName = "elementName";
            string newValue = "newValue";
            string defaultValue = "defaultValue";

            JobService jobService = new JobService(null, null, null, null, null);

            //Act
            jobService.AddElement(builder_newValue, elName, newValue, defaultValue);
            jobService.AddElement(builder_defaultValue, elName, null, defaultValue);

            //Assert
            Assert.AreEqual(", \"elementName\": \"newValue\"", builder_newValue.ToString(), "New value assignment failed");
            Assert.AreEqual(", \"elementName\": \"defaultValue\"", builder_defaultValue.ToString(), "Default value assignment failed");

        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void AddElement_StringValue_Exception()
        {
            //Arrange
            StringBuilder builder = new StringBuilder();
            string elName = "elementName";
            string newValue = null;
            string defaultValue = "defaultValue";

            JobService jobService = new JobService(null, null, null, null, null);

            //Act
            jobService.AddElement(builder, elName, newValue, defaultValue);

            //Assert
            Assert.ThrowsException<ArgumentNullException>(() => jobService.AddElement(builder, null, newValue, defaultValue), "Failed to thow exception for null elementName");
            Assert.ThrowsException<ArgumentNullException>(() => jobService.AddElement(builder, elName, String.Empty, String.Empty), "Failed to thow exception for null newValue and defaultValue");
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void AddElement_IntValue()
        {
            //Arrange
            StringBuilder builder_newValue = new StringBuilder();
            StringBuilder builder_defaultValue = new StringBuilder();
            string elName = "elementName";
            int newValue = 1;
            int defaultValue = 99;

            JobService jobService = new JobService(null, null, null, null, null);

            //Act
            jobService.AddElement(builder_newValue, elName, newValue, defaultValue);
            jobService.AddElement(builder_defaultValue, elName, null, defaultValue);

            //Assert
            Assert.AreEqual(", \"elementName\": 1", builder_newValue.ToString(), "New value assignment failed");
            Assert.AreEqual(", \"elementName\": 99", builder_defaultValue.ToString(), "Default value assignment failed");

        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void AddElement_IntValue_Exception()
        {
            //Arrange
            StringBuilder builder = new StringBuilder();
            string elName = "elementName";
            int newValue = 1;
            int defaultValue = 99;

            JobService jobService = new JobService(null, null, null, null, null);

            //Act
            jobService.AddElement(builder, elName, newValue, defaultValue);

            //Assert
            Assert.ThrowsException<ArgumentNullException>(() => jobService.AddElement(builder, null, newValue, defaultValue), "Failed to thow exception for null elementName");
            Assert.ThrowsException<ArgumentNullException>(() => jobService.AddElement(builder, elName, String.Empty, String.Empty), "Failed to thow exception for null newValue and defaultValue");
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void AddArgementsElement()
        {
            //Arrange
            StringBuilder builder_newValue = new StringBuilder();
            StringBuilder builder_defaultValue = new StringBuilder();
            string[] argsNewValue = new string[] { "\"type\":\"alpha\"", "\"count\":5678" };
            string[] argsDefaultValue = new string[] { "\"abc\":\"zyz\"", "\"123\":9999" };

            JobService jobService = new JobService(null, null, null, null, null);

            //Act
            jobService.AddArgumentsElement(builder_newValue, argsNewValue, argsDefaultValue);
            jobService.AddArgumentsElement(builder_defaultValue, null, argsDefaultValue);

            //Arrange
            Assert.AreEqual(", \"args\": [\"\"type\":\"alpha\"\",\"\"count\":5678\"]", builder_newValue.ToString(), "New value assignment failed");
            Assert.AreEqual(", \"args\": [\"\"abc\":\"zyz\"\",\"\"123\":9999\"]", builder_defaultValue.ToString(), "Default value assignment failed");
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void BuildLivyPostContent()
        {
            //Arrange
            Mock<IDataFeatures> features = new Mock<IDataFeatures>();
            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            features.Setup(s => s.CLA3497_UniqueLivySessionName.GetValue()).Returns(true);            

            var javaOptionsOverrideDto = new JavaOptionsOverrideDto()
            {
                ClusterUrl = "abc.com",
                DriverMemory = "22GB",
                DriverCores = 22,
                ExecutorMemory = "88GB",
                ExecutorCores = 88,
                NumExecutors = 9999,
                ConfigurationParameters = "Override parameters",
                Arguments = new string[] { "\"123\":7777" }
            };

            var DataSource = new JavaAppSource()
            {
                Options = new SourceOptions()
                {
                    JarFile = "com.something.file",
                    ClassName = "awesome_class_name",
                    JarDepenencies = new string[] { "jar1", "anotherjar2" }
                },
                Name = "This_DataSource_Name"
            };

            var retrieverJob = new RetrieverJob()
            {
                DataSource = DataSource,
                JobOptions = new RetrieverJobOptions()
                {
                    JavaAppOptions = new RetrieverJobOptions.JavaOptions()
                    {
                        DriverMemory = "1MB",
                        DriverCores = 1,
                        ExecutorMemory = "99MB",
                        ExecutorCores = 99,
                        NumExecutors = 12345,
                        ConfigurationParameters = "config parameters",
                        Arguments = new string[] { "\"abc\":\"zyz\"" }
                    }
                }
            };

            context.Setup(s => s.GetById<JavaAppSource>(It.IsAny<int>())).Returns(DataSource);

            Mock<JobService> service = new Mock<JobService>(context.Object, null, null, features.Object, null) { CallBase = true };
            service.Setup(s => s.GenerateUniqueLivySessionName(DataSource)).Returns("session_name");

            //Act
            var result = service.Object.BuildLivyPostContent(javaOptionsOverrideDto, retrieverJob);

            //Assert
            Assert.AreEqual("{\"file\": \"com.something.file\", \"className\": \"awesome_class_name\", \"name\": \"session_name\", \"driverMemory\": \"22GB\", \"driverCores\": 22, \"executorMemory\": \"88GB\", \"executorCores\": 88, \"numExecutors\": 9999, \"conf\": \"Override parameters\", \"args\": [\"\"123\":7777\"], \"jars\": [\"jar1\",\"anotherjar2\"]}", result);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void BuildLivyPostContent_Default_Values()
        {
            //Arrange
            Mock<IDataFeatures> features = new Mock<IDataFeatures>();
            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            features.Setup(s => s.CLA3497_UniqueLivySessionName.GetValue()).Returns(true);


            var javaOptionsOverrideDto = new JavaOptionsOverrideDto()
            {
                ClusterUrl = "abc.com",
                DriverMemory = null,
                DriverCores = null,
                ExecutorMemory = null,
                ExecutorCores = null,
                NumExecutors = null,
                ConfigurationParameters = null,
                Arguments = null
            };

            var DataSource = new JavaAppSource()
            {
                Options = new SourceOptions()
                {
                    JarFile = "com.something.file",
                    ClassName = "awesome_class_name",
                    JarDepenencies = new string[] { "jar1", "anotherjar2" }
                },
                Name = "This_DataSource_Name"
            };

            var retrieverJob = new RetrieverJob()
            {
                DataSource = DataSource,
                JobOptions = new RetrieverJobOptions()
                {
                    JavaAppOptions = new RetrieverJobOptions.JavaOptions()
                    {
                        DriverMemory = "1MB",
                        DriverCores = 1,
                        ExecutorMemory = "99MB",
                        ExecutorCores = 99,
                        NumExecutors = 12345,
                        ConfigurationParameters = "config parameters",
                        Arguments = new string[] { "\"abc\":\"zyz\"" }
                    }
                }
            };

            context.Setup(s => s.GetById<JavaAppSource>(It.IsAny<int>())).Returns(DataSource);

            Mock<JobService> service = new Mock<JobService>(context.Object, null, null, features.Object, null) { CallBase = true };
            service.Setup(s => s.GenerateUniqueLivySessionName(DataSource)).Returns("session_name");

            //Act
            var result = service.Object.BuildLivyPostContent(javaOptionsOverrideDto, retrieverJob);

            //Assert
            Assert.AreEqual("{\"file\": \"com.something.file\", \"className\": \"awesome_class_name\", \"name\": \"session_name\", \"driverMemory\": \"1MB\", \"driverCores\": 1, \"executorMemory\": \"99MB\", \"executorCores\": 99, \"numExecutors\": 12345, \"conf\": \"config parameters\", \"args\": [\"\"abc\":\"zyz\"\"], \"jars\": [\"jar1\",\"anotherjar2\"]}", result);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public void MapToSubmission()
        {
            var javaOptionsOverrideDto = new JavaOptionsOverrideDto()
            {
                ClusterUrl = "abc.com",
                FlowExecutionGuid = "1111111111",
                RunInstanceGuid = "9999999999"
            };

            var retrieverJob = new RetrieverJob()
            {
                Id = 22,
                JobGuid = Guid.Parse("ADBB3009-2B71-4A20-B416-8858C77C216E"),
            };

            JobService service = new JobService(null, null, null, null, null);

            //Act
            var result = service.MapToSubmission(retrieverJob, javaOptionsOverrideDto);

            Assert.AreEqual(javaOptionsOverrideDto.ClusterUrl, result.ClusterUrl);
            Assert.AreEqual(javaOptionsOverrideDto.FlowExecutionGuid, result.FlowExecutionGuid);
            Assert.AreEqual(javaOptionsOverrideDto.RunInstanceGuid, result.RunInstanceGuid);
            Assert.AreEqual(JsonConvert.SerializeObject(javaOptionsOverrideDto), result.Serialized_Job_Options);
            Assert.AreEqual(retrieverJob, result.JobId);
            Assert.AreEqual(retrieverJob.JobGuid, result.JobGuid);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SubmitApacheLivyJobInternalAsync(bool isLivyCallSuccessful)
        {
            //Arrange
            RetrieverJob job = new RetrieverJob()
            {
                JobGuid = Guid.NewGuid()
            };
            JavaOptionsOverrideDto dto = new JavaOptionsOverrideDto();

            LivyBatch livyBatch = new LivyBatch()
            {
                Id = 11,
                State = "Success",
                Appid = "App Id",
                AppInfo = new System.Collections.Generic.Dictionary<string, string>() { { "driverLogUrl", "driver value" }, { "sparkUiUrl", "spark UI Url value"} }
            };

            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(livyBatch)),
                StatusCode = (isLivyCallSuccessful) ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest
            };

            Mock<IApacheLivyProvider> apacheProvider = new Mock<IApacheLivyProvider>();
            apacheProvider.Setup(s => s.PostRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(response);


            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.Add(It.IsAny<Submission>()));
            context.Setup(s => s.Add(It.IsAny<JobHistory>()));
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<JobService> jobService = new Mock<JobService>(context.Object, null, null, null, apacheProvider.Object) { CallBase = true };
            jobService.Setup(s => s.BuildLivyPostContent(dto, job)).Returns("content");
            jobService.Setup(s => s.MapToSubmission(It.IsAny<RetrieverJob>(), It.IsAny<JavaOptionsOverrideDto>())).Verifiable();
            jobService.Setup(s => s.GetClusterUrl(It.IsAny<JavaOptionsOverrideDto>())).Returns("http://awe-t-apspml-01:8999");

            
            Times jobHistoryAddCount = isLivyCallSuccessful ? Times.Once() : Times.Never();
            Times saveChancesCount = isLivyCallSuccessful ? Times.Exactly(2) : Times.Once();

            //Act
            _ = jobService.Object.SubmitApacheLivyJobInternalAsync(job, job.JobGuid, dto);

            //Assert
            jobService.VerifyAll();            
            context.Verify(v => v.Add(It.IsAny<Submission>()), Times.Once);
            context.Verify(v => v.Add(It.IsAny<JobHistory>()), jobHistoryAddCount);
            context.Verify(v => v.SaveChanges(It.IsAny<bool>()), saveChancesCount);
        }

        [TestMethod]
        public void GenerateUniqueLivySessionName()
        {
            //Arrange
            JavaAppSource javaAppSource = new JavaAppSource()
            {
                Name = "JavaSrc"
            };

            JobService jobService = new JobService(null, null, null, null, null);

            //Act
            string result = jobService.GenerateUniqueLivySessionName(javaAppSource);

            //Assert
            Assert.IsTrue(result.StartsWith($"{javaAppSource.Name}_"));
            Assert.AreEqual(14, result.Length);
        }

    }
}
