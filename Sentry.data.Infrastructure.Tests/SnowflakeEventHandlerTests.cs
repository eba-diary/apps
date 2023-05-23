using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class SnowflakeEventHandlerTests
    {
        [TestMethod]
        public void HandleSnowflakeCreateRequest_WithSchema()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.CREATE_REQUEST,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = null,
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.Requested.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeCreateResponse_Success()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.CREATE_RESPONSE,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = "SUCCESS",
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.Available.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeCreateResponse_Failure()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.CREATE_RESPONSE,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = "FAILURE",
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.RequestFailed.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeCreateResponse_Other()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.CREATE_RESPONSE,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = "PENDING",
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.Pending.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeDeleteRequest()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.DELETE_REQUEST,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = null,
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.DeleteRequested.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeDeleteResponse_Success()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.DELETE_RESPONSE,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = "SUCCESS",
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.Deleted.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeDeleteResponse_Failure()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.DELETE_RESPONSE,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = "FAILURE",
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.DeleteFailed.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        [TestMethod]
        public void HandleSnowflakeDeleteResponse_Other()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> featureFlags = repository.Create<IDataFeatures>();

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            FileSchema schema = new FileSchema()
            {
                SchemaId = 2,
                ConsumptionDetails = new List<SchemaConsumption>()
                {
                    new SchemaConsumptionSnowflake()
                    {
                        SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString()
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(schema);

            SnowflakeEventHandler snowflakeEventHandler = new SnowflakeEventHandler(datasetContext.Object, featureFlags.Object);

            SnowConsumptionMessageModel message = new SnowConsumptionMessageModel()
            {
                DatasetID = 1,
                EventType = GlobalConstants.SnowConsumptionMessageTypes.DELETE_RESPONSE,
                SchemaID = 2,
                RevisionID = 2,
                SnowStatus = "PENDING",
                InitiatorID = "000000",
                ChangeIND = null,
            };

            snowflakeEventHandler.HandleSnowConsumptionMessage(BuildSampleJsonFromObject(message));

            Assert.AreEqual(ConsumptionLayerTableStatusEnum.Pending.ToString(), schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStatus);

            repository.VerifyAll();
        }

        private string BuildSampleJsonFromObject(Object sample)
        {
            return JsonConvert.SerializeObject(sample);
        }
    }
}
