using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.FeatureFlags.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowServiceTests : BaseCoreUnitTest
    {
        /// <summary>
        /// - Test that the DataFlowService.Validate() method correctly identifies a duplicate DataFlow name
        /// and responds with the correct validation result.
        /// </summary>
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public async Task DataFlowService_Validate_DuplicateName_NoNamedEnvironments()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new[] { new DataFlow() { Name = "Foo" } };
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterService.Object, null, null);
            var dataFlow = new DataFlowDto() { Name = "Foo" };

            // Act
            var result = await dataFlowService.ValidateAsync(dataFlow);

            // Assert
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);
            Assert.IsTrue(result.ValidationResults.Contains(DataFlow.ValidationErrors.nameMustBeUnique));
        }

        /// <summary>
        /// Tests successful validation of the DataFlowDto
        /// </summary>
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public async Task DataFlowService_Validate_Success()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new DataFlow[0];
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterService.Object, null, null);
            var dataFlow = new DataFlowDto() { Name = "Bar", NamedEnvironment = "TEST", NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.NonProd };

            // Act
            var result = await dataFlowService.ValidateAsync(dataFlow);

            // Assert
            Assert.AreEqual(0, result.ValidationResults.GetAll().Count);
        }


        #region ProducerS3Drop Tests

        [TestMethod]
        public void DataActionQueryExtensions_GetProducerS3DropAction_HRDropLocation()
        {

            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ProducerS3DropAction).Returns(MockClasses.MockProducerS3DropActions().AsQueryable());

            // Act
            var result = context.Object.ProducerS3DropAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.AreEqual(20, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetProducerS3DropAction_DLSTDropLocation()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ProducerS3DropAction).Returns(MockClasses.MockProducerS3DropActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.ProducerS3DropAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(15, result.Id);
        }

        #endregion

        #region RawStorage Tests

        [TestMethod]
        public void DataActionQueryExtensions_GetRawStorageAction_HRRawStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.RawStorageAction).Returns(MockClasses.MockRawStorageActions().AsQueryable());

            // Act
            var result = context.Object.RawStorageAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.AreEqual(16, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetRawStorageAction_DlstRawStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.RawStorageAction).Returns(MockClasses.MockRawStorageActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.RawStorageAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(22, result.Id);
        }
        #endregion

        #region QueryStorage Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetQueryStorageAction_HRQueryStorage()
        {            
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.QueryStorageAction).Returns(MockClasses.MockQueryStorageActions().AsQueryable());

            // Act
            var result = context.Object.QueryStorageAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.AreEqual(17, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetQueryStorageAction_DlstQueryStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.QueryStorageAction).Returns(MockClasses.MockQueryStorageActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.QueryStorageAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(23, result.Id);
        }
        #endregion

        #region ConvertToParquet Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetConvertToParquetAction_HRConvertToParquet()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ConvertToParquetAction).Returns(MockClasses.MockConvertToParquetActions().AsQueryable());

            // Act
            var result = context.Object.ConvertToParquetAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.AreEqual(19, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetConvertToParquetAction_DlstConvertToParquet()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ConvertToParquetAction).Returns(MockClasses.MockConvertToParquetActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.ConvertToParquetAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(24, result.Id);
        }
        #endregion

        #region UncompressZip Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetUncompressZipAction_HRUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressZipAction).Returns(MockClasses.MockUncompressZipActions().AsQueryable());

            // Act
            //var result = context.Object.UncompressZipAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.UncompressZipAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetUncompressZipAction_DlstUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressZipAction).Returns(MockClasses.MockUncompressZipActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.UncompressZipAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(25, result.Id);
        }
        #endregion

        #region GoogleApi Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetGoogleApiAction_HRUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.GoogleApiAction).Returns(MockClasses.MockGoogleApiActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.GoogleApiAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetGoogleApiAction_DlstUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.GoogleApiAction).Returns(MockClasses.MockGoogleApiActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.GoogleApiAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(26, result.Id);
        }
        #endregion

        #region ClaimIQ Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetClaimIQAction_HRClaimIQ()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ClaimIQAction).Returns(MockClasses.MockClaimIQActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.ClaimIQAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetClaimIQAction_DlstClaimIQ()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ClaimIQAction).Returns(MockClasses.MockClaimIQActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.ClaimIQAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(27, result.Id);
        }
        #endregion

        #region UncompressGzip Tests 
        [TestMethod]
        public void DataActionQueryExtensions_GetUncompressGzipAction_HRUncompressGzip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressGzipAction).Returns(MockClasses.MockUncompressGzipActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.UncompressGzipAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetUncompressGzipAction_DlstUncompressGzip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressGzipAction).Returns(MockClasses.MockUncompressGzipActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.UncompressGzipAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(28, result.Id);
        }
        #endregion

        #region FixedWidth Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetFixedWidthAction_HRFixedWidth()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.FixedWidthAction).Returns(MockClasses.MockFixedWidthActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.FixedWidthAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetFixedWidthAction_DlstFixedWidth()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.FixedWidthAction).Returns(MockClasses.MockFixedWidthActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.FixedWidthAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(29, result.Id);
        }
        #endregion

        #region XML Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetXmlAction_HRXml()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.XMLAction).Returns(MockClasses.MockXmlActions().AsQueryable());

            // Act
            var result = context.Object.XMLAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.AreEqual(21, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetXmlAction_DlstXml()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.XMLAction).Returns(MockClasses.MockXmlActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.XMLAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(30, result.Id);
        }

        #endregion

        #region JsonFlattening Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetJsonFlatteningAction_HRJsonFlattening()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.JsonFlatteningAction).Returns(MockClasses.MockJsonFlatteningActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.JsonFlatteningAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetJsonFlatteningAction_DlstJsonFlattening()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.JsonFlatteningAction).Returns(MockClasses.MockJsonFlatteningActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.JsonFlatteningAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(31, result.Id);
        }

        #endregion

        #region SchemaMap Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetSchemaMapAction_HRSchemaMap()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaMapAction).Returns(MockClasses.MockSchemaMapActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.SchemaMapAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetSchemaMapAction_DlstSchemaMap()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaMapAction).Returns(MockClasses.MockSchemaMapActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.SchemaMapAction.GetAction(new MockDataFeatures(), true));
        }

        #endregion

        #region S3Drop Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetS3DropAction_HRS3Drop()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.S3DropAction).Returns(MockClasses.MockS3DropActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.S3DropAction.GetAction(new MockDataFeatures(), true));
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetS3DropAction_DlstS3Drop()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.S3DropAction).Returns(MockClasses.MockS3DropActions().AsQueryable());

            // Assert
            Assert.ThrowsException<DataFlowStepNotImplementedException>(() => context.Object.S3DropAction.GetAction(new MockDataFeatures(), true));
        }

        #endregion

        #region SchemaLoad Tests
        [TestMethod]
        public void DataActionQueryExtensions_GetSchemaLoadAction_HRSchemaLoading()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaLoadAction).Returns(MockClasses.MockSchemaLoadActions().AsQueryable());

            // Act
            var result = context.Object.SchemaLoadAction.GetAction(new MockDataFeatures(), true);

            // Assert
            Assert.AreEqual(18, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetSchemaLoadAction_DlstSchemaLoading()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaLoadAction).Returns(MockClasses.MockSchemaLoadActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();

            // Act
            var result = context.Object.SchemaLoadAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(32, result.Id);
        }

        #endregion

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void CheckForDeleteDataFlow_ShouldBeEnqueued_OnlyOnce()
        {
            // Arrange
            var client = new Mock<IBackgroundJobClient>();

            var dataflowService = new DataFlowService(null, null, null, null, null, null, null, client.Object);

            // Act
            dataflowService.Delete_Queue(new List<int>() { 1 }, "123456", true);

            // Assert
            client.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "Delete" && (int)job.Args[0] == 1),
                It.IsAny<EnqueuedState>()), Times.Once);
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DataFlow_Delete_DataFlowNotFound_Is_Thrown()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow = new DataFlow()
            {
                Id = 1,
                Name = "FileSchemaFlow_TestFlow",
                ObjectStatus = ObjectStatusEnum.Active
            };

            var user = new Mock<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);

            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Assert
            Assert.ThrowsException<DataFlowNotFound>(() => dataflowService.Delete(2, user.Object, false));
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void Delete_Sets_Object_Metadata_Correctly_When_Incoming_Object_Active_And_LogicalDelete_False()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow = new DataFlow()
            {
                Id = 1,
                Name = "FileSchemaFlow_TestFlow",
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue
            };
            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);

            var user = new Mock<IApplicationUser>();
            user.Setup(x => x.AssociateId).Returns("123456");

            //We need to mock out IJobService due to call to DeleteJobByDataFlowId()
            var jobService = new Mock<IJobService>();

            var dataflowService = new DataFlowService(context.Object, null, jobService.Object, null, null, null, null, null);

            //Act
            dataflowService.Delete(1, user.Object, false);

            //Assert
            DataFlow deletedFlow = context.Object.GetById<DataFlow>(1);

            Assert.AreEqual(ObjectStatusEnum.Deleted, deletedFlow.ObjectStatus);
            Assert.AreEqual("123456", deletedFlow.DeleteIssuer);
            Assert.AreNotEqual(DateTime.MaxValue, deletedFlow.DeleteIssueDTM);
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void Delete_Does_Not_Call_Save_Changes()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");
            user.Setup(s => s.AssociateId).Returns("123456");
           

            DataFlow df = MockClasses.MockDataFlow();
            RetrieverJob job = MockClasses.GetMockRetrieverJob(
                                        MockClasses.MockDataFileConfig(
                                                MockClasses.MockDataset()), new FtpSource());
            job.DataFlow = df;
            List<RetrieverJob> jobList = new List<RetrieverJob>() { job };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<DataFlow>(It.IsAny<int>())).Returns(df);
            context.Setup(s => s.RetrieverJob).Returns(jobList.AsQueryable());

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(s => s.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var dataFlowService = new DataFlowService(context.Object, null, jobService.Object, null, null, null, null, null);
            
            //Act
            dataFlowService.Delete(df.Id, user.Object, true);

            //Assert
            context.Verify(x => x.SaveChanges(true), Times.Never);
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void Delete_Passes_Null_User_Info_To_JobService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");
            user.Setup(s => s.AssociateId).Returns("123456");


            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Pending_Delete;
            df.DeleteIssuer = "654321";

            RetrieverJob job = MockClasses.GetMockRetrieverJob(
                                        MockClasses.MockDataFileConfig(
                                                MockClasses.MockDataset()), new FtpSource());
            job.DataFlow = df;
            List<RetrieverJob> jobList = new List<RetrieverJob>() { job };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<DataFlow>(It.IsAny<int>())).Returns(df);
            context.Setup(s => s.RetrieverJob).Returns(jobList.AsQueryable());

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var dataFlowService = new DataFlowService(context.Object, null, jobService.Object, null, null, null, null, null);

            //Act
            //Using this syntax to ensure correct delete overload gets called
            dataFlowService.Delete(id:df.Id, user:null, logicalDelete:false);

            //Assert
            jobService.Verify(v => v.Delete(It.IsAny<List<int>>(), null, false), Times.Once);
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void Delete_Passes_Incoming_User_Info_To_JobService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");
            user.Setup(s => s.AssociateId).Returns("123456");


            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Pending_Delete;
            df.DeleteIssuer = "654321";

            RetrieverJob job = MockClasses.GetMockRetrieverJob(
                                        MockClasses.MockDataFileConfig(
                                                MockClasses.MockDataset()), new FtpSource());
            job.DataFlow = df;
            List<RetrieverJob> jobList = new List<RetrieverJob>() { job };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<DataFlow>(It.IsAny<int>())).Returns(df);
            context.Setup(s => s.RetrieverJob).Returns(jobList.AsQueryable());

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var dataFlowService = new DataFlowService(context.Object, null, jobService.Object, null, null, null, null, null);

            //Act
            //Using this syntax to ensure correct delete overload gets called
            dataFlowService.Delete(id: df.Id, user: user.Object, logicalDelete: false);

            //Assert
            jobService.Verify(v => v.Delete(It.IsAny<List<int>>(), user.Object, false), Times.Once);
        }
    }
}
