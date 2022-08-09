using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Web.Controllers;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DataFlowControllerTests
    {
        [TestMethod]
        public void Map_DataFlow_Model_To_Dto()
        {
            //Arrange
            var user = new Mock<IApplicationUser>();
            user.Setup(s => s.AssociateId).Returns("123456");

            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            DataFlowModel flowModel = MockClasses.MockDataFlowModel();

            var dataFlowController = new DataFlowController(null, null, null, null, null, null, null, null, userService.Object);

            
            //Act
            DataFlowDto dto = dataFlowController.ModelToDto(flowModel);
            DataFlowDto toBeSearilalized = dto;
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
            Assert.AreEqual(flowModel.SchemaMaps[0], dto.SchemaMap, "SchemaMap");
            Assert.AreEqual(flowModel.RetrieverJob, dto.RetrieverJob, "RetrieverJob");
            Assert.AreEqual(JsonConvert.SerializeObject(toBeSearilalized), dto.DFQuestionnaire, "DFQuestionnaire");            
        }
    }
}
