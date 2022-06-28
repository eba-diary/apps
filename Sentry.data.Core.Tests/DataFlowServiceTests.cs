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
using System.Linq.Expressions;
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
            var result = await dataFlowService.Validate(dataFlow);

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
            var result = await dataFlowService.Validate(dataFlow);

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
            ((MockBooleanFeatureFlag)dataFeatures.CLA3240_UseDropLocationV2).MockValue = true;

            // Act
            var result = context.Object.ProducerS3DropAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(15, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetProducerS3DropAction_DATADropLocation()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ProducerS3DropAction).Returns(MockClasses.MockProducerS3DropActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3240_UseDropLocationV2).MockValue = false;

            // Act
            var result = context.Object.ProducerS3DropAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(12, result.Id);
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
        public void DataActionQueryExtensions_GetRawStorageAction_DataRawStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.RawStorageAction).Returns(MockClasses.MockRawStorageActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.RawStorageAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(2, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetRawStorageAction_DlstRawStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.RawStorageAction).Returns(MockClasses.MockRawStorageActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetQueryStorageAction_DataQueryStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.QueryStorageAction).Returns(MockClasses.MockQueryStorageActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.QueryStorageAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(3, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetQueryStorageAction_DlstQueryStorage()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.QueryStorageAction).Returns(MockClasses.MockQueryStorageActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetConvertToParquetAction_DataConvertToParquet()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ConvertToParquetAction).Returns(MockClasses.MockConvertToParquetActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.ConvertToParquetAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(6, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetConvertToParquetAction_DlstConvertToParquet()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ConvertToParquetAction).Returns(MockClasses.MockConvertToParquetActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetUncompressZipAction_DataUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressZipAction).Returns(MockClasses.MockUncompressZipActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.UncompressZipAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(5, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetUncompressZipAction_DlstUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressZipAction).Returns(MockClasses.MockUncompressZipActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetGoogleApiAction_DataUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.GoogleApiAction).Returns(MockClasses.MockGoogleApiActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.GoogleApiAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(8, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetGoogleApiAction_DlstUncompressZip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.GoogleApiAction).Returns(MockClasses.MockGoogleApiActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetClaimIQAction_DataClaimIQ()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ClaimIQAction).Returns(MockClasses.MockClaimIQActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.ClaimIQAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(9, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetClaimIQAction_DlstClaimIQ()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.ClaimIQAction).Returns(MockClasses.MockClaimIQActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetUncompressGzipAction_DataUncompressGzip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressGzipAction).Returns(MockClasses.MockUncompressGzipActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.UncompressGzipAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(10, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetUncompressGzipAction_DlstUncompressGzip()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.UncompressGzipAction).Returns(MockClasses.MockUncompressGzipActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetFixedWidthAction_DataFixedWidth()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.FixedWidthAction).Returns(MockClasses.MockFixedWidthActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.FixedWidthAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(11, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetFixedWidthAction_DlstFixedWidth()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.FixedWidthAction).Returns(MockClasses.MockFixedWidthActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetXmlAction_DataXml()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.XMLAction).Returns(MockClasses.MockXmlActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.XMLAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(13, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetXmlAction_DlstXml()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.XMLAction).Returns(MockClasses.MockXmlActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetJsonFlatteningAction_DataJsonFlattening()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.JsonFlatteningAction).Returns(MockClasses.MockJsonFlatteningActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.JsonFlatteningAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(14, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetJsonFlatteningAction_DlstJsonFlattening()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.JsonFlatteningAction).Returns(MockClasses.MockJsonFlatteningActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

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
        public void DataActionQueryExtensions_GetSchemaMapAction_DataSchemaMap()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaMapAction).Returns(MockClasses.MockSchemaMapActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.SchemaMapAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(7, result.Id);
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
        public void DataActionQueryExtensions_GetS3DropAction_DataS3Drop()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.S3DropAction).Returns(MockClasses.MockS3DropActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.S3DropAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(1, result.Id);
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
        public void DataActionQueryExtensions_GetSchemaLoadAction_DataSchemaLoading()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaLoadAction).Returns(MockClasses.MockSchemaLoadActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = false;

            // Act
            var result = context.Object.SchemaLoadAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(4, result.Id);
        }

        [TestMethod]
        public void DataActionQueryExtensions_GetSchemaLoadAction_DlstSchemaLoading()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.SchemaLoadAction).Returns(MockClasses.MockSchemaLoadActions().AsQueryable());

            var dataFeatures = new MockDataFeatures();
            ((MockBooleanFeatureFlag)dataFeatures.CLA3332_ConsolidatedDataFlows).MockValue = true;

            // Act
            var result = context.Object.SchemaLoadAction.GetAction(dataFeatures, false);

            // Assert
            Assert.AreEqual(32, result.Id);
        }

        #endregion
        /*
         *  Unit test looking at the implementation of getDataFlowDtoByStepId
         */
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void testGetDataFlowDtoByStepId()
        {
            // Arrange
            int stepId = 2;

            var context = new Mock<IDatasetContext>();
            var datafeature = new Mock<IDataFeatures>();

            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Active;
            df.Id = 1;
            df.DatasetId = 1;
            df.SchemaId = 1;

            DataFlow df2 = MockClasses.MockDataFlow();
            df2.ObjectStatus = ObjectStatusEnum.Active;
            df2.Id = 2;
            df2.DatasetId = 2;
            df2.SchemaId = 2;

            DataFlowStep step1 = new DataFlowStep()
            {
                Id = 1,
                Action = new ProducerS3DropAction(),
                DataFlow = df
            };

            DataFlowStep step2 = new DataFlowStep()
            {
                Id = stepId,
                Action = new ProducerS3DropAction(),
                DataFlow = df2
            };

            df.Steps = new[] { step1 };
            df2.Steps = new[] { step2 };

            // created two dataflow mock classes and two associated dataflowsteps

            var dataflows = new[] { df, df2 };
            var dataflowsteps = new[] { step1, step2 };

            context.SetupGet(f => f.DataFlow).Returns(dataflows.AsQueryable);
            context.SetupGet(f => f.DataFlowStep).Returns(dataflowsteps.AsQueryable);

            datafeature.Setup(f => f.CLA3332_ConsolidatedDataFlows.GetValue()).Returns(true);

            var dataflowservice = new DataFlowService(context.Object, null, null, null, null, null, datafeature.Object, null); // creating the dataflowservice object

            // Act
            var result = dataflowservice.GetDataFlowDtoByStepId(stepId).Id;// this creates a nullReferenceException  -> gets the step Id from the currrent dataflowservice object
            

            // Assert
            Assert.AreEqual(step2.Id, result);
        }

        /*
         * Unit test looking at the implementation of getSchemaIdFromDatafileId
         */
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void testGetSchemaIdFromDatafileId()
        {
            // Arrange
            int datafileId = 3;
            int schemaId = 3;

            FileSchema schema = new FileSchema()
            {
                SchemaId = schemaId,
            };

            DatasetFile temp = new DatasetFile();
            temp.DatasetFileId = datafileId;
            temp.Schema = schema;

            var datasetfiles = new[] {temp };

            var context = new Mock<IDatasetContext>();
            context.SetupGet(f => f.DatasetFileStatusActive).Returns(datasetfiles.AsQueryable);

            var dataflowservice = new DataFlowService(context.Object, null, null, null, null, null, null, null);
            
            // Act
            int testSchema = dataflowservice.GetSchemaIdFromDatasetFileId(datafileId);
            
            // Assert
            Assert.AreEqual(schemaId, testSchema);
        }

        /*
         * Unit test looking at the implementation of ValidateStepIdAndDatasetFileIds
         */
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void testValidateStepIdAndDatasetFileIds()
        {
            // Arrange
            List<int> datasetFileIds = new List<int> { 3, 3, 3};
            int stepId = 3;

            // creating 3 datasetfiles with the same associated schema/schemaid
            FileSchema schema = new FileSchema()
            {
                SchemaId = 3,
            };

            DatasetFile dsf1 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 1
            };
            DatasetFile dsf2 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 2
            };
            DatasetFile dsf3 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 3
            };

            var datasetfiles = new[] { dsf1, dsf2, dsf3 };

            var context = new Mock<IDatasetContext>();
            var datafeature = new Mock<IDataFeatures>();

            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Active;
            df.Id = 1;
            df.DatasetId = 1;
            df.SchemaId = 1;

            DataFlow df2 = MockClasses.MockDataFlow();
            df2.ObjectStatus = ObjectStatusEnum.Active;
            df2.Id = 3;
            df2.DatasetId = 3;
            df2.SchemaId = 3;

            DataFlowStep step1 = new DataFlowStep()
            {
                Id = 1,
                Action = new ProducerS3DropAction(),
                DataFlow = df
            };

            DataFlowStep step2 = new DataFlowStep()
            {
                Id = stepId,
                Action = new ProducerS3DropAction(),
                DataFlow = df2
            };

            df.Steps = new[] { step1 };
            df2.Steps = new[] { step2 };

            // created two dataflow mock classes and two associated dataflowsteps

            var dataflows = new[] { df, df2 };
            var dataflowsteps = new[] { step1, step2 };

            context.SetupGet(f => f.DataFlow).Returns(dataflows.AsQueryable);
            context.SetupGet(f => f.DataFlowStep).Returns(dataflowsteps.AsQueryable);
            context.SetupGet(f => f.DatasetFileStatusActive).Returns(datasetfiles.AsQueryable);
            datafeature.Setup(f => f.CLA3332_ConsolidatedDataFlows.GetValue()).Returns(true);

            var dataflowservice = new DataFlowService(context.Object, null, null, null, null, null, datafeature.Object, null);

            // Act
            bool indicator = dataflowservice.ValidateStepIdAndDatasetFileIds(stepId, datasetFileIds);

            // Asset
            Assert.AreEqual(true, indicator); // status code worked
        }

        /*
         * Unit test to see if exception is thrown when stepId cannot be found
         */
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void StepIdNotFound()
        {
            // Arrange
            List<int> datasetFileIds = new List<int> { 3, 3, 3 };
            int stepId = 8;

            // creating 3 datasetfiles with the same associated schema/schemaid
            FileSchema schema = new FileSchema()
            {
                SchemaId = 3,
            };

            DatasetFile dsf1 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 1
            };
            DatasetFile dsf2 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 2
            };
            DatasetFile dsf3 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 3
            };

            var datasetfiles = new[] { dsf1, dsf2, dsf3 };

            var context = new Mock<IDatasetContext>();
            var datafeature = new Mock<IDataFeatures>();

            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Active;
            df.Id = 1;
            df.DatasetId = 1;
            df.SchemaId = 1;

           

            DataFlowStep step1 = new DataFlowStep()
            {
                Id = 1,
                Action = new ProducerS3DropAction(),
                DataFlow = df
            };

            

            df.Steps = new[] { step1 };

            // created two dataflow mock classes and two associated dataflowsteps

            var dataflows = new[] { df };
            var dataflowsteps = new[] { step1 };

            context.SetupGet(f => f.DataFlow).Returns(dataflows.AsQueryable);
            context.SetupGet(f => f.DataFlowStep).Returns(dataflowsteps.AsQueryable);
            context.SetupGet(f => f.DatasetFileStatusActive).Returns(datasetfiles.AsQueryable);
            datafeature.Setup(f => f.CLA3332_ConsolidatedDataFlows.GetValue()).Returns(true);

            var dataflowservice = new DataFlowService(context.Object, null, null, null, null, null, datafeature.Object, null);

            // Act

            Assert.ThrowsException<DataFlowStepNotFound>(() => dataflowservice.GetDataFlowDtoByStepId(stepId));

         
        }

        /*
         * Unit test to see if exception is thrown when stepId cannot be found
         */
        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DatasetFileIdsNotFound()
        {
            // Arrange
            List<int> datasetFileIds = new List<int> { 3, 3, 3 };
            int stepId = 8;

            // creating 3 datasetfiles with the same associated schema/schemaid
            FileSchema schema = new FileSchema()
            {
                SchemaId = 3,
            };

            DatasetFile dsf1 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 1
            };
            DatasetFile dsf2 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 2
            };
            DatasetFile dsf3 = new DatasetFile()
            {
                Schema = schema,
                DatasetFileId = 3
            };

            var datasetfiles = new[] { dsf1, dsf2, dsf3 };

            var context = new Mock<IDatasetContext>();
            var datafeature = new Mock<IDataFeatures>();

            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Active;
            df.Id = 1;
            df.DatasetId = 1;
            df.SchemaId = 1;



            DataFlowStep step1 = new DataFlowStep()
            {
                Id = 1,
                Action = new ProducerS3DropAction(),
                DataFlow = df
            };



            df.Steps = new[] { step1 };

            // created two dataflow mock classes and two associated dataflowsteps

            var dataflows = new[] { df };
            var dataflowsteps = new[] { step1 };

            context.SetupGet(f => f.DataFlow).Returns(dataflows.AsQueryable);
            context.SetupGet(f => f.DataFlowStep).Returns(dataflowsteps.AsQueryable);
            context.SetupGet(f => f.DatasetFileStatusActive).Returns(datasetfiles.AsQueryable);
            datafeature.Setup(f => f.CLA3332_ConsolidatedDataFlows.GetValue()).Returns(true);

            var dataflowservice = new DataFlowService(context.Object, null, null, null, null, null, datafeature.Object, null);

            int testDatasetFileId = 12;

            // Act
            
            Assert.ThrowsException<DataFileNotFoundException>(() => dataflowservice.GetSchemaIdFromDatasetFileId(testDatasetFileId));


        }


        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DataFlowService_UpgradeDataFlow_Invalid_DataFlow_Id()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow = new DataFlow()
            {
                Id = 1
            };
            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);


            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Act
            Assert.ThrowsException<DataFlowNotFound>(() => dataflowService.UpgradeDataFlow(2));
        }


        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DataFlowService_UpgradeDataFlow_Multiple_SchemaMappings()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow = new DataFlow()
            {
                Id = 1,
                Name = "TestFlow"
            };

            var dataflowStep = new DataFlowStep()
            {
                Id = 1,
                DataAction_Type_Id = DataActionType.SchemaMap
            };

            List<SchemaMap> schemaMappings = new List<SchemaMap>()
            {
                new SchemaMap()
                {
                    Id = 1,
                    DataFlowStepId = dataflowStep
                },
                new SchemaMap()
                {
                    Id = 2,
                    DataFlowStepId = dataflowStep
                }
            };

            dataflowStep.SchemaMappings = schemaMappings;
            dataflow.Steps = new List<DataFlowStep>() { dataflowStep };

            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);

            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Act
            Assert.ThrowsException<ArgumentException>(() => dataflowService.UpgradeDataFlow(1));
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DataFlowService_UpgradeDataFlow_FileSchemaFlow_Not_Upgraded()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow = new DataFlow()
            {
                Id = 1,
                Name = "FileSchemaFlow_TestFlow"
            };

            var dataflowStep = new DataFlowStep()
            {
                Id = 1,
                DataAction_Type_Id = DataActionType.SchemaMap
            };

            dataflow.Steps = new List<DataFlowStep>() { dataflowStep };

            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);

            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Act
            Assert.ThrowsException<ArgumentException>(() => dataflowService.UpgradeDataFlow(1));
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DataFlowService_UpgradeDataFlow_No_SchemaMap_Step_Not_Upgraded()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow = new DataFlow()
            {
                Id = 1,
                Name = "FileSchemaFlow_TestFlow"
            };

            var dataflowStep = new DataFlowStep()
            {
                Id = 1,
                DataAction_Type_Id = DataActionType.SchemaLoad
            };

            dataflow.Steps = new List<DataFlowStep>() { dataflowStep };

            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);

            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Act
            Assert.ThrowsException<ArgumentException>(() => dataflowService.UpgradeDataFlow(1));
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void DataFlowService_UpgradeDataFlow_Only_Active_DataFlows_Upgraded()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var dataflow_Deleted = new DataFlow()
            {
                Id = 1,
                Name = "FileSchemaFlow_TestFlow",
                ObjectStatus = ObjectStatusEnum.Deleted
            };

            var dataflow_PendingDelete = new DataFlow()
            {
                Id = 2,
                Name = "FileSchemaFlow_PendingDelete",
                ObjectStatus = ObjectStatusEnum.Pending_Delete
            };

            var dataflow_Disabled = new DataFlow()
            {
                Id = 3,
                Name = "FileSchemaFlow_Diabled",
                ObjectStatus = ObjectStatusEnum.Disabled
            };

            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow_Deleted);
            context.Setup(f => f.GetById<DataFlow>(2)).Returns(dataflow_PendingDelete);
            context.Setup(f => f.GetById<DataFlow>(3)).Returns(dataflow_Disabled);

            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Act
            Assert.ThrowsException<ArgumentException>(() => dataflowService.UpgradeDataFlow(1));
            Assert.ThrowsException<ArgumentException>(() => dataflowService.UpgradeDataFlow(2));
            Assert.ThrowsException<ArgumentException>(() => dataflowService.UpgradeDataFlow(3));
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void CheckForUpgradeDataFlow_ShouldBeEnqueued_OnlyOnce()
        {
            // Arrange
            var client = new Mock<IBackgroundJobClient>();
            var dataflowService = new DataFlowService(null, null, null, null, null, null, null, client.Object);

            // Act
            dataflowService.UpgradeDataFlows(new int[] { 1, 2 });

            // Assert
            client.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "UpgradeDataFlow" && (int)job.Args[0] == 1),
                It.IsAny<EnqueuedState>()), Times.Once);
            client.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "UpgradeDataFlow" && (int)job.Args[0] == 2),
                It.IsAny<EnqueuedState>()), Times.Once);
        }
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

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void Ignore_DataFlowDetailDto_Where_Status_is_not_Active()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");
            user.Setup(s => s.AssociateId).Returns("123456");


            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Pending_Delete;
            df.DatasetId = 1;

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

            // Act
            List<DataFlowDetailDto> testFlow = dataFlowService.GetDataFlowDetailDtoByDatasetId(1);
            
            // Assert
            Assert.AreEqual(0,testFlow.Count);
        }

        [TestCategory("Core DataFlowService")]
        [TestMethod]
        public void Get_DataFlowDetailDto_Where_Status_is_Active()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");
            user.Setup(s => s.AssociateId).Returns("123456");

            // Setup Dataflow steps for DataFlow objects
            DataFlowStep step = new DataFlowStep()
            {
                Action = new ProducerS3DropAction(),
                DataFlow = new DataFlow()
                {
                    Id = 1
                }
            };

            DataFlowStep step2 = new DataFlowStep()
            {
                Action = new ProducerS3DropAction(),
                DataFlow = new DataFlow()
                {
                    Id = 1
                }
            };

            // Mock 2 DataFlow objects - both sharing a DatasetId, but with different object statuses
            DataFlow df = MockClasses.MockDataFlow();
            df.ObjectStatus = ObjectStatusEnum.Deleted;
            df.DatasetId = 2;
            df.SchemaId = 1;
            df.Steps = new[] { step };

            DataFlow df2 = MockClasses.MockDataFlow();
            df2.ObjectStatus = ObjectStatusEnum.Active;
            df2.DatasetId = 2;
            df2.SchemaId = 2;
            df2.Steps = new[] { step2 };

            var dataflows = new[] { df, df2 };

            // Create mock retrieve jobs
            RetrieverJob job = MockClasses.GetMockRetrieverJob(
                                        MockClasses.MockDataFileConfig(
                                                MockClasses.MockDataset()), new FtpSource());

            RetrieverJob job2 = MockClasses.GetMockRetrieverJob(
                                        MockClasses.MockDataFileConfig(
                                                MockClasses.MockDataset()), new FtpSource());
            job.DataFlow = df;
            job2.DataFlow = df2;
            List<RetrieverJob> jobList = new List<RetrieverJob>() { job, job2 };

            // Mock dataset context and setup return values
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.DataFlow).Returns(dataflows.AsQueryable);
            context.Setup(s => s.RetrieverJob).Returns(jobList.AsQueryable());


            // Mock job service and setup return values
            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            // Mock data features and setup return values
            Mock<IDataFeatures> _datafeatures = new Mock<IDataFeatures>();
            _datafeatures.Setup(_ => _.CLA3332_ConsolidatedDataFlows.GetValue()).Returns(true);

            // Setup DataFlowService
            var dataFlowService = new DataFlowService(context.Object, null, jobService.Object, null, null, null, _datafeatures.Object, null);

            // Act
            List<DataFlowDetailDto> testFlow = dataFlowService.GetDataFlowDetailDtoByDatasetId(2);

            // Assert
            // Ensuring that the DataFlow with the deleted object status is filtered out and only returns the 
            // DataFlowDetailDto object mapped from the active object status DataFlow
            Assert.AreEqual(1, testFlow.Count);
        }
    }
}
