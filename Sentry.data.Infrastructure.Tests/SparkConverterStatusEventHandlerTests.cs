using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class SparkConverterStatusEventHandlerTests
    {
        [TestCategory("SparkConverterStatus")]
        [TestMethod]
        public void SuccessfullMessage()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();

            //SETUP
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));                 //I could pass in values where I want to verify the message passed in these values and then do a verify success
            string mockMessage = @"{'EventType':'SPARKCONVERTERSTATUS','Status':'Success','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";

            //ACT
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

           //VERIFY
            mockEventService.Verify(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
        [TestCategory("SparkConverterStatus")]
        [TestMethod]
        public void NotSPARKCONVERTERSTATUS()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();

            //SETUP
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));                 //I could pass in values where I want to verify the message passed in these values and then do a verify success
            string mockMessage = @"{'EventType':'SPARKCONVERTERSTATUS1','Status':'Success','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";
            
            //ACT
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY            
            mockEventService.Verify(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [TestCategory("SparkConverterStatus")]
        [TestMethod]
        public void StatusNotSucccess()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();

            //SETUP
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));                 //I could pass in values where I want to verify the message passed in these values and then do a verify success
            string mockMessage = @"{'EventType':'SPARKCONVERTERSTATUS','Status':'Failed','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";
            
            
            //ACT
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockEventService.Verify(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }
        [TestCategory("SparkConverterStatus")]
        [TestMethod]
        public void BadMessage()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();

            //SETUP
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));                 //I could pass in values where I want to verify the message passed in these values and then do a verify success
            string mockMessage = String.Empty;
            
            //ACT
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockEventService.Verify(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [TestCategory("SparkConverterStatus")]
        [TestMethod]
        public void NullEventType()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();

            //SETUP
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));                 //I could pass in values where I want to verify the message passed in these values and then do a verify success
            string mockMessage = @"{'EventTypeBro':'SPARKCONVERTERSTATUS','Status':'Failed','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";

            //ACT
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockEventService.Verify(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [TestCategory("SparkConverterStatus")]
        [TestMethod]
        public void NullDatasetID()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();

            //SETUP
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));                 //I could pass in values where I want to verify the message passed in these values and then do a verify success
            string mockMessage = @"{'EventType':'SPARKCONVERTERSTATUS','Status':'Failed','DatasetIdMe':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";

            //ACT
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockEventService.Verify(x => x.PublishSuccessEventByDatasetId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }
    }
}
