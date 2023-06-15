using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.FeatureFlags;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class FileDeleteEventHandlerTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public void Process_Success_For_Delete_Failure_FileNotFoundParquet_Deleted_Sucessfully()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();            

            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));
            
            string mockMessage = GetDataString("FileDelete/MultipleFiles_ParquetNotFound.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object,mockDataFileService.Object,mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.AtMost(2));
            mockDataFileService.VerifyAll();
        }

        [TestMethod]
        public void Process_LegitFailure_For_Delete_Failure()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/ParquetFailed.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Once);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.AtMost(1));
            mockDataFileService.VerifyAll();
        }

        [TestMethod]
        public void Process_OverallFailure_But_Parquet_Success()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/OverallFailure_ParquetSuccess.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Once);
            mockDataFileService.VerifyAll();
        }

        [TestMethod]
        public void Process_OverallFailure_But_Parquet_Failure_Raw_Success_MakeSureStillDeleteFailure()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/OverallFailure_ParquetFailure.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Once);
            mockDataFileService.VerifyAll();
        }

        [TestMethod]
        public void Process_Parquet_Not_Found()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/ParquetNotFound.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Once);
            mockDataFileService.VerifyAll();

        }

        [TestMethod]
        public void Process_NoDeleteProcessStatus()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/NoDeleteProcessStatus.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Once);
            mockDataFileService.VerifyAll();

        }

        [TestMethod]
        public void Missing_DeleteProcessStatusPerIDProcess()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/NoDeleteProcessStatusPerID.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Never);

            string mockMessage2 = GetDataString("FileDelete/EmptyDeleteProcessStatusPerID.json");
            handle.HandleLogic(mockMessage2);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Never);
        }

        [TestMethod]
        public void Empty_DeleteProcessStatusPerIDProcess()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/EmptyDeleteProcessStatusPerID.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Never);
        }

        [TestMethod]
        public void Bad_DatasetFileIdDeleteStatus()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/BadDatasetFileDropIdDeleteStatus.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Deleted), Times.Once);
        }

        [TestMethod]
        public void FeatureFlagWorks()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(false);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/ParquetNotFound.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
        }


        [TestMethod]
        public void Process_Mutiples_success()
        {
            //ARRANGE
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IEventService> mockEventService = mockRepository.Create<IEventService>();
            Mock<IDatasetFileService> mockDataFileService = mockRepository.Create<IDatasetFileService>();


            //mock features
            Mock<IFeatureFlag<bool>> feature = mockRepository.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> mockDataFeatureService = mockRepository.Create<IDataFeatures>();
            mockDataFeatureService.SetupGet(x => x.CLA4049_ALLOW_S3_FILES_DELETE).Returns(feature.Object);
            mockDataFileService.Setup(x => x.UpdateObjectStatus(It.IsAny<int[]>(), It.IsAny<Core.GlobalEnums.ObjectStatusEnum>()));

            string mockMessage = GetDataString("FileDelete/MultipleFiles_Success.json");
            FileDeleteEventHandler handle = new FileDeleteEventHandler(mockEventService.Object, mockDataFileService.Object, mockDataFeatureService.Object);
            handle.HandleLogic(mockMessage);

            //VERIFY
            mockDataFileService.Verify(x => x.UpdateObjectStatus(It.IsAny<int[]>(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure), Times.Never);
            mockDataFileService.VerifyAll();
        }

    }
}
