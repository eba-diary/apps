using Hangfire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Sentry.data.Core.DTO.Job;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Livy;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var JobService = new JobService(context.Object, null, jobManager.Object, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, null, null, null, null);

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

            var JobService = new JobService(context.Object, null, jobManager.Object, null, null, null);

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

            var JobService = new JobService(context.Object, null, jobManager.Object, null, null, null);

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
                Schedule = "* * * * *",
                PagingType = PagingType.Token,
                PageTokenField = "tokenField",
                PageParameterName = "token_param",
                RequestVariables = new List<RequestVariableDto>
                {
                    new RequestVariableDto
                    {
                        VariableName = "var",
                        VariableValue = "value",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.Setup(x => x.GetById<DataFlow>(3)).Returns(new DataFlow() { Id = 31 });
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

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
            Assert.AreEqual(1, job.RequestVariables.Count);

            RequestVariable requestVariable = job.RequestVariables.First();
            Assert.AreEqual("var", requestVariable.VariableName);
            Assert.AreEqual("value", requestVariable.VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.Daily, requestVariable.VariableIncrementType);

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
            Assert.AreEqual(PagingType.Token, httpsOptions.PagingType);
            Assert.AreEqual("tokenField", httpsOptions.PageTokenField);
            Assert.AreEqual("token_param", httpsOptions.PageParameterName);

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

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

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
                },
                RequestVariables = new List<RequestVariableDto>()
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
                    RelativeUri = "RelativeUri",
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions()
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

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

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_RequestVariablesMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>
                {
                    new RequestVariableDto
                    {
                        VariableName = "Name",
                        VariableValue = "Value",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>
                    {
                        new RequestVariable
                        {
                            VariableName = "Name",
                            VariableValue = "Value",
                            VariableIncrementType = RequestVariableIncrementType.Daily
                        }
                    },
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions()
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsTrue(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_RequestVariablesValueNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>
                {
                    new RequestVariableDto
                    {
                        VariableName = "Name",
                        VariableValue = "Value1",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>
                    {
                        new RequestVariable
                        {
                            VariableName = "Name",
                            VariableValue = "Value2",
                            VariableIncrementType = RequestVariableIncrementType.Daily
                        }
                    },
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions()
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_RequestVariablesNameNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>
                {
                    new RequestVariableDto
                    {
                        VariableName = "Name1",
                        VariableValue = "Value",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>
                    {
                        new RequestVariable
                        {
                            VariableName = "Name2",
                            VariableValue = "Value",
                            VariableIncrementType = RequestVariableIncrementType.Daily
                        }
                    },
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions()
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_RequestVariablesIncrementTypeNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>
                {
                    new RequestVariableDto
                    {
                        VariableName = "Name",
                        VariableValue = "Value",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>
                    {
                        new RequestVariable
                        {
                            VariableName = "Name",
                            VariableValue = "Value",
                            VariableIncrementType = RequestVariableIncrementType.None
                        }
                    },
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions()
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_RequestVariablesCountNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>
                {
                    new RequestVariableDto
                    {
                        VariableName = "Name",
                        VariableValue = "Value",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>
                    {
                        new RequestVariable
                        {
                            VariableName = "Name",
                            VariableValue = "Value",
                            VariableIncrementType = RequestVariableIncrementType.Daily
                        },
                        new RequestVariable
                        {
                            VariableName = "Name2",
                            VariableValue = "Value2",
                            VariableIncrementType = RequestVariableIncrementType.Daily
                        }
                    },
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions()
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_PagingTypeNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>(),
                PagingType = PagingType.PageNumber,
                PageParameterName = "Name",
                PageTokenField = null
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>(),
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions
                        {
                            PagingType = PagingType.Token,
                            PageParameterName = "Name",
                            PageTokenField = null
                        }
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_PagingParameterNameNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>(),
                PagingType = PagingType.PageNumber,
                PageParameterName = "Name",
                PageTokenField = null
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>(),
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions
                        {
                            PagingType = PagingType.PageNumber,
                            PageParameterName = "Name2",
                            PageTokenField = null
                        }
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_PagingTokenFieldNoMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>(),
                PagingType = PagingType.Token,
                PageParameterName = "Name",
                PageTokenField = "Token"
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>(),
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions
                        {
                            PagingType = PagingType.Token,
                            PageParameterName = "Name",
                            PageTokenField = "Token2"
                        }
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsFalse(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_WithExecutionParameters_PagingMatch()
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
                },
                RequestVariables = new List<RequestVariableDto>(),
                PagingType = PagingType.Token,
                PageParameterName = "Name",
                PageTokenField = "Token"
            };

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataFlow = new DataFlow() { SchemaId = 2 },
                    RelativeUri = "RelativeUri",
                    RequestVariables = new List<RequestVariable>(),
                    JobOptions = new RetrieverJobOptions
                    {
                        HttpOptions = new HttpsOptions
                        {
                            PagingType = PagingType.Token,
                            PageParameterName = "Name",
                            PageTokenField = "Token"
                        }
                    }
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(new HTTPSSource() { Id = 11 });
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(new FileSchema() { SchemaId = 21 });
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>()));

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.IsTrue(job.ExecutionParameters.Any());

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveRetrieverJob_PagingHTTPS_RetrieverJob()
        {
            RetrieverJobDto dto = new RetrieverJobDto()
            {
                DataSourceId = 1,
                FileSchema = 2,
                DataFlow = 3,
                RelativeUri = "RelativeUri",
                Schedule = "* * * * *",
                RequestMethod = HttpMethods.get,
                RequestDataFormat = HttpDataFormat.json,
                PagingType = PagingType.Token,
                PageTokenField = "tokenField",
                PageParameterName = "token",
                TargetFileName = "FileName"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            DataSource dataSource = new HTTPSSource() { Id = 1 };
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(dataSource);
            FileSchema fileSchema = new FileSchema() { SchemaId = 2 };
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(fileSchema);
            DataFlow dataFlow = new DataFlow() { Id = 3 };
            datasetContext.Setup(x => x.GetById<DataFlow>(3)).Returns(dataFlow);
            datasetContext.Setup(x => x.Add(It.IsAny<RetrieverJob>())).Callback<RetrieverJob>(x => x.Id = 4);

            JobService service = new JobService(datasetContext.Object, null, null, null, null, null);

            RetrieverJob job = service.CreateAndSaveRetrieverJob(dto);

            Assert.AreEqual(4, job.Id);
            Assert.IsNull(job.DatasetConfig);
            Assert.AreEqual(dataSource, job.DataSource);
            Assert.AreEqual(fileSchema, job.FileSchema);
            Assert.AreEqual(dataFlow, job.DataFlow);
            Assert.IsFalse(job.IsGeneric);
            Assert.AreNotEqual(Guid.Empty, job.JobGuid);
            Assert.AreEqual("RelativeUri", job.RelativeUri);
            Assert.AreEqual("* * * * *", job.Schedule);
            Assert.AreEqual("Central Standard Time", job.TimeZone);
            Assert.IsNull(job.DeleteIssuer);
            Assert.AreEqual(DateTime.MaxValue, job.DeleteIssueDTM);
            Assert.AreEqual(ObjectStatusEnum.Active, job.ObjectStatus);
            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.IsNotNull(job.JobOptions);

            RetrieverJobOptions jobOptions = job.JobOptions;
            Assert.IsTrue(jobOptions.IsRegexSearch);
            Assert.IsFalse(jobOptions.OverwriteDataFile);
            Assert.IsNull(jobOptions.SearchCriteria);
            Assert.IsFalse(jobOptions.CreateCurrentFile);
            Assert.AreEqual(FtpPattern.NoPattern, jobOptions.FtpPattern);
            Assert.AreEqual("FileName", jobOptions.TargetFileName);
            Assert.IsNotNull(jobOptions.CompressionOptions);
            Assert.IsNotNull(jobOptions.HttpOptions);

            HttpsOptions httpsOptions = jobOptions.HttpOptions;
            Assert.IsNull(httpsOptions.Body);
            Assert.AreEqual(HttpMethods.get, httpsOptions.RequestMethod);
            Assert.AreEqual(HttpDataFormat.json, httpsOptions.RequestDataFormat);
            Assert.AreEqual(PagingType.Token, httpsOptions.PagingType);
            Assert.AreEqual("tokenField", httpsOptions.PageTokenField);
            Assert.AreEqual("token", httpsOptions.PageParameterName);

            datasetContext.VerifyAll();
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

            JobService jobService = new JobService(null, null, null, null, null, null);

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

            JobService jobService = new JobService(null, null, null, null, null, null);

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

            JobService jobService = new JobService(null, null, null, null, null, null);

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

            JobService jobService = new JobService(null, null, null, null, null, null);

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

            JobService jobService = new JobService(null, null, null, null, null, null);

            //Act
            jobService.AddLivyArgumentsElement(builder_newValue, argsNewValue, argsDefaultValue);
            jobService.AddLivyArgumentsElement(builder_defaultValue, null, argsDefaultValue);

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
                ConfigurationParameters = "\"Override parameters\"",
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

            Mock<JobService> service = new Mock<JobService>(context.Object, null, null, features.Object, null, null) { CallBase = true };
            service.Setup(s => s.GenerateUniqueLivySessionName(DataSource)).Returns("session_name");

            //Act
            var result = service.Object.BuildLivyPostContent(javaOptionsOverrideDto, retrieverJob);

            //Assert
            Assert.AreEqual("{\"file\": \"com.something.file\", \"className\": \"awesome_class_name\", \"name\": \"session_name\", \"driverMemory\": \"22GB\", \"driverCores\": 22, \"executorMemory\": \"88GB\", \"executorCores\": 88, \"numExecutors\": 9999, \"conf\":\"Override parameters\", \"args\": [\"\"123\":7777\"], \"jars\": [\"jar1\",\"anotherjar2\"]}", result);
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
                        ConfigurationParameters = "\"config parameters\"",
                        Arguments = new string[] { "\"abc\":\"zyz\"" }
                    }
                }
            };

            context.Setup(s => s.GetById<JavaAppSource>(It.IsAny<int>())).Returns(DataSource);

            Mock<JobService> service = new Mock<JobService>(context.Object, null, null, features.Object, null, null) { CallBase = true };
            service.Setup(s => s.GenerateUniqueLivySessionName(DataSource)).Returns("session_name");

            //Act
            var result = service.Object.BuildLivyPostContent(javaOptionsOverrideDto, retrieverJob);

            //Assert
            Assert.AreEqual("{\"file\": \"com.something.file\", \"className\": \"awesome_class_name\", \"name\": \"session_name\", \"driverMemory\": \"1MB\", \"driverCores\": 1, \"executorMemory\": \"99MB\", \"executorCores\": 99, \"numExecutors\": 12345, \"conf\":\"config parameters\", \"args\": [\"\"abc\":\"zyz\"\"], \"jars\": [\"jar1\",\"anotherjar2\"]}", result);
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

            JobService service = new JobService(null, null, null, null, null, null);

            //Act
            var result = service.MapToSubmission(retrieverJob, javaOptionsOverrideDto);

            //Assert
            Assert.AreEqual(javaOptionsOverrideDto.ClusterUrl, result.ClusterUrl);
            Assert.AreEqual(javaOptionsOverrideDto.FlowExecutionGuid, result.FlowExecutionGuid);
            Assert.AreEqual(javaOptionsOverrideDto.RunInstanceGuid, result.RunInstanceGuid);
            Assert.AreEqual(retrieverJob, result.JobId);
            Assert.AreEqual(retrieverJob.JobGuid, result.JobGuid);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public async Task SubmitApacheLivyJobInternalAsync_LivyCallSuccessful()
        {
            //Arrange
            RetrieverJob job = new RetrieverJob()
            {
                Id = 22,
                JobGuid = Guid.NewGuid()
            };

            JavaOptionsOverrideDto dto = new JavaOptionsOverrideDto();

            LivyBatch livyBatch = new LivyBatch()
            {
                Id = 11,
                State = "Success",
                Appid = "App Id",
                AppInfo = new System.Collections.Generic.Dictionary<string, string>() { { "driverLogUrl", "driver value" }, { "sparkUiUrl", "spark UI Url value" } }
            };

            Submission sub = new Submission()
            {
                JobId = job,
                JobGuid = job.JobGuid,
                Created = DateTime.Now,
                FlowExecutionGuid = "",
                RunInstanceGuid = "",
                ClusterUrl = ""
            };

            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(livyBatch)),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            Mock<IApacheLivyProvider> apacheProvider = new Mock<IApacheLivyProvider>();
            apacheProvider.Setup(s => s.PostRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(response);


            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.Add(It.IsAny<Submission>()));
            context.Setup(s => s.Add(It.IsAny<JobHistory>()));
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<JobService> jobService = new Mock<JobService>(context.Object, null, null, null, apacheProvider.Object, null) { CallBase = true };
            jobService.Setup(s => s.BuildLivyPostContent(dto, job)).Returns("content");
            jobService.Setup(s => s.GetClusterUrl(It.IsAny<JavaOptionsOverrideDto>())).Returns("http://awe-t-apspml-01:8999");


            Times jobHistoryAddCount = Times.Once();
            Times saveChancesCount = Times.Exactly(2);

            //Act
            _ = await jobService.Object.SubmitApacheLivyJobInternalAsync(job, job.JobGuid, dto);

            //Assert
            jobService.VerifyAll();

            context.Verify(v => v.Add(It.IsAny<Submission>()), Times.Once);
            context.Verify(v => v.Add(It.IsAny<JobHistory>()), jobHistoryAddCount);
            context.Verify(v => v.SaveChanges(It.IsAny<bool>()), saveChancesCount);
        }

        [TestCategory("Core JobService")]
        [TestMethod]
        public async Task SubmitApacheLivyJobInternalAsync_LivyCall_Unsuccessful()
        {
            //Arrange
            RetrieverJob job = new RetrieverJob()
            {
                Id = 22,
                JobGuid = Guid.NewGuid()
            };

            JavaOptionsOverrideDto dto = new JavaOptionsOverrideDto();

            LivyBatch livyBatch = new LivyBatch()
            {
                Id = 11,
                State = "Success",
                Appid = "App Id",
                AppInfo = new System.Collections.Generic.Dictionary<string, string>() { { "driverLogUrl", "driver value" }, { "sparkUiUrl", "spark UI Url value" } }
            };

            Submission sub = new Submission()
            {
                JobId = job,
                JobGuid = job.JobGuid,
                Created = DateTime.Now,
                FlowExecutionGuid = "",
                RunInstanceGuid = "",
                ClusterUrl = ""
            };

            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(livyBatch)),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

            Mock<IApacheLivyProvider> apacheProvider = new Mock<IApacheLivyProvider>();
            apacheProvider.Setup(s => s.PostRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(response);


            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.Add(It.IsAny<Submission>()));
            context.Setup(s => s.Add(It.IsAny<JobHistory>()));
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<JobService> jobService = new Mock<JobService>(context.Object, null, null, null, apacheProvider.Object, null) { CallBase = true };
            jobService.Setup(s => s.BuildLivyPostContent(dto, job)).Returns("content");
            jobService.Setup(s => s.GetClusterUrl(It.IsAny<JavaOptionsOverrideDto>())).Returns("http://awe-t-apspml-01:8999");


            Times jobHistoryAddCount = Times.Never();
            Times saveChancesCount = Times.Once();

            //Act
            _ = await jobService.Object.SubmitApacheLivyJobInternalAsync(job, job.JobGuid, dto);

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

            JobService jobService = new JobService(null, null, null, null, null, null);

            //Act
            string result = jobService.GenerateUniqueLivySessionName(javaAppSource);

            //Assert
            Assert.IsTrue(result.StartsWith($"{javaAppSource.Name}_"));
            Assert.AreEqual(14, result.Length);
        }

        [TestMethod]
        public async Task GetApacheLivyBatchStatusInternalAsync_BatchNotFound()
        {
            Guid jobGuid = Guid.NewGuid();
            string flowExecutionGuid = "10000001";
            string runInstanceGuid = "00000000";

            JobHistory jobHistory = new JobHistory()
            {
                HistoryId = 123,
                JobId = new RetrieverJob()
                {
                    Id = 999,
                    JobGuid = jobGuid
                },
                JobGuid = jobGuid,
                BatchId = 777,
                Submission = new Submission()
                {
                    FlowExecutionGuid = flowExecutionGuid,
                    RunInstanceGuid = runInstanceGuid
                },
                Active = true
            };

            var responseContent = $"{{ \"msg\":\"Session '777' not found.\"}}";

            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage()
            {
                Content = new System.Net.Http.StringContent(JsonConvert.SerializeObject(responseContent)),
                StatusCode = System.Net.HttpStatusCode.NotFound
            };

            Mock<IApacheLivyProvider> apacheProvider = new Mock<IApacheLivyProvider>();
            apacheProvider.Setup(x => x.GetRequestAsync(It.IsAny<string>())).Returns(Task.FromResult(response));

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<JobService> jobService = new Mock<JobService>(context.Object, null, null, null, apacheProvider.Object, null) { CallBase = true };
            jobService.Setup(s => s.GetClusterUrl(It.IsAny<JobHistory>())).Returns("abc.com");

            _ = await jobService.Object.GetApacheLivyBatchStatusAsync(jobHistory);

            context.Verify(v => v.SaveChanges(It.IsAny<bool>()), Times.Once);

        }

        [TestMethod]
        public async Task GetApacheLivyBatchStatusAsync_No_History_Record()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<JobHistory> jobHistoryRecords = new List<JobHistory>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.JobHistory).Returns(jobHistoryRecords.AsQueryable());

            JobService jobService = new JobService(context.Object, null, null, null, null, null);

            _ = await jobService.GetApacheLivyBatchStatusAsync(1, 1);

            mr.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetApacheLivyBatchStatusAsync_JobId_Required()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<JobHistory> jobHistoryRecords = new List<JobHistory>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.JobHistory).Returns(jobHistoryRecords.AsQueryable());

            JobService jobService = new JobService(context.Object, null, null, null, null, null);

            _ = await jobService.GetApacheLivyBatchStatusAsync(0, 1);

            mr.VerifyAll();
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetApacheLivyBatchStatusAsync_BatchId_Required()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<JobHistory> jobHistoryRecords = new List<JobHistory>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.JobHistory).Returns(jobHistoryRecords.AsQueryable());

            JobService jobService = new JobService(context.Object, null, null, null, null, null);

            _ = await jobService.GetApacheLivyBatchStatusAsync(1, 0);

            mr.VerifyAll();
        }

        [TestMethod]
        public void MaptoJobHistory()
        {
            Guid jobGuid = Guid.NewGuid();
            string flowExecutionGuid = "10000001";
            string runInstanceGuid = "00000000";

            JobHistory jobHistory = new JobHistory()
            {
                HistoryId = 123,
                JobId = new RetrieverJob()
                {
                    Id = 999,
                    JobGuid = jobGuid
                },
                JobGuid = jobGuid,
                BatchId = 777,
                Submission = new Submission()
                {
                    FlowExecutionGuid = flowExecutionGuid,
                    RunInstanceGuid = runInstanceGuid
                },
                Active = true
            };

            string livyResponse = @"{
            ""id"": 1034,
            ""name"": ""CloudAnalyticsApp01_D9KYX0"",
            ""owner"": null,
            ""proxyUser"": null,
            ""state"": ""dead"",
            ""appId"": ""application_1657659422545_1035"",
            ""appInfo"": {
                ""driverLogUrl"": ""http://ip-10-84-88-142.us-east-2.compute.internal:8188/applicationhistory/logs/ip-10-84-88-226.us-east-2.compute.internal:8041/container_1657659422545_1035_02_000001/container_1657659422545_1035_02_000001/livy"",
                ""sparkUiUrl"": ""http://ip-10-84-88-142.us-east-2.compute.internal:20888/proxy/application_1657659422545_1035/""
            },
            ""log"": [
                ""\t start time: 1664900013804"",
                ""\t final status: UNDEFINED"",
                ""\t tracking URL: http://ip-10-84-88-142.us-east-2.compute.internal:20888/proxy/application_1657659422545_1035/"",
                ""\t user: livy"",
                ""22/10/04 16:13:33 INFO ShutdownHookManager: Shutdown hook called"",
                ""22/10/04 16:13:33 INFO ShutdownHookManager: Deleting directory /mnt/tmp/spark-6d126217-a18b-4add-9ac7-46b9852c91ab"",
                ""22/10/04 16:13:33 INFO ShutdownHookManager: Deleting directory /mnt/tmp/spark-5baf2392-8fe3-42be-97ee-4df71c104f1b"",
                ""\nstderr: "",
                ""\nYARN Diagnostics: "",
                ""Shutdown hook called before final status was reported.""
            ]
            }";

            var lr = JsonConvert.DeserializeObject<LivyReply>(livyResponse);

            JobService jobService = new JobService(null, null, null, null, null, null);

            JobHistory result = jobService.MapToJobHistory(jobHistory, lr);

            Assert.IsNotNull(result.LogInfo);
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_Null_NamedEnvironmentTypeFeatureOff_DfsMonitorDtos()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("NonProd");

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsDataFlowBasic() { BaseUri = new Uri(@"c:\tmp\DatasetLoader\") },
                    DataFlow = new DataFlow() { FlowStorageCode = "000001" }
                },
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsDataFlowBasic()
                },
                new RetrieverJob()
                {
                    Id = 3,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new HTTPSSource()
                }
            };

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());

            JobService service = new JobService(datasetContext.Object, null, null, dataFeatures.Object, null, null);

            List<DfsMonitorDto> results = service.GetDfsRetrieverJobs(null);

            Assert.AreEqual(1, results.Count);

            DfsMonitorDto dto = results.First();
            Assert.AreEqual(1, dto.JobId);
            Assert.AreEqual(@"c:\tmp\DatasetLoader\000001", dto.MonitorTarget);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_Null_NamedEnvironmentTypeFeatureOn_DfsMonitorDtos()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    DataSource = new DfsDataFlowBasic() { BaseUri = new Uri(@"c:\tmp\DatasetLoader\") },
                    DataFlow = new DataFlow() { FlowStorageCode = "000001" }
                },
                new RetrieverJob()
                {
                    Id = 2,
                    RelativeUri = @"SAID\DEV\000002",
                    DataSource = new DfsNonProdSource() {BaseUri = new Uri(@"c:\tmp\nonprod\") }
                }
            };

            Mock<IDfsRetrieverJobProvider> dfsRetrieverJobProvider = mockRepository.Create<IDfsRetrieverJobProvider>();
            dfsRetrieverJobProvider.Setup(x => x.GetDfsRetrieverJobs(null)).Returns(jobs);

            JobService service = new JobService(null, null, null, dataFeatures.Object, null, dfsRetrieverJobProvider.Object);

            List<DfsMonitorDto> results = service.GetDfsRetrieverJobs(null);

            Assert.AreEqual(2, results.Count);

            DfsMonitorDto dto = results.First();
            Assert.AreEqual(1, dto.JobId);
            Assert.AreEqual(@"c:\tmp\DatasetLoader\000001", dto.MonitorTarget);

            dto = results.Last();
            Assert.AreEqual(2, dto.JobId);
            Assert.AreEqual(@"c:\tmp\nonprod\SAID\DEV\000002", dto.MonitorTarget);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_DEV_NamedEnvironmentTypeFeatureOn_DfsMonitorDtos()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    DataSource = new DfsDataFlowBasic() { BaseUri = new Uri(@"c:\tmp\DatasetLoader\") },
                    DataFlow = new DataFlow() { FlowStorageCode = "000001" }
                },
                new RetrieverJob()
                {
                    Id = 2,
                    RelativeUri = @"SAID\DEV\000002",
                    DataSource = new DfsNonProdSource() {BaseUri = new Uri(@"c:\tmp\nonprod\") }
                }
            };

            Mock<IDfsRetrieverJobProvider> dfsRetrieverJobProvider = mockRepository.Create<IDfsRetrieverJobProvider>();
            dfsRetrieverJobProvider.Setup(x => x.GetDfsRetrieverJobs("DEV")).Returns(jobs);
            dfsRetrieverJobProvider.SetupGet(x => x.AcceptedNamedEnvironments).Returns(new List<string>() { "DEV" });

            JobService service = new JobService(null, null, null, dataFeatures.Object, null, dfsRetrieverJobProvider.Object);

            List<DfsMonitorDto> results = service.GetDfsRetrieverJobs("DEV");

            Assert.AreEqual(2, results.Count);

            DfsMonitorDto dto = results.First();
            Assert.AreEqual(1, dto.JobId);
            Assert.AreEqual(@"c:\tmp\DatasetLoader\000001", dto.MonitorTarget);

            dto = results.Last();
            Assert.AreEqual(2, dto.JobId);
            Assert.AreEqual(@"c:\tmp\nonprod\SAID\DEV\000002", dto.MonitorTarget);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_DEV_NamedEnvironmentTypeFeatureOn_DfsRetrieverJobException()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            Mock<IDfsRetrieverJobProvider> dfsRetrieverJobProvider = mockRepository.Create<IDfsRetrieverJobProvider>();
            dfsRetrieverJobProvider.SetupGet(x => x.AcceptedNamedEnvironments).Returns(new List<string>() { "TEST", "NRTEST" });

            JobService service = new JobService(null, null, null, dataFeatures.Object, null, dfsRetrieverJobProvider.Object);

            Assert.ThrowsException<DfsRetrieverJobException>(() => service.GetDfsRetrieverJobs("DEV"), "The requesting named environment 'DEV' is not an accepted named environment (TEST, NRTEST).");

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void InstantiateJobsForCreation_DfsNonProdSource()
        {
            JobService service = new JobService(null, null, null, null, null, null);

            DataFlow dataFlow = new DataFlow()
            {
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                FlowStorageCode = "000001"
            };

            DataSource dataSource = new DfsNonProdSource();

            RetrieverJob result = service.InstantiateJobsForCreation(dataFlow, dataSource);

            Assert.AreEqual("Instant", result.Schedule);
            Assert.AreEqual("SAID/DEV/000001", result.RelativeUri);
        }

        [TestMethod]
        public void InstantiateJobsForCreation_DfsProdSource()
        {
            JobService service = new JobService(null, null, null, null, null, null);

            DataFlow dataFlow = new DataFlow()
            {
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                FlowStorageCode = "000001"
            };

            DataSource dataSource = new DfsProdSource();

            RetrieverJob result = service.InstantiateJobsForCreation(dataFlow, dataSource);

            Assert.AreEqual("Instant", result.Schedule);
            Assert.AreEqual("SAID/DEV/000001", result.RelativeUri);
        }
    }
}
