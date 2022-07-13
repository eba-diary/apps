using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using System.Web.Mvc;
using System.Threading;

namespace Sentry.data.Web.Controllers
{
    public class DataFlowMetricController : BaseController
    {
        private readonly DataFlowMetricService _dataFlowMetricService;
        public DataFlowMetricSearchDto searchDto;

        public DataFlowMetricController(DataFlowMetricService dataFlowMetricService)
        {
            _dataFlowMetricService = dataFlowMetricService;
        }
        public DataFlowMetricAccordionModel GetDataFlowMetricAccordionModel(List<DataFileFlowMetricsDto> dtoList)
        {
            DataFlowMetricAccordionModel dataFlowAccordionModel = new DataFlowMetricAccordionModel();
            dataFlowAccordionModel.DataFlowMetricGroups = dtoList;
            return dataFlowAccordionModel;
        }
        [HttpPost]
        public void GetSearchDto(DataFlowMetricSearchDto searchDtoData)
        {
            searchDto = new DataFlowMetricSearchDto();
            searchDto.DatasetToSearch = searchDtoData.DatasetToSearch;
            searchDto.SchemaToSearch = searchDtoData.SchemaToSearch;
            searchDto.FileToSearch = searchDtoData.FileToSearch;
        }
        public ActionResult GetDataFlowMetricAccordionView()
        {
            /*
            List<DataFlowMetricEntity> entityList = _dataFlowMetricService.GetDataFlowMetricEntities(searchDto);
            List<DataFlowMetricDto> metricDtoList = _dataFlowMetricService.GetMetricList(entityList);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(metricDtoList);
            DataFlowMetricAccordionModel dataFlowAccordionModel = GetDataFlowMetricAccordionModel(fileGroups);
            */
            //uncomment above and delete below mock when you get access to elastic search database
            DataFlowMetricEntity entity1 = new DataFlowMetricEntity();
            entity1.DatasetFileId = 1;
            entity1.FileName = "ExampleFileNameOne.csv";
            entity1.MetricGeneratedDateTime = DateTime.Now;
            entity1.EventContents = "{\"OriginalS3Event\":\"{\\\"EventType\\\":\\\"S3EVENT1\\\",\\\"Payload\\\":{\\\"s3\\\":{\\\"bucket\\\":{\\\"name\\\":\\\"sentry-dlst-qual-dataset-ae2\\\",\\\"arn\\\":\\\"arn:aws:s3:::sentry-dlst-qual-dataset-ae2\\\",\\\"ownerIdentity\\\":{\\\"principalId\\\":\\\"A1FN8XYD8GR8UA\\\"}},\\\"s3SchemaVersion\\\":\\\"1.0\\\",\\\"configurationId\\\":\\\"42f4a5fa-7c98-4767-9e7d-89072fda0c81\\\",\\\"object\\\":{\\\"versionId\\\":\\\".QXZyRPs8S6giI_Nqu3l8QoV.J3Ya.M6\\\",\\\"size\\\":144,\\\"eTag\\\":\\\"7b74542285539819dc7cf33af5d6833f\\\",\\\"key\\\":\\\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\\\",\\\"sequencer\\\":\\\"0062BC82D162F342DB\\\"}},\\\"awsRegion\\\":\\\"us-east-2\\\",\\\"eventVersion\\\":\\\"2.1\\\",\\\"responseElements\\\":{\\\"x-amz-request-id\\\":\\\"SPS8TZKKY3QT8DWY\\\",\\\"x-amz-id-2\\\":\\\"Tq1Zx8Hh0RF2eIyh3PzcR1SpJ0kHROFNUFNpJjUxxzVWb76yYHDAzykaADSOz1K53Gt2Txn3FGc+Ctb3ebAGP4KXdmGSBO3M\\\"},\\\"eventSource\\\":\\\"aws:s3\\\",\\\"eventTime\\\":\\\"2022-06-29T16:50:25.445Z\\\",\\\"requestParameters\\\":{\\\"sourceIPAddress\\\":\\\"10.84.81.224\\\"},\\\"eventName\\\":\\\"ObjectCreated:Copy\\\",\\\"userIdentity\\\":{\\\"principalId\\\":\\\"AWS:AROAVPLRX5GQGDICB7NBB:i-0f26398608f0447c6\\\"}},\\\"DatasetID\\\":0}\",\"StepTargetBucket\":\"sentry-dlst-qual-dataset-ae2\",\"EventType\":\"V2_DATAFLOWSTEP_QUERYSTORAGE_START\",\"DownstreamTargets\":[{\"BucketName\":\"sentry-dlst-qual-dataset-ae2\",\"ObjectKey\":\"temp-file/parquet/CLPC/RELEASE/3532319/20220629164459000/\"}],\"DatasetID\":\"464\",\"SourceBucket\":\"sentry-dlst-qual-dataset-ae2\",\"ActionId\":\"23\",\"ActionGuid\":\"61c96a9d-e3cb-49c9-9318-d73cc492e5b5\",\"SourceKey\":\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\",\"StepId\":\"9113\",\"StepTargetPrefix\":\"rawquery/CLPC/RELEASE/3532319/2022/6/29/\",\"FlowExecutionGuid\":\"20220629164459000\",\"RunInstanceGuid\":null,\"DataFlowGuid\":\"9BFEF702-17CC-49F7-A57F-4DF01EDAD86C\",\"SchemaId\":\"2023\",\"DataFlowId\":\"2046\",\"FileSize\":144,\"S3EventTime\":\"2022-06-29T16:50:25.445Z\"}";
            entity1.StatusCode = "C";
            entity1.TotalFlowSteps = 5;
            entity1.CurrentFlowStep = 4;
            entity1.EventMetricId = 1;
            entity1.DataFlowStepId = 1014;
            DataFlowMetricEntity entity2 = new DataFlowMetricEntity();
            entity2.DatasetFileId = 1;
            entity2.FileName = "ExampleFileNameOne.csv";
            Thread.Sleep(100);
            entity2.MetricGeneratedDateTime = DateTime.Now;
            entity2.EventContents = "{\"OriginalS3Event\":\"{\\\"EventType\\\":\\\"S3EVENT2\\\",\\\"Payload\\\":{\\\"s3\\\":{\\\"bucket\\\":{\\\"name\\\":\\\"sentry-dlst-qual-dataset-ae2\\\",\\\"arn\\\":\\\"arn:aws:s3:::sentry-dlst-qual-dataset-ae2\\\",\\\"ownerIdentity\\\":{\\\"principalId\\\":\\\"A1FN8XYD8GR8UA\\\"}},\\\"s3SchemaVersion\\\":\\\"1.0\\\",\\\"configurationId\\\":\\\"42f4a5fa-7c98-4767-9e7d-89072fda0c81\\\",\\\"object\\\":{\\\"versionId\\\":\\\".QXZyRPs8S6giI_Nqu3l8QoV.J3Ya.M6\\\",\\\"size\\\":144,\\\"eTag\\\":\\\"7b74542285539819dc7cf33af5d6833f\\\",\\\"key\\\":\\\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\\\",\\\"sequencer\\\":\\\"0062BC82D162F342DB\\\"}},\\\"awsRegion\\\":\\\"us-east-2\\\",\\\"eventVersion\\\":\\\"2.1\\\",\\\"responseElements\\\":{\\\"x-amz-request-id\\\":\\\"SPS8TZKKY3QT8DWY\\\",\\\"x-amz-id-2\\\":\\\"Tq1Zx8Hh0RF2eIyh3PzcR1SpJ0kHROFNUFNpJjUxxzVWb76yYHDAzykaADSOz1K53Gt2Txn3FGc+Ctb3ebAGP4KXdmGSBO3M\\\"},\\\"eventSource\\\":\\\"aws:s3\\\",\\\"eventTime\\\":\\\"2022-06-29T16:50:25.445Z\\\",\\\"requestParameters\\\":{\\\"sourceIPAddress\\\":\\\"10.84.81.224\\\"},\\\"eventName\\\":\\\"ObjectCreated:Copy\\\",\\\"userIdentity\\\":{\\\"principalId\\\":\\\"AWS:AROAVPLRX5GQGDICB7NBB:i-0f26398608f0447c6\\\"}},\\\"DatasetID\\\":0}\",\"StepTargetBucket\":\"sentry-dlst-qual-dataset-ae2\",\"EventType\":\"V2_DATAFLOWSTEP_QUERYSTORAGE_START\",\"DownstreamTargets\":[{\"BucketName\":\"sentry-dlst-qual-dataset-ae2\",\"ObjectKey\":\"temp-file/parquet/CLPC/RELEASE/3532319/20220629164459000/\"}],\"DatasetID\":\"464\",\"SourceBucket\":\"sentry-dlst-qual-dataset-ae2\",\"ActionId\":\"23\",\"ActionGuid\":\"61c96a9d-e3cb-49c9-9318-d73cc492e5b5\",\"SourceKey\":\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\",\"StepId\":\"9113\",\"StepTargetPrefix\":\"rawquery/CLPC/RELEASE/3532319/2022/6/29/\",\"FlowExecutionGuid\":\"20220629164459000\",\"RunInstanceGuid\":null,\"DataFlowGuid\":\"9BFEF702-17CC-49F7-A57F-4DF01EDAD86C\",\"SchemaId\":\"2023\",\"DataFlowId\":\"2046\",\"FileSize\":144,\"S3EventTime\":\"2022-06-29T16:50:25.445Z\"}";
            entity2.StatusCode = "C";
            entity2.TotalFlowSteps = 5;
            entity2.CurrentFlowStep = 5;
            entity2.EventMetricId = 2;
            entity2.DataFlowStepId = 1014;
            DataFlowMetricEntity entity3 = new DataFlowMetricEntity();
            entity3.DatasetFileId = 2;
            entity3.FileName = "ExampleFileNameTwo.csv";
            entity3.MetricGeneratedDateTime = DateTime.Now;
            entity3.EventContents = "{\"OriginalS3Event\":\"{\\\"EventType\\\":\\\"S3EVENT3\\\",\\\"Payload\\\":{\\\"s3\\\":{\\\"bucket\\\":{\\\"name\\\":\\\"sentry-dlst-qual-dataset-ae2\\\",\\\"arn\\\":\\\"arn:aws:s3:::sentry-dlst-qual-dataset-ae2\\\",\\\"ownerIdentity\\\":{\\\"principalId\\\":\\\"A1FN8XYD8GR8UA\\\"}},\\\"s3SchemaVersion\\\":\\\"1.0\\\",\\\"configurationId\\\":\\\"42f4a5fa-7c98-4767-9e7d-89072fda0c81\\\",\\\"object\\\":{\\\"versionId\\\":\\\".QXZyRPs8S6giI_Nqu3l8QoV.J3Ya.M6\\\",\\\"size\\\":144,\\\"eTag\\\":\\\"7b74542285539819dc7cf33af5d6833f\\\",\\\"key\\\":\\\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\\\",\\\"sequencer\\\":\\\"0062BC82D162F342DB\\\"}},\\\"awsRegion\\\":\\\"us-east-2\\\",\\\"eventVersion\\\":\\\"2.1\\\",\\\"responseElements\\\":{\\\"x-amz-request-id\\\":\\\"SPS8TZKKY3QT8DWY\\\",\\\"x-amz-id-2\\\":\\\"Tq1Zx8Hh0RF2eIyh3PzcR1SpJ0kHROFNUFNpJjUxxzVWb76yYHDAzykaADSOz1K53Gt2Txn3FGc+Ctb3ebAGP4KXdmGSBO3M\\\"},\\\"eventSource\\\":\\\"aws:s3\\\",\\\"eventTime\\\":\\\"2022-06-29T16:50:25.445Z\\\",\\\"requestParameters\\\":{\\\"sourceIPAddress\\\":\\\"10.84.81.224\\\"},\\\"eventName\\\":\\\"ObjectCreated:Copy\\\",\\\"userIdentity\\\":{\\\"principalId\\\":\\\"AWS:AROAVPLRX5GQGDICB7NBB:i-0f26398608f0447c6\\\"}},\\\"DatasetID\\\":0}\",\"StepTargetBucket\":\"sentry-dlst-qual-dataset-ae2\",\"EventType\":\"V2_DATAFLOWSTEP_QUERYSTORAGE_START\",\"DownstreamTargets\":[{\"BucketName\":\"sentry-dlst-qual-dataset-ae2\",\"ObjectKey\":\"temp-file/parquet/CLPC/RELEASE/3532319/20220629164459000/\"}],\"DatasetID\":\"464\",\"SourceBucket\":\"sentry-dlst-qual-dataset-ae2\",\"ActionId\":\"23\",\"ActionGuid\":\"61c96a9d-e3cb-49c9-9318-d73cc492e5b5\",\"SourceKey\":\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\",\"StepId\":\"9113\",\"StepTargetPrefix\":\"rawquery/CLPC/RELEASE/3532319/2022/6/29/\",\"FlowExecutionGuid\":\"20220629164459000\",\"RunInstanceGuid\":null,\"DataFlowGuid\":\"9BFEF702-17CC-49F7-A57F-4DF01EDAD86C\",\"SchemaId\":\"2023\",\"DataFlowId\":\"2046\",\"FileSize\":144,\"S3EventTime\":\"2022-06-29T16:50:25.445Z\"}";
            entity3.StatusCode = "F";
            entity3.TotalFlowSteps = 5;
            entity3.CurrentFlowStep = 5;
            entity3.EventMetricId = 3; 
            entity3.DataFlowStepId = 1014;
            DataFlowMetricEntity entity4 = new DataFlowMetricEntity();
            entity4.DatasetFileId = 3;
            entity4.FileName = "ExampleFileNameThree.csv";
            entity4.MetricGeneratedDateTime = DateTime.Now;
            entity4.EventContents = "{\"OriginalS3Event\":\"{\\\"EventType\\\":\\\"S3EVENT4\\\",\\\"Payload\\\":{\\\"s3\\\":{\\\"bucket\\\":{\\\"name\\\":\\\"sentry-dlst-qual-dataset-ae2\\\",\\\"arn\\\":\\\"arn:aws:s3:::sentry-dlst-qual-dataset-ae2\\\",\\\"ownerIdentity\\\":{\\\"principalId\\\":\\\"A1FN8XYD8GR8UA\\\"}},\\\"s3SchemaVersion\\\":\\\"1.0\\\",\\\"configurationId\\\":\\\"42f4a5fa-7c98-4767-9e7d-89072fda0c81\\\",\\\"object\\\":{\\\"versionId\\\":\\\".QXZyRPs8S6giI_Nqu3l8QoV.J3Ya.M6\\\",\\\"size\\\":144,\\\"eTag\\\":\\\"7b74542285539819dc7cf33af5d6833f\\\",\\\"key\\\":\\\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\\\",\\\"sequencer\\\":\\\"0062BC82D162F342DB\\\"}},\\\"awsRegion\\\":\\\"us-east-2\\\",\\\"eventVersion\\\":\\\"2.1\\\",\\\"responseElements\\\":{\\\"x-amz-request-id\\\":\\\"SPS8TZKKY3QT8DWY\\\",\\\"x-amz-id-2\\\":\\\"Tq1Zx8Hh0RF2eIyh3PzcR1SpJ0kHROFNUFNpJjUxxzVWb76yYHDAzykaADSOz1K53Gt2Txn3FGc+Ctb3ebAGP4KXdmGSBO3M\\\"},\\\"eventSource\\\":\\\"aws:s3\\\",\\\"eventTime\\\":\\\"2022-06-29T16:50:25.445Z\\\",\\\"requestParameters\\\":{\\\"sourceIPAddress\\\":\\\"10.84.81.224\\\"},\\\"eventName\\\":\\\"ObjectCreated:Copy\\\",\\\"userIdentity\\\":{\\\"principalId\\\":\\\"AWS:AROAVPLRX5GQGDICB7NBB:i-0f26398608f0447c6\\\"}},\\\"DatasetID\\\":0}\",\"StepTargetBucket\":\"sentry-dlst-qual-dataset-ae2\",\"EventType\":\"V2_DATAFLOWSTEP_QUERYSTORAGE_START\",\"DownstreamTargets\":[{\"BucketName\":\"sentry-dlst-qual-dataset-ae2\",\"ObjectKey\":\"temp-file/parquet/CLPC/RELEASE/3532319/20220629164459000/\"}],\"DatasetID\":\"464\",\"SourceBucket\":\"sentry-dlst-qual-dataset-ae2\",\"ActionId\":\"23\",\"ActionGuid\":\"61c96a9d-e3cb-49c9-9318-d73cc492e5b5\",\"SourceKey\":\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\",\"StepId\":\"9113\",\"StepTargetPrefix\":\"rawquery/CLPC/RELEASE/3532319/2022/6/29/\",\"FlowExecutionGuid\":\"20220629164459000\",\"RunInstanceGuid\":null,\"DataFlowGuid\":\"9BFEF702-17CC-49F7-A57F-4DF01EDAD86C\",\"SchemaId\":\"2023\",\"DataFlowId\":\"2046\",\"FileSize\":144,\"S3EventTime\":\"2022-06-29T16:50:25.445Z\"}";
            entity4.StatusCode = "C";
            entity4.TotalFlowSteps = 5;
            entity4.CurrentFlowStep = 4;
            entity4.EventMetricId = 4;
            entity4.DataFlowStepId = 1014;
            DataFlowMetricEntity entity5 = new DataFlowMetricEntity();
            entity5.DatasetFileId = 4;
            entity5.FileName = "ExampleFileNameFour.csv";
            entity5.MetricGeneratedDateTime = DateTime.Now;
            entity5.EventContents = "{\"OriginalS3Event\":\"{\\\"EventType\\\":\\\"S3EVENT5\\\",\\\"Payload\\\":{\\\"s3\\\":{\\\"bucket\\\":{\\\"name\\\":\\\"sentry-dlst-qual-dataset-ae2\\\",\\\"arn\\\":\\\"arn:aws:s3:::sentry-dlst-qual-dataset-ae2\\\",\\\"ownerIdentity\\\":{\\\"principalId\\\":\\\"A1FN8XYD8GR8UA\\\"}},\\\"s3SchemaVersion\\\":\\\"1.0\\\",\\\"configurationId\\\":\\\"42f4a5fa-7c98-4767-9e7d-89072fda0c81\\\",\\\"object\\\":{\\\"versionId\\\":\\\".QXZyRPs8S6giI_Nqu3l8QoV.J3Ya.M6\\\",\\\"size\\\":144,\\\"eTag\\\":\\\"7b74542285539819dc7cf33af5d6833f\\\",\\\"key\\\":\\\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\\\",\\\"sequencer\\\":\\\"0062BC82D162F342DB\\\"}},\\\"awsRegion\\\":\\\"us-east-2\\\",\\\"eventVersion\\\":\\\"2.1\\\",\\\"responseElements\\\":{\\\"x-amz-request-id\\\":\\\"SPS8TZKKY3QT8DWY\\\",\\\"x-amz-id-2\\\":\\\"Tq1Zx8Hh0RF2eIyh3PzcR1SpJ0kHROFNUFNpJjUxxzVWb76yYHDAzykaADSOz1K53Gt2Txn3FGc+Ctb3ebAGP4KXdmGSBO3M\\\"},\\\"eventSource\\\":\\\"aws:s3\\\",\\\"eventTime\\\":\\\"2022-06-29T16:50:25.445Z\\\",\\\"requestParameters\\\":{\\\"sourceIPAddress\\\":\\\"10.84.81.224\\\"},\\\"eventName\\\":\\\"ObjectCreated:Copy\\\",\\\"userIdentity\\\":{\\\"principalId\\\":\\\"AWS:AROAVPLRX5GQGDICB7NBB:i-0f26398608f0447c6\\\"}},\\\"DatasetID\\\":0}\",\"StepTargetBucket\":\"sentry-dlst-qual-dataset-ae2\",\"EventType\":\"V2_DATAFLOWSTEP_QUERYSTORAGE_START\",\"DownstreamTargets\":[{\"BucketName\":\"sentry-dlst-qual-dataset-ae2\",\"ObjectKey\":\"temp-file/parquet/CLPC/RELEASE/3532319/20220629164459000/\"}],\"DatasetID\":\"464\",\"SourceBucket\":\"sentry-dlst-qual-dataset-ae2\",\"ActionId\":\"23\",\"ActionGuid\":\"61c96a9d-e3cb-49c9-9318-d73cc492e5b5\",\"SourceKey\":\"temp-file/rawquery/CLPC/RELEASE/3532319/20220629164459000/CKMT_QUAL_FACTORTRACES_01_0_0000393124.json.trg\",\"StepId\":\"9113\",\"StepTargetPrefix\":\"rawquery/CLPC/RELEASE/3532319/2022/6/29/\",\"FlowExecutionGuid\":\"20220629164459000\",\"RunInstanceGuid\":null,\"DataFlowGuid\":\"9BFEF702-17CC-49F7-A57F-4DF01EDAD86C\",\"SchemaId\":\"2023\",\"DataFlowId\":\"2046\",\"FileSize\":144,\"S3EventTime\":\"2022-06-29T16:50:25.445Z\"}";
            entity5.StatusCode = "F";
            entity5.TotalFlowSteps = 5;
            entity5.CurrentFlowStep = 4;
            entity5.EventMetricId = 5;
            entity5.DataFlowStepId = 1014;
            List<DataFlowMetricEntity> dataFlowMetricEntities = new List<DataFlowMetricEntity>();
            dataFlowMetricEntities.Add(entity1);
            dataFlowMetricEntities.Add(entity2);
            dataFlowMetricEntities.Add(entity3);
            dataFlowMetricEntities.Add(entity4);
            dataFlowMetricEntities.Add(entity5);

            List<DataFlowMetricDto> metricDtoList = _dataFlowMetricService.GetMetricList(dataFlowMetricEntities);
            List<DataFileFlowMetricsDto> fileGroups = _dataFlowMetricService.GetFileMetricGroups(metricDtoList);
            DataFlowMetricAccordionModel dataFlowAccordionModel = GetDataFlowMetricAccordionModel(fileGroups);
            return PartialView("_DataFlowMetricAccordion", dataFlowAccordionModel);
        }
    }
}