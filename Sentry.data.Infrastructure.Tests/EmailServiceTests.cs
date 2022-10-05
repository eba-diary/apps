using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Mock;
using Sentry.data.Core.Entities.DataProcessing;
using System.Net.Mail;
using Moq.Protected;
using Sentry.data.Core.GlobalEnums;


namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class EmailServiceTests
    {
        [TestMethod]
        public void EnsureProperEmailSubjectAndFeatureFlagOn()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL).Returns(feature.Object);

            Mock<IEmailClient> mockEmailClient = mockRepository.Create<IEmailClient>();

            //CALLBACK HERE PASSES A FUNCTION AS A PARM WHICH ASSERTS VARIOUS CONDITIONS WE WANT TO TEST
            mockEmailClient.Setup(x => x.Send(It.IsAny<MailMessage>())).Callback<MailMessage>((m) =>
            {
                Assert.AreEqual("S3 SINK CONNECTOR CREATE REMINDER", m.Subject);
            });

           
            EmailService emailService = new EmailService(null, mockDataFeatureService.Object,mockEmailClient.Object);
            DataFlow df = new DataFlow() 
            {
                TopicName = nameof(df.TopicName),
                S3ConnectorName = nameof(df.S3ConnectorName),
                FlowStorageCode = nameof(df.FlowStorageCode),
                NamedEnvironment = nameof(df.NamedEnvironment),
                SaidKeyCode = nameof(df.SaidKeyCode),
                Name = nameof(df.Name),
                CreatedBy = nameof(df.CreatedBy),
                Id = 10,
                IngestionType = (int) IngestionType.Topic,
                DatasetId = 10,
                SchemaId = 10
            };

            emailService.SendS3SinkConnectorRequestEmail(df);
            mockEmailClient.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            mockEmailClient.VerifyAll();        //VERIFY ALL SETUPS WERE EXECUTED
        }


        [TestMethod]
        public void EnsureFeatureFlagOff()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(false);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL).Returns(feature.Object);

            Mock<IEmailClient> mockEmailClient = mockRepository.Create<IEmailClient>();

            EmailService emailService = new EmailService(null, mockDataFeatureService.Object, mockEmailClient.Object);
            DataFlow df = new DataFlow()
            {
                TopicName = nameof(df.TopicName),
                S3ConnectorName = nameof(df.S3ConnectorName),
                FlowStorageCode = nameof(df.FlowStorageCode),
                NamedEnvironment = nameof(df.NamedEnvironment),
                SaidKeyCode = nameof(df.SaidKeyCode),
                Name = nameof(df.Name),
                CreatedBy = nameof(df.CreatedBy),
                Id = 10,
                IngestionType = (int)IngestionType.Topic,
                DatasetId = 10,
                SchemaId = 10
            };

            emailService.SendS3SinkConnectorRequestEmail(df);
            mockEmailClient.VerifyAll();
        }


        [TestMethod]
        public void EnsureBadDataFlowWorks()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL).Returns(feature.Object);

            Mock<IEmailClient> mockEmailClient = mockRepository.Create<IEmailClient>();

            //CALLBACK HERE PASSES A FUNCTION AS A PARM WHICH ASSERTS VARIOUS CONDITIONS WE WANT TO TEST
            mockEmailClient.Setup(x => x.Send(It.IsAny<MailMessage>())).Callback<MailMessage>((m) =>
            {
                Assert.AreEqual("S3 SINK CONNECTOR CREATE REMINDER", m.Subject);
            });


            EmailService emailService = new EmailService(null, mockDataFeatureService.Object, mockEmailClient.Object);
            DataFlow df = new DataFlow()
            {
                Id = 10,
                IngestionType = (int)IngestionType.Topic,
                DatasetId = 10,
                SchemaId = 10
            };

            emailService.SendS3SinkConnectorRequestEmail(df);
            mockEmailClient.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
            mockEmailClient.VerifyAll();        //VERIFY ALL SETUPS WERE EXECUTED
        }


        [TestMethod]
        public void EnsureNonTopicDoesNotSend()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(false);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL).Returns(feature.Object);

            Mock<IEmailClient> mockEmailClient = mockRepository.Create<IEmailClient>();

            EmailService emailService = new EmailService(null, mockDataFeatureService.Object, mockEmailClient.Object);
            DataFlow df = new DataFlow()
            {
                TopicName = nameof(df.TopicName),
                S3ConnectorName = nameof(df.S3ConnectorName),
                FlowStorageCode = nameof(df.FlowStorageCode),
                NamedEnvironment = nameof(df.NamedEnvironment),
                SaidKeyCode = nameof(df.SaidKeyCode),
                Name = nameof(df.Name),
                CreatedBy = nameof(df.CreatedBy),
                Id = 10,
                IngestionType = (int)IngestionType.DFS_Drop,
                DatasetId = 10,
                SchemaId = 10
            };

            emailService.SendS3SinkConnectorRequestEmail(df);
            mockEmailClient.VerifyAll();
        }
    }
}
