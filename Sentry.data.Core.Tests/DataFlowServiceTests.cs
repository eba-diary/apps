﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Linq;
using System.Threading.Tasks;
using Sentry.FeatureFlags.Mock;
using Sentry.FeatureFlags;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowServiceTests : BaseCoreUnitTest
    {
        /// <summary>
        /// - Test that the DataFlowService.Validate() method correctly identifies a duplicate DataFlow name
        /// and responds with the correct validation result.
        /// </summary>
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
        [TestMethod]
        public void CheckForDeleteDataFlow_ShouldBeEnqueued_OnlyOnce()
        {
            // Arrange
            var client = new Mock<IBackgroundJobClient>();

            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            userService.Setup(f => f.GetCurrentUser()).Returns(user1.Object);

            var dataflowService = new DataFlowService(null, userService.Object, null, null, null, null, null, client.Object);

            // Act
            dataflowService.DeleteDataFlows(new int[] { 1, 2 });

            // Assert
            client.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "Delete" && (int)job.Args[0] == 1),
                It.IsAny<EnqueuedState>()), Times.Once);
            client.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "Delete" && (int)job.Args[0] == 2),
                It.IsAny<EnqueuedState>()), Times.Once);
        }

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

            context.Setup(f => f.GetById<DataFlow>(1)).Returns(dataflow);

            var dataflowService = new DataFlowService(context.Object, null, null, null, null, null, null, null);

            //Assert
            Assert.ThrowsException<DataFlowNotFound>(() => dataflowService.Delete(2, "123456", false));
        }

        [TestMethod]
        public void DataFlow_Delete__Delete_Metadata_Set_Propertly()
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

            //We need to mock out IJobService due to call to DeleteJobByDataFlowId()
            var jobService = new Mock<IJobService>();

            var dataflowService = new DataFlowService(context.Object, null, jobService.Object, null, null, null, null, null);

            //Act
            dataflowService.Delete(1, "123456", false);

            //Assert
            DataFlow deletedFlow = context.Object.GetById<DataFlow>(1);

            Assert.AreEqual(ObjectStatusEnum.Deleted, deletedFlow.ObjectStatus);
            Assert.AreEqual("123456", deletedFlow.DeleteIssuer);
            Assert.AreNotEqual(DateTime.MaxValue, deletedFlow.DeleteIssueDTM);
        }
    }
}