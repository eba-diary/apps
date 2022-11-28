using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests.Extensions
{
    [TestClass]
    public class DataFlowMetricExtensionsTests
    {
        [TestMethod]
        public void DataFileFlowMetricDtoList_ToModels_Mappings()
        {
            //arrange
            DateTime time = DateTime.Now;
            DataFileFlowMetricsDto dto = new DataFileFlowMetricsDto()
            {
                DatasetFileId = 1,
                FileName = "filename",
                FirstEventTime = time,
                LastEventTime = time,
                Duration = "100",
                AllEventsPresent = true,
                AllEventsComplete = true,
                TargetCode = "targetcode"

            };
            List<DataFileFlowMetricsDto> dtoList = new List<DataFileFlowMetricsDto>() { dto };
            //act
            List<DataFlowMetricGroupModel> models = dtoList.ToModels();
            DataFlowMetricGroupModel model = models[0];
            //assert
            Assert.AreEqual(1, model.DatasetFileId);
            Assert.AreEqual("filename", model.FileName);
            Assert.AreEqual(time, model.FirstEventTime);
            Assert.AreEqual(time, model.LastEventTime);
            Assert.AreEqual("100", model.Duration);
            Assert.AreEqual(true, model.AllEventsPresent);
            Assert.AreEqual(true, model.AllEventsComplete);
            Assert.AreEqual("targetcode", model.TargetCode);
        }
        [TestMethod]
        public void DataFileFlowMetricDtoList_ToModels_EmptyInput()
        {
            //arrange
            List<DataFileFlowMetricsDto> dtoList = new List<DataFileFlowMetricsDto>();
            //act
            List<DataFlowMetricGroupModel> models = dtoList.ToModels();
            //assert
            Assert.AreEqual(0, models.Count);
        }
        [TestMethod]
        public void DataFlowMetricDtoList_ToModels_Mappings()
        {
            //arrange
            DateTime time = DateTime.Now;
            List<DataFlowMetricDto> dtoList = new List<DataFlowMetricDto>(){
                new DataFlowMetricDto()
                {
                QueryMadeDateTime = time,
                SchemaId = 1,
                EventContents = "contents",
                TotalFlowSteps = 5,
                FileModifiedDateTime = time,
                OriginalFileName = "name",
                DatasetId = 1,
                CurrentFlowStep = 5,
                DataActionId = 1,
                DataFlowId = 1,
                Partition = 1,
                DataActionTypeId = 1,
                MessageKey = "messagekey",
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
                FileName = "filename",
                SaidKeyCode = "keycode",
                MetricGeneratedDateTime = time,
                DatasetFileId = 1,
                ProcessStartDateTime = time,
                StatusCode = "statuscode",
                }
            };
            //act
            List<DataFlowMetricModel> models = dtoList.ToModels();
            DataFlowMetricModel model = models[0];
            //assert
            Assert.AreEqual(time, model.QueryMadeDateTime);
            Assert.AreEqual(1, model.SchemaId);
            Assert.AreEqual("contents", model.EventContents);
            Assert.AreEqual(5, model.TotalFlowSteps);
            Assert.AreEqual(time, model.FileModifiedDateTime);
            Assert.AreEqual("name", model.OriginalFileName);
            Assert.AreEqual(1, model.DatasetId);
            Assert.AreEqual(5, model.CurrentFlowStep);
            Assert.AreEqual(1, model.DataActionId);
            Assert.AreEqual(1, model.DataFlowId);
            Assert.AreEqual(1, model.Partition);
            Assert.AreEqual(1, model.DataActionTypeId);
            Assert.AreEqual("messagekey", model.MessageKey);
            Assert.AreEqual(1, model.Duration);
            Assert.AreEqual(1, model.Offset);
            Assert.AreEqual("flowname", model.DataFlowName);
            Assert.AreEqual(1, model.DataFlowStepId);
            Assert.AreEqual("executionguid", model.FlowExecutionGuid);
            Assert.AreEqual(1, model.FileSize);
            Assert.AreEqual(1, model.EventMetricId);
            Assert.AreEqual("storagecode", model.StorageCode);
            Assert.AreEqual(time, model.FileCreatedDateTime);
            Assert.AreEqual("instanceguid", model.RunInstanceGuid);
            Assert.AreEqual("filename", model.FileName);
            Assert.AreEqual("keycode", model.SaidKeyCode);
            Assert.AreEqual(time, model.MetricGeneratedDateTime);
            Assert.AreEqual(1, model.DatasetFileId);
            Assert.AreEqual(time, model.ProcessStartDateTime);
            Assert.AreEqual("statuscode", model.StatusCode);
        }

        [TestMethod]
        public void DataFlowMetricDtoList_ToModels_EmptyInput()
        {
            //arrange
            List<DataFlowMetricDto> flowEvents = new List<DataFlowMetricDto>();
            //act
            List<DataFlowMetricModel> models = flowEvents.ToModels();
            //assert
            Assert.AreEqual(0, models.Count);
        }

        [TestMethod]
        public void Map_DataFlow_Model_To_Dto()
        {
            DataFlowModel flowModel = MockClasses.MockDataFlowModel();

            DataFlowDto dto = flowModel.ToDto("123456");
            DataFlowDto toBeSearilalized = JsonConvert.DeserializeObject<DataFlowDto>(JsonConvert.SerializeObject(dto));
            toBeSearilalized.DFQuestionnaire = null;

            //Assert
            Assert.AreEqual(flowModel.DataFlowId, dto.Id, "Id");
            Assert.AreEqual(flowModel.Name, dto.Name, "Name");
            Assert.AreEqual(flowModel.SAIDAssetKeyCode, dto.SaidKeyCode, "SaidKeyCode");
            Assert.AreEqual(flowModel.CreatedBy, dto.CreatedBy, "CreatedBy");
            Assert.AreEqual(flowModel.CreatedDTM, dto.CreateDTM, "CreatedDTM");
            Assert.AreEqual(flowModel.IngestionTypeSelection, dto.IngestionType, "IngestionType");
            Assert.AreEqual(flowModel.IsCompressed, dto.IsCompressed, "IsCompressed");
            Assert.AreEqual(flowModel.IsPreProcessingRequired, dto.IsPreProcessingRequired, "IsPreProcessingRequired");
            Assert.AreEqual(flowModel.PreProcessingSelection, dto.PreProcessingOption, "PreProcessingOption");
            Assert.AreEqual(flowModel.ObjectStatus, dto.ObjectStatus, "ObjectStatu");
            Assert.AreEqual(flowModel.StorageCode, dto.FlowStorageCode, "FlowStorageCode");
            Assert.AreEqual(flowModel.NamedEnvironment, dto.NamedEnvironment, "NamedEnvironment");
            Assert.AreEqual(flowModel.NamedEnvironmentType, dto.NamedEnvironmentType, "NamedEnvironmentType");
            Assert.AreEqual(flowModel.PrimaryContactId, dto.PrimaryContactId, "PrimaryContactId");
            Assert.IsNotNull(dto.RetrieverJob, "RetrieverJob");
            Assert.AreEqual(JsonConvert.SerializeObject(toBeSearilalized), dto.DFQuestionnaire, "DFQuestionnaire");

            Assert.AreEqual(1, dto.SchemaMap.Count, "Schema Maps Count");
            Assert.AreEqual(flowModel.SchemaMaps[0].SelectedSchema, dto.SchemaMap[0].SchemaId, "SchemaMap.SchemaId");

            Assert.AreEqual(flowModel.TopicName, dto.TopicName, "Sample-Topic-Name");
            Assert.AreEqual(flowModel.TopicName, dto.TopicName, "Sample_Topic_Name");
        }
    }
}
