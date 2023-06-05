using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class MetadataMonitorServiceTests
    {
        [TestMethod]
        public void CheckConsumptionLayerStatus_NoneToEmail()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.Available.ToString()
                    }
                }
            };
            
            FileSchema schema2 = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.Deleted.ToString()
                    }
                }
            };

            FileSchema schema3 = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = true,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteRequested.ToString(),
                        LastChanged = DateTime.Now.AddDays(-2)
                    }
                }
            };
            
            FileSchema schema4 = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() {  },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteRequested.ToString(),
                        LastChanged = DateTime.Now.AddDays(-2)
                    }
                }
            };

            IList<FileSchema> schemaList = new List<FileSchema>() { schema, schema2, schema3, schema4 };

            datasetContext.Setup(x => x.FileSchema).Returns(schemaList.AsQueryable());

            Mock<IEmailService> emailService = repository.Create<IEmailService>();

            MetadataMonitorService monitorService = new MetadataMonitorService(datasetContext.Object, emailService.Object);

            monitorService.CheckConsumptionLayerStatus();

            emailService.Verify(x => x.SendConsumptionLayerStaleEmail(It.IsAny<IList<SchemaConsumptionSnowflake>>()), Times.Never());

            repository.VerifyAll();

        }

        [TestMethod]
        public void CheckConsumptionLayerStatus_EmailTwo()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.Available.ToString()
                    }
                }
            };

            FileSchema schema2 = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.Deleted.ToString()
                    }
                }
            };

            FileSchema schema3 = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteRequested.ToString(),
                        LastChanged = DateTime.Now.AddDays(-2)
                    }
                }
            };

            FileSchema schema4 = new FileSchema()
            {
                SchemaId = 2,
                DeleteInd = false,
                Revisions = new List<SchemaRevision>() { new SchemaRevision() },
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.DeleteRequested.ToString(),
                        LastChanged = DateTime.Now.AddDays(-2)
                    }
                }
            };

            IList<FileSchema> schemaList = new List<FileSchema>() { schema, schema2, schema3, schema4 };

            datasetContext.Setup(x => x.FileSchema).Returns(schemaList.AsQueryable());

            Mock<IEmailService> emailService = repository.Create<IEmailService>();
            emailService.Setup(x => x.SendConsumptionLayerStaleEmail(It.IsAny<IList<SchemaConsumptionSnowflake>>()));

            MetadataMonitorService monitorService = new MetadataMonitorService(datasetContext.Object, emailService.Object);

            monitorService.CheckConsumptionLayerStatus();

            emailService.Verify(x => x.SendConsumptionLayerStaleEmail(It.Is<IList<SchemaConsumptionSnowflake>>(y => y.Count() == 2)), Times.Once());

            repository.VerifyAll();

        }
    }
}
