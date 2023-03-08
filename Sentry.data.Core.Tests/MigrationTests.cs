using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
