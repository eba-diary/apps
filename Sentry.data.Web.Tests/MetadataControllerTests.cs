using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.data.Web.Extensions;
using Sentry.data.Web.Models.ApiModels.Migration;
using Sentry.data.Web.WebApi.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class MetadataControllerTests : BaseWebUnitTests
    {

        [TestInitialize]
        public void MyTestInitialize()
        {
            TestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }

        [TestMethod]
        public void MigrationRequest_ToDto()
        { 
            DatasetMigrationRequestModel model = new DatasetMigrationRequestModel()
            {
                SourceDatasetId = 99,
                TargetDatasetNamedEnvironment = "TEST",
                TargetDatasetId = 22,
                SchemaMigrationRequests = new List<SchemaMigrationRequestModel>()
                {
                    new SchemaMigrationRequestModel()
                    {
                        SourceSchemaId = 44,
                        TargetDatasetNamedEnvironment = "TEST",
                        TargetDataFlowNamedEnviornment = "TEST2",
                        TargetDatasetId = 33
                    },
                    new SchemaMigrationRequestModel()
                    {
                        SourceSchemaId = 66,
                        TargetDatasetNamedEnvironment = "TEST",
                        TargetDataFlowNamedEnviornment = "TEST2",
                        TargetDatasetId = 33
                    },
                    new SchemaMigrationRequestModel()
                    {
                        SourceSchemaId = 88,
                        TargetDatasetNamedEnvironment = "TEST",
                        TargetDataFlowNamedEnviornment = "TEST2",
                        TargetDatasetId = 33
                    }
                }
            };
            var datasetRequest = model.ToDto();
            var schemaRequest = datasetRequest.SchemaMigrationRequests.FirstOrDefault(w => w.SourceSchemaId == 44);

            //Assert
            Assert.AreEqual(99, datasetRequest.SourceDatasetId);
            Assert.AreEqual("TEST", datasetRequest.TargetDatasetNamedEnvironment);
            Assert.AreEqual(22, datasetRequest.TargetDatasetId);
            Assert.IsNotNull(schemaRequest);
            Assert.AreEqual(44, schemaRequest.SourceSchemaId);
            Assert.AreEqual("TEST", schemaRequest.TargetDatasetNamedEnvironment);
            Assert.AreEqual("TEST2", schemaRequest.TargetDataFlowNamedEnvironment);
            Assert.AreEqual(33, schemaRequest.TargetDatasetId);

        }

        [TestMethod]
        public void ToDatasetMigrationResponseModel()
        {
            SchemaMigrationRequestResponse schemaRequestResponse = new SchemaMigrationRequestResponse()
            {
                TargetSchemaId = 22,
                MigratedSchema = true,
                SchemaMigrationReason = "Schema Migrated",
                TargetSchemaRevisionId = 33,
                MigratedSchemaRevision = true,
                SchemaRevisionMigrationReason = "Schema Revision Migrated",
                TargetDataFlowId = 44,
                MigratedDataFlow = true,
                DataFlowMigrationReason = "DataFlow Migrated"
            };

            DatasetMigrationRequestResponse datasetRequestResponse = new DatasetMigrationRequestResponse()
            {
                DatasetId = 11,
                IsDatasetMigrated = true,
                DatasetMigrationReason = "It Migrated",
                SchemaMigrationResponses = new List<SchemaMigrationRequestResponse>() { schemaRequestResponse }
            };

            MetadataController controller = new MetadataController(null, null, null, null, null, null, null, null, null);

            //Act
            DatasetMigrationResponseModel datasetResponseModel = controller.ToDatasetMigrationResponseModel(datasetRequestResponse);
            SchemaMigrationResponseModel schemaResponseModel = datasetResponseModel.SchemaMigrationResponse.FirstOrDefault();
            //Assert
            Assert.IsNotNull(datasetResponseModel);
            Assert.AreEqual(datasetRequestResponse.DatasetId, datasetResponseModel.DatasetId);
            Assert.AreEqual(datasetRequestResponse.IsDatasetMigrated, datasetResponseModel.IsDatasetMigrated);
            Assert.AreEqual(datasetRequestResponse.DatasetMigrationReason, datasetResponseModel.DatasetMigrationReason);
            Assert.AreEqual(1, datasetRequestResponse.SchemaMigrationResponses.Count);
            Assert.IsNotNull(schemaResponseModel);
            Assert.AreEqual(schemaRequestResponse.TargetSchemaId, schemaResponseModel.SchemaId);
            Assert.AreEqual(schemaRequestResponse.MigratedSchema, schemaResponseModel.IsSchemaMigrated);
            Assert.AreEqual(schemaRequestResponse.SchemaMigrationReason, schemaResponseModel.SchemaMigrationMessage);
            Assert.AreEqual(schemaRequestResponse.TargetSchemaRevisionId, schemaResponseModel.SchemaRevisionId);
            Assert.AreEqual(schemaRequestResponse.MigratedSchemaRevision, schemaResponseModel.IsSchemaRevisionMigrated);
            Assert.AreEqual(schemaRequestResponse.SchemaRevisionMigrationReason, schemaResponseModel.SchemaRevisionMigrationMessage);
            Assert.AreEqual(schemaRequestResponse.TargetDataFlowId, schemaResponseModel.DataFlowId);
            Assert.AreEqual(schemaRequestResponse.MigratedDataFlow, schemaResponseModel.IsDataFlowMigrated);
            Assert.AreEqual(schemaRequestResponse.DataFlowMigrationReason, schemaResponseModel.DataFlowMigrationMessage);
        }
    }
}
