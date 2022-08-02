using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                FileName = "",
                FirstEventTime = time,
                LastEventTime = time,
                Duration = "",
                AllEventsPresent = true,
                AllEventsComplete = true,
                TargetCode = ""

            };
            List<DataFileFlowMetricsDto> dtoList = new List<DataFileFlowMetricsDto>() { dto };
            //act
            List<DataFlowMetricGroupModel> models = dtoList.ToModels();
            //assert
            Assert.AreEqual(1, models[0].DatasetFileId);
            Assert.AreEqual("", models[0].FileName);
            Assert.AreEqual(time, models[0].FirstEventTime);
            Assert.AreEqual(time, models[0].LastEventTime);
            Assert.AreEqual("", models[0].Duration);
            Assert.AreEqual(true, models[0].AllEventsPresent);
            Assert.AreEqual(true, models[0].AllEventsComplete);
            Assert.AreEqual("", models[0].TargetCode);
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
                EventContents = "",
                TotalFlowSteps = 5,
                FileModifiedDateTime = time,
                OriginalFileName = "name",
                DatasetId = 1,
                CurrentFlowStep = 5,
                DataActionId = 1,
                DataFlowId = 1,
                Partition = 1,
                DataActionTypeId = 1,
                MessageKey = "",
                Duration = 1,
                Offset = 1,
                DataFlowName = "",
                DataFlowStepId = 1,
                FlowExecutionGuid = "",
                FileSize = 1,
                EventMetricId = 1,
                StorageCode = "",
                FileCreatedDateTime = time,
                RunInstanceGuid = "",
                FileName = "name",
                SaidKeyCode = "",
                MetricGeneratedDateTime = time,
                DatasetFileId = 1,
                ProcessStartDateTime = time,
                StatusCode = "",
                }
            };
            //act
            List<DataFlowMetricModel> models = dtoList.ToModels();
            //assert
            Assert.AreEqual(time, models[0].QueryMadeDateTime);
            Assert.AreEqual(1, models[0].SchemaId);
            Assert.AreEqual("", models[0].EventContents);
            Assert.AreEqual(5, models[0].TotalFlowSteps);
            Assert.AreEqual(time, models[0].FileModifiedDateTime);
            Assert.AreEqual("name", models[0].OriginalFileName);
            Assert.AreEqual(1, models[0].DatasetId);
            Assert.AreEqual(5, models[0].CurrentFlowStep);
            Assert.AreEqual(1, models[0].DataActionId);
            Assert.AreEqual(1, models[0].DataFlowId);
            Assert.AreEqual(1, models[0].Partition);
            Assert.AreEqual(1, models[0].DataActionTypeId);
            Assert.AreEqual("", models[0].MessageKey);
            Assert.AreEqual(1, models[0].Duration);
            Assert.AreEqual(1, models[0].Offset);
            Assert.AreEqual("", models[0].DataFlowName);
            Assert.AreEqual(1, models[0].DataFlowStepId);
            Assert.AreEqual("", models[0].FlowExecutionGuid);
            Assert.AreEqual(1, models[0].FileSize);
            Assert.AreEqual(1, models[0].EventMetricId);
            Assert.AreEqual("", models[0].StorageCode);
            Assert.AreEqual(time, models[0].FileCreatedDateTime);
            Assert.AreEqual("", models[0].RunInstanceGuid);
            Assert.AreEqual("name", models[0].FileName);
            Assert.AreEqual("", models[0].SaidKeyCode);
            Assert.AreEqual(time, models[0].MetricGeneratedDateTime);
            Assert.AreEqual(1, models[0].DatasetFileId);
            Assert.AreEqual(time, models[0].ProcessStartDateTime);
            Assert.AreEqual("", models[0].StatusCode);
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
    }
}
