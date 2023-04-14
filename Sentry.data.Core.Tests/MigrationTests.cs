﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class MigrationTests
    {
        [TestMethod]
        public void MotToSchemaMigrationRequest()
        {
            DatasetMigrationRequest datasetMigrationRequest = new DatasetMigrationRequest()
            {
                TargetDatasetId = 0,
                TargetDatasetNamedEnvironment = "PROD",
                TargetDatasetNamedEnvironmentType = GlobalEnums.NamedEnvironmentType.Prod,
                SourceDatasetId = 99,
                SchemaMigrationRequests = new List<DatasetSchemaMigrationRequest>()
                {
                    new DatasetSchemaMigrationRequest()
                    {
                        SourceSchemaId = 77,
                        TargetDataFlowNamedEnvironment = "PROD1"
                    },
                    new DatasetSchemaMigrationRequest()
                    {
                        SourceSchemaId = 44,
                        TargetDataFlowNamedEnvironment = "PROD2"
                    }
                }
            };

            List<SchemaMigrationRequest> schemaRequests = datasetMigrationRequest.MapToSchemaMigrationRequest();

            Assert.AreEqual(2, schemaRequests.Count);
            Assert.IsFalse(schemaRequests.Any(w => w.TargetDatasetNamedEnvironment != "PROD"));
            Assert.IsFalse(schemaRequests.Any(w => w.TargetDatasetNamedEnvironmentType != GlobalEnums.NamedEnvironmentType.Prod));
            Assert.AreEqual(1, schemaRequests.Count(w => w.SourceSchemaId == 77));
            Assert.AreEqual(1, schemaRequests.Count(w => w.SourceSchemaId == 44));
        }


        [TestMethod]
        public void Validate_Get_Correct_MigrationHistory_Dev_Test()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();

            List<MigrationHistory> migrationHistoriesContext = new List<MigrationHistory>();
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_DEV_to_TEST());
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_TEST_to_DEV());
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_TEST_to_QUAL());
            context.Setup(f => f.MigrationHistory).Returns(migrationHistoriesContext.AsQueryable());

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.GetDatasetDetailDto(It.IsAny<int>())).Returns(MockClasses.MockDatasetDetailDtoForMigrationHistory_DEV());
            
            // Fake
            MigrationService migrationService = new MigrationService(context.Object, datasetService.Object);
            List<MigrationHistory> migrationHistoriesReturned = migrationService.GetMigrationHistory(1000,"DEV");

            // Verify
            Assert.AreEqual(2,migrationHistoriesReturned.Count);    //2 TOTAL MigrationHistory returned ONLY
            Assert.AreEqual(0, migrationHistoriesReturned.Where(w => w.MigrationHistoryId == 3).Count());       //MAKE SURE ID 3 was NOT included even though it exists because it wasn't involved in relatives
            Assert.AreEqual(2, migrationHistoriesReturned.Where(w => w.MigrationHistoryId == 1 || w.MigrationHistoryId == 2).Count());  //only Ids 1 and 2 should be returned
        }

        [TestMethod]
        public void Validate_Get_Correct_MigrationHistory_Qual()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();

            List<MigrationHistory> migrationHistoriesContext = new List<MigrationHistory>();
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_DEV_to_TEST());
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_TEST_to_DEV());
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_TEST_to_QUAL());
            context.Setup(f => f.MigrationHistory).Returns(migrationHistoriesContext.AsQueryable());


            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.GetDatasetDetailDto(It.IsAny<int>())).Returns(MockClasses.MockDatasetDetailDtoForMigrationHistory_QUAL());

            // Fake
            MigrationService migrationService = new MigrationService(context.Object, datasetService.Object);
            List<MigrationHistory> migrationHistoriesReturned = migrationService.GetMigrationHistory(1002, "QUAL");

            // Verify
            Assert.AreEqual(1, migrationHistoriesReturned.Count); //1 TOTAL MigrationHistory returned ONLY SINCE QUAL WAS THE ORIGIN and was only involved once
            Assert.AreEqual(1, migrationHistoriesReturned.Where(w => w.MigrationHistoryId == 3).Count());   //QUAL ID WAS ACTUALLY BROUGHT BACK
            Assert.AreEqual(0, migrationHistoriesReturned.Where(w => w.MigrationHistoryId == 1 || w.MigrationHistoryId == 2).Count());  //DEV AND TEST DON'T EXIST
        }


        [TestMethod]
        public void Validate_RelativesHaveMigrationHistory()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();

            List<MigrationHistory> migrationHistoriesContext = new List<MigrationHistory>();
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_DEV_to_TEST());
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_TEST_to_DEV());
            migrationHistoriesContext.Add(MockClasses.MockMigrationHistory_TEST_to_QUAL());
            context.Setup(f => f.MigrationHistory).Returns(migrationHistoriesContext.AsQueryable());

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.GetDatasetDetailDto(It.IsAny<int>())).Returns(MockClasses.MockDatasetDetailDtoForMigrationHistory_QUAL());

            // Fake
            MigrationService migrationService = new MigrationService(context.Object, datasetService.Object);
            DatasetRelativeOriginDto relativesWithMigrationHistory = migrationService.GetRelativesWithMigrationHistory(1000);

            // Verify
            Assert.AreEqual(3, relativesWithMigrationHistory.DatasetRelativesDto.Count);
        }

    }
}
