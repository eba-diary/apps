using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class DataFlowMetricServiceTests
    {
        [TestMethod]
        public void GetDataFileFlowMetrics_EmptyReturn()
        {
            //arrange
            var stubDataFlowMetricProvider  = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
            Assert.AreEqual(0, fileGroups.Count);
            stubDataFlowMetricProvider.VerifyAll();
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupMappings()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                EventMetricId = 2,
                EventContents = "contents",
                DatasetFileId = 1,
                FileName = "filename",
                EventMetricCreatedDateTime = new DateTime(100000),
                ExecutionOrder = 5,
                MaxExecutionOrder = 5,
                StatusCode = "C"

            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {
                EventMetricId = 1,
                EventContents = "contents",
                DatasetFileId = 1,
                FileName = "filename",
                EventMetricCreatedDateTime = new DateTime(10000),
                ExecutionOrder = 4,
                MaxExecutionOrder = 5,
                StatusCode = "C"
            };

            entityList.Add(entity1);
            entityList.Add(entity2);


            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFileFlowMetricsDto fileGroup = fileGroups[0];
            //assert
            Assert.AreEqual(entity2.EventMetricCreatedDateTime, fileGroup.FirstEventTime);
            Assert.AreEqual(entity1.EventMetricCreatedDateTime, fileGroup.LastEventTime);
            Assert.AreEqual(entity1.FileName, fileGroup.FileName);
            Assert.AreEqual(entity1.DatasetFileId, fileGroup.DatasetFileId);
            Assert.AreEqual("0.009", fileGroup.Duration);
            Assert.IsTrue(fileGroup.AllEventsComplete);
            Assert.IsTrue(fileGroup.AllEventsPresent);
            Assert.AreEqual(2, fileGroup.FlowEvents.Count);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupOrder()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                EventMetricId = 2,
                EventContents = "contents",
                DatasetFileId = 1,
                FileName = "filename",
                EventMetricCreatedDateTime = new DateTime(100000),
                ExecutionOrder = 5,
                MaxExecutionOrder = 5,
                StatusCode = "C"

            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {
                EventMetricId = 1,
                EventContents = "contents",
                DatasetFileId = 2,
                FileName = "filename2",
                EventMetricCreatedDateTime = new DateTime(10000),
                ExecutionOrder = 5,
                MaxExecutionOrder = 5,
                StatusCode = "C"
            };

            entityList.Add(entity1);
            entityList.Add(entity2);


            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
            Assert.AreEqual(entity1.FileName, fileGroups[0].FileName);
            Assert.AreEqual(entity2.FileName, fileGroups[1].FileName);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventMappings()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DateTime time = DateTime.Now;
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                QueryMadeDateTime = time,
                SchemaId = 1,
                EventContents = "contents",
                MaxExecutionOrder = 5,
                FileModifiedDateTime = time,
                OriginalFileName = "name",
                DatasetId = 1,
                ExecutionOrder = 5,
                DataActionId = 1,
                DataFlowId = 1,
                Partition = 1,
                DataActionTypeId = 1,
                MessageKey = "message",
                Duration = 1,
                Offset = 1,
                DataFlowName = "flowname",
                DataFlowStepId = 1,
                FlowExecutionGuid = "executionguid",
                FileSize = 1,
                EventMetricId = 1,
                StorageCode = "storagecode",
                FileCreatedDateTime = time,
                RunInstanceGuid = "instanceguid",
                FileName = "name",
                SaidKeyCode = "keycode",
                EventMetricCreatedDateTime = time,
                DatasetFileId = 1,
                ProcessStartDateTime = time,
                StatusCode = "statuscode",
            };
            entityList.Add(entity1);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFlowMetricDto metricDto = fileGroups[0].FlowEvents[0];
            //assert
            Assert.AreEqual(time, metricDto.QueryMadeDateTime);
            Assert.AreEqual(1, metricDto.SchemaId);
            Assert.AreEqual("contents", metricDto.EventContents);
            Assert.AreEqual(5, metricDto.TotalFlowSteps);
            Assert.AreEqual(time, metricDto.FileModifiedDateTime);
            Assert.AreEqual("name", metricDto.OriginalFileName);
            Assert.AreEqual(1, metricDto.DatasetId);
            Assert.AreEqual(5, metricDto.CurrentFlowStep);
            Assert.AreEqual(1, metricDto.DataActionId);
            Assert.AreEqual(1, metricDto.DataFlowId);
            Assert.AreEqual(1, metricDto.Partition);
            Assert.AreEqual(1, metricDto.DataActionTypeId);
            Assert.AreEqual("message", metricDto.MessageKey);
            Assert.AreEqual(1, metricDto.Duration);
            Assert.AreEqual(1, metricDto.Offset);
            Assert.AreEqual("flowname", metricDto.DataFlowName);
            Assert.AreEqual(1, metricDto.DataFlowStepId);
            Assert.AreEqual("executionguid", metricDto.FlowExecutionGuid);
            Assert.AreEqual(1, metricDto.FileSize);
            Assert.AreEqual(1, metricDto.EventMetricId);
            Assert.AreEqual("storagecode", metricDto.StorageCode);
            Assert.AreEqual(time, metricDto.FileCreatedDateTime);
            Assert.AreEqual("instanceguid", metricDto.RunInstanceGuid);
            Assert.AreEqual("name", metricDto.FileName);
            Assert.AreEqual("keycode", metricDto.SaidKeyCode);
            Assert.AreEqual(time, metricDto.MetricGeneratedDateTime);
            Assert.AreEqual(1, metricDto.DatasetFileId);
            Assert.AreEqual(time, metricDto.ProcessStartDateTime);
            Assert.AreEqual("statuscode", metricDto.StatusCode);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();

        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventOrder()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                EventMetricId = 2,


            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {
                EventMetricId = 1,
  
            };
            DataFlowMetric entity3 = new DataFlowMetric()
            {
                EventMetricId = 3,

            };

            entityList.Add(entity1);
            entityList.Add(entity2);
            entityList.Add(entity3);


            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFileFlowMetricsDto fileGroup = fileGroups[0];
            //assert
            Assert.AreEqual(entity3.EventMetricId, fileGroup.FlowEvents[0].EventMetricId);
            Assert.AreEqual(entity1.EventMetricId, fileGroup.FlowEvents[1].EventMetricId);
            Assert.AreEqual(entity2.EventMetricId, fileGroup.FlowEvents[2].EventMetricId);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();
        }

        [TestMethod]
        public void GetAllTotalFilesCount_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllTotalFiles()).Returns(GetElasticDataFlowMetricList());

            //act
            long docCount= dataFlowMetricService.GetAllTotalFilesCount();

            //assert
            Assert.AreEqual(2, docCount);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllTotalFiles_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllTotalFiles()).Returns(GetElasticDataFlowMetricList());

            List<Dataset> datasets = new List<Dataset>();

            datasets.Add(new Dataset()
            {
                DatasetName = "TEST1",
                DatasetId = 1
            });

            datasets.Add(new Dataset()
            {
                DatasetName = "TEST2",
                DatasetId = 2
            });

            stubIDatasetContext.Setup(x => x.Datasets).Returns(datasets.AsQueryable());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<DatasetProcessActivityDto> datasetProcessActivityDtos = dataFlowMetricService.GetAllTotalFiles();


            //assert
            Assert.AreEqual("TEST1", datasetProcessActivityDtos[0].DatasetName);
            Assert.AreEqual(1, datasetProcessActivityDtos[0].DatasetId);
            Assert.AreEqual(1, datasetProcessActivityDtos[0].FileCount);
            Assert.AreEqual(assertDateTime, datasetProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("TEST2", datasetProcessActivityDtos[1].DatasetName);
            Assert.AreEqual(2, datasetProcessActivityDtos[1].DatasetId);
            Assert.AreEqual(1, datasetProcessActivityDtos[1].FileCount);
            Assert.AreEqual(assertDateTime, datasetProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllTotalFilesByDataset_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllTotalFilesByDataset(It.IsAny<int>())).Returns(GetElasticDataFlowMetricList());

            List<FileSchema> schemas = new List<FileSchema>();

            schemas.Add(new FileSchema()
            {
                Name = "TEST1",
                SchemaId = 1
            });

            schemas.Add(new FileSchema()
            {
                Name = "TEST2",
                SchemaId = 2
            });

            stubIDatasetContext.Setup(x => x.FileSchema).Returns(schemas.AsQueryable());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<SchemaProcessActivityDto> schemaProcessActivityDtos = dataFlowMetricService.GetAllTotalFilesByDataset(1);

            //assert
            Assert.AreEqual("TEST1", schemaProcessActivityDtos[0].SchemaName);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].SchemaId);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].DatasetId);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].FileCount);
            Assert.AreEqual(assertDateTime, schemaProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("TEST2", schemaProcessActivityDtos[1].SchemaName);
            Assert.AreEqual(2, schemaProcessActivityDtos[1].SchemaId);
            Assert.AreEqual(1, schemaProcessActivityDtos[1].DatasetId);
            Assert.AreEqual(1, schemaProcessActivityDtos[1].FileCount);
            Assert.AreEqual(assertDateTime, schemaProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllTotalFilesBySchema_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllTotalFilesBySchema(It.IsAny<int>())).Returns(GetElasticDataFlowMetricList());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<DatasetFileProcessActivityDto> datasetFileProcessActivityDtos = dataFlowMetricService.GetAllTotalFilesBySchema(1);

            //assert
            Assert.AreEqual("fileName", datasetFileProcessActivityDtos[0].FileName);
            Assert.AreEqual("executionGuid", datasetFileProcessActivityDtos[0].FlowExecutionGuid);
            Assert.AreEqual(0, datasetFileProcessActivityDtos[0].LastFlowStep);
            Assert.AreEqual(assertDateTime, datasetFileProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("fileName", datasetFileProcessActivityDtos[1].FileName);
            Assert.AreEqual("executionGuid", datasetFileProcessActivityDtos[1].FlowExecutionGuid);
            Assert.AreEqual(0, datasetFileProcessActivityDtos[1].LastFlowStep);
            Assert.AreEqual(assertDateTime, datasetFileProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllFailedFilesCount_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllFailedFiles()).Returns(GetElasticDataFlowMetricList());

            //act
            long docCount = dataFlowMetricService.GetAllFailedFilesCount();

            //assert
            Assert.AreEqual(2, docCount);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllFailedFiles_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllFailedFiles()).Returns(GetElasticDataFlowMetricList());

            List<Dataset> datasets = new List<Dataset>();

            datasets.Add(new Dataset()
            {
                DatasetName = "TEST1",
                DatasetId = 1
            });

            datasets.Add(new Dataset()
            {
                DatasetName = "TEST2",
                DatasetId = 2
            });

            stubIDatasetContext.Setup(x => x.Datasets).Returns(datasets.AsQueryable());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<DatasetProcessActivityDto> datasetProcessActivityDtos = dataFlowMetricService.GetAllFailedFiles();


            //assert
            Assert.AreEqual("TEST1", datasetProcessActivityDtos[0].DatasetName);
            Assert.AreEqual(1, datasetProcessActivityDtos[0].DatasetId);
            Assert.AreEqual(1, datasetProcessActivityDtos[0].FileCount);
            Assert.AreEqual(assertDateTime, datasetProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("TEST2", datasetProcessActivityDtos[1].DatasetName);
            Assert.AreEqual(2, datasetProcessActivityDtos[1].DatasetId);
            Assert.AreEqual(1, datasetProcessActivityDtos[1].FileCount);
            Assert.AreEqual(assertDateTime, datasetProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllFailedFilesByDataset_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllFailedFilesByDataset(It.IsAny<int>())).Returns(GetElasticDataFlowMetricList());

            List<FileSchema> schemas = new List<FileSchema>();

            schemas.Add(new FileSchema()
            {
                Name = "TEST1",
                SchemaId = 1
            });

            schemas.Add(new FileSchema()
            {
                Name = "TEST2",
                SchemaId = 2
            });

            stubIDatasetContext.Setup(x => x.FileSchema).Returns(schemas.AsQueryable());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<SchemaProcessActivityDto> schemaProcessActivityDtos = dataFlowMetricService.GetAllFailedFilesByDataset(1);

            //assert
            Assert.AreEqual("TEST1", schemaProcessActivityDtos[0].SchemaName);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].SchemaId);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].DatasetId);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].FileCount);
            Assert.AreEqual(assertDateTime, schemaProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("TEST2", schemaProcessActivityDtos[1].SchemaName);
            Assert.AreEqual(2, schemaProcessActivityDtos[1].SchemaId);
            Assert.AreEqual(1, schemaProcessActivityDtos[1].DatasetId);
            Assert.AreEqual(1, schemaProcessActivityDtos[1].FileCount);
            Assert.AreEqual(assertDateTime, schemaProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllFailedFilesBySchema_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllFailedFilesBySchema(It.IsAny<int>())).Returns(GetElasticDataFlowMetricList());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<DatasetFileProcessActivityDto> datasetFileProcessActivityDtos = dataFlowMetricService.GetAllFailedFilesBySchema(1);

            //assert
            Assert.AreEqual("fileName", datasetFileProcessActivityDtos[0].FileName);
            Assert.AreEqual("executionGuid", datasetFileProcessActivityDtos[0].FlowExecutionGuid);
            Assert.AreEqual(0, datasetFileProcessActivityDtos[0].LastFlowStep);
            Assert.AreEqual(assertDateTime, datasetFileProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("fileName", datasetFileProcessActivityDtos[1].FileName);
            Assert.AreEqual("executionGuid", datasetFileProcessActivityDtos[1].FlowExecutionGuid);
            Assert.AreEqual(0, datasetFileProcessActivityDtos[1].LastFlowStep);
            Assert.AreEqual(assertDateTime, datasetFileProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllInFlightFiles_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllInFlightFiles()).Returns(GetElasticDataFlowMetricList());

            List<Dataset> datasets = new List<Dataset>();

            datasets.Add(new Dataset()
            {
                DatasetName = "TEST1",
                DatasetId = 1
            });

            datasets.Add(new Dataset()
            {
                DatasetName = "TEST2",
                DatasetId = 2
            });

            stubIDatasetContext.Setup(x => x.Datasets).Returns(datasets.AsQueryable());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<DatasetProcessActivityDto> datasetProcessActivityDtos = dataFlowMetricService.GetAllInFlightFiles();


            //assert
            Assert.AreEqual("TEST1", datasetProcessActivityDtos[0].DatasetName);
            Assert.AreEqual(1, datasetProcessActivityDtos[0].DatasetId);
            Assert.AreEqual(1, datasetProcessActivityDtos[0].FileCount);
            Assert.AreEqual(assertDateTime, datasetProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("TEST2", datasetProcessActivityDtos[1].DatasetName);
            Assert.AreEqual(2, datasetProcessActivityDtos[1].DatasetId);
            Assert.AreEqual(1, datasetProcessActivityDtos[1].FileCount);
            Assert.AreEqual(assertDateTime, datasetProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllInFlightFilesByDataset_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllInFlightFilesByDataset(It.IsAny<int>())).Returns(GetElasticDataFlowMetricList());

            List<FileSchema> schemas = new List<FileSchema>();

            schemas.Add(new FileSchema()
            {
                Name = "TEST1",
                SchemaId = 1
            });

            schemas.Add(new FileSchema()
            {
                Name = "TEST2",
                SchemaId = 2
            });

            stubIDatasetContext.Setup(x => x.FileSchema).Returns(schemas.AsQueryable());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<SchemaProcessActivityDto> schemaProcessActivityDtos = dataFlowMetricService.GetAllInFlightFilesByDataset(1);

            //assert
            Assert.AreEqual("TEST1", schemaProcessActivityDtos[0].SchemaName);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].SchemaId);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].DatasetId);
            Assert.AreEqual(1, schemaProcessActivityDtos[0].FileCount);
            Assert.AreEqual(assertDateTime, schemaProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("TEST2", schemaProcessActivityDtos[1].SchemaName);
            Assert.AreEqual(2, schemaProcessActivityDtos[1].SchemaId);
            Assert.AreEqual(1, schemaProcessActivityDtos[1].DatasetId);
            Assert.AreEqual(1, schemaProcessActivityDtos[1].FileCount);
            Assert.AreEqual(assertDateTime, schemaProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetAllInFlightFilesBySchema_MappedReturn()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            stubDataFlowMetricProvider.Setup(x => x.GetAllInFlightFilesBySchema(It.IsAny<int>())).Returns(GetElasticDataFlowMetricList());

            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            //act
            List<DatasetFileProcessActivityDto> datasetFileProcessActivityDtos = dataFlowMetricService.GetAllInFlightFilesBySchema(1);

            //assert
            Assert.AreEqual("fileName", datasetFileProcessActivityDtos[0].FileName);
            Assert.AreEqual("executionGuid", datasetFileProcessActivityDtos[0].FlowExecutionGuid);
            Assert.AreEqual(0, datasetFileProcessActivityDtos[0].LastFlowStep);
            Assert.AreEqual(assertDateTime, datasetFileProcessActivityDtos[0].LastEventTime);

            Assert.AreEqual("fileName", datasetFileProcessActivityDtos[1].FileName);
            Assert.AreEqual("executionGuid", datasetFileProcessActivityDtos[1].FlowExecutionGuid);
            Assert.AreEqual(0, datasetFileProcessActivityDtos[1].LastFlowStep);
            Assert.AreEqual(assertDateTime, datasetFileProcessActivityDtos[1].LastEventTime);

            stubDataFlowMetricProvider.VerifyAll();
            stubIDatasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetDataFlowMetrics_MappedReturn()
        {
            //arrange
            var stubIElasticDocumentClient = new Mock<IElasticDocumentClient>();

            DataFlowMetricSearchDto dto = new DataFlowMetricSearchDto()
            {
                DatasetFileIds = new int[] { 1, 2, 3 },
                DatasetId = 1,
                SchemaId = 1
            };

            stubIElasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<Func<Nest.SearchDescriptor<DataFlowMetric>, ISearchRequest>>())).ReturnsAsync(GetElasticDataFlowMetricList);

            DataFlowMetricProvider stubDataFlowMetricProvider = new DataFlowMetricProvider(stubIElasticDocumentClient.Object);

            //act
            List<DataFlowMetric> metricList = stubDataFlowMetricProvider.GetDataFlowMetrics(dto);

            //assert
            Assert.IsNotNull(metricList);

            stubIElasticDocumentClient.VerifyAll();
        }

        private ElasticResult<DataFlowMetric> GetElasticDataFlowMetricList()
        {
            DateTime assertDateTime = new DateTime(2023, 6, 6, 12, 35, 0);

            return new ElasticResult<DataFlowMetric>
            {
                SearchTotal = 2,
                Documents = new List<DataFlowMetric>()
                {
                    new DataFlowMetric()
                    {
                        QueryMadeDateTime = DateTime.Now,
                        SchemaId = 1,
                        EventContents = "eventContents",
                        MaxExecutionOrder = 0,
                        FileModifiedDateTime = DateTime.Now,
                        OriginalFileName = "ogFileName",
                        DatasetId = 1,
                        ExecutionOrder = 1,
                        DataActionId = 1,
                        Partition = 1,
                        DataActionTypeId = 1,
                        MessageKey = "messageKey",
                        Duration = 1,
                        Offset = 0,
                        DataFlowName = "flowName",
                        DataFlowStepId = 1,
                        FlowExecutionGuid = "executionGuid",
                        FileSize = 1,
                        EventMetricId = 1,
                        StorageCode = "storageCode",
                        FileCreatedDateTime = DateTime.Now,
                        FileName = "fileName",
                        SaidKeyCode = "keyCode",
                        EventMetricCreatedDateTime = assertDateTime,
                        DatasetFileId = 1,
                        ProcessStartDateTime = DateTime.Now,
                        StatusCode = "statusCode"
                    },
                    new DataFlowMetric()
                    {
                        QueryMadeDateTime = DateTime.Now,
                        SchemaId = 2,
                        EventContents = "eventContents",
                        MaxExecutionOrder = 0,
                        FileModifiedDateTime = DateTime.Now,
                        OriginalFileName = "ogFileName",
                        DatasetId = 2,
                        ExecutionOrder = 2,
                        DataActionId = 2,
                        Partition = 2,
                        DataActionTypeId = 2,
                        MessageKey = "messageKey",
                        Duration = 2,
                        Offset = 0,
                        DataFlowName = "flowName",
                        DataFlowStepId = 2,
                        FlowExecutionGuid = "executionGuid",
                        FileSize = 1,
                        EventMetricId = 2,
                        StorageCode = "storageCode",
                        FileCreatedDateTime = DateTime.Now,
                        FileName = "fileName",
                        SaidKeyCode = "keyCode",
                        EventMetricCreatedDateTime = assertDateTime,
                        DatasetFileId = 2,
                        ProcessStartDateTime = DateTime.Now,
                        StatusCode = "statusCode"
                    }
                },
                Aggregations = new AggregateDictionary(new Dictionary<string, IAggregate>
                {
                    [FilterCategoryNames.DataFlowMetric.DOC_COUNT] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 1,
                                Key = "1"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 1,
                                Key = "2"
                            }
                        }.AsReadOnly()
                    }
                })
            };
        }
    }
}