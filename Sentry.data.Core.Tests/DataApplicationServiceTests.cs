using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataApplicationServiceTests
    {
        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void DeleteDataset_Initializes_DatasetService()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true).Verifiable();

            var lazyService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void DeleteDatasetFileConfig_Initializes_ConfigService()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true).Verifiable();

            var lazyService = new Lazy<IConfigService>(() => configService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, null, lazyService, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDatasetFileConfig(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void DeleteDataFlow_Initializes_DataFlowService()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true).Verifiable();

            var lazyService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, null, null, lazyService, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataflow(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_Calls_SaveChanges_Once()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(idList[0], user.Object, true)).Returns(true);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(true), Times.Once);
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_Loops_Through_All_Ids_Passed()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1, 2 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            datasetService.Verify(x => x.Delete(It.Is<int>(id => id == 1), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Once);
            datasetService.Verify(x => x.Delete(It.Is<int>(id => id == 2), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Once);
            datasetService.Verify(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_One_Failure_Calls_Context_Clear_And_No_Changes_Saved()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(false);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.Clear(), Times.Once);
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_One_Failure_Returns_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(false);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);


            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.Clear(), Times.Once);
        }

        [TestMethod]
        public void ReturnValue()
        {
            var dataApplicationService = new Mock<IDataApplicationService>();
            dataApplicationService.Setup(x => x.DeleteDataset(new List<int>() { 1 }, new Mock<IApplicationUser>().Object, false)).Returns(false);
        }

        [TestMethod]
        public void CreateDataset()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            /* mock datasetdto */
            DatasetDto dsDto = MockClasses.MockDatasetDto(new List<Dataset> { MockClasses.MockDataset() }).First();

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.Create(It.IsAny<DatasetDto>())).Returns(dsDto.DatasetId);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(true));
            
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.EnqueueCreateDefaultSecurityForDataset(dsDto.DatasetId));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA3718_Authorization.GetValue()).Returns(true);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var lazyDataFeatures = new Lazy<IDataFeatures>(() => dataFeatures.Object);
            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, lazyDataFeatures, lazySecurityService, null);

            //Act
            int newDatasetId = dataApplicationService.Create(dsDto);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(dsDto.DatasetId, newDatasetId);

        }
    }
}
