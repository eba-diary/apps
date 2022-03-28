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
        [TestMethod]
        public void TestHandleLogic1()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            mockEventService.Setup(x => x.PublishSuccessEventByDatasetId(GlobalConstants.EventType.CREATED_FILE, "SPARKCONVERTERSTATUS SUCCESS",0));
           
            string mockMessage = @"{'EventType':'SPARKCONVERTERSTATUS','Status':'Success','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";
            SparkConverterEventHandler handle = new SparkConverterEventHandler(mockEventService.Object);
            handle.HandleLogic(mockMessage);

            string mockMessageNotSparkConverter = @"{'EventType':'SPARKCONVERTERSTATUS1','Status':'Success','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";
            handle.HandleLogic(mockMessageNotSparkConverter);

            string mockMessageNotSuccess = @"{'EventType':'SPARKCONVERTERSTATUS','Status':'Failed','DatasetId':'1044','SchemaId':'42','EtlFileName':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','EtlFileNameOnly':'9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourcePath':'s3://sentry-data-prod-dataset-ae2/rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','SourceBucket':'sentry-data-prod-dataset-ae2','SourceKey':'rawquery/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetPath':'s3://sentry-data-prod-dataset-ae2/parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','TargetBucket':'sentry-data-prod-dataset-ae2','TargetKey':'parquet/data/6740001/2022/3/16/9d13c053-afe9-466f-845e-092e0c2164b5_20220316194646601.json','StartDateTime':'2022-03-16 19:48:59.960','ProcessDuration':'59759','FileSize':153,'FlowExecutionGuid':'20220316194646601'}";
            handle.HandleLogic(mockMessageNotSuccess);
        }
    }
}
