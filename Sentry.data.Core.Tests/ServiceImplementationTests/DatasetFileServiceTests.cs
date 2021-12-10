using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetFileServiceTests
    {
        [TestMethod]
        public void DatasetFileExtention_ToDto()
        {
            // Arrange
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            DatasetFile DatasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);

            // Act
            DatasetFileDto dto = DatasetFile.ToDto();

            //Assert
            Assert.AreEqual(DatasetFile.DatasetFileId, dto.DatasetFileId);
            Assert.AreEqual(DatasetFile.FileName, dto.FileName);
            Assert.AreEqual(DatasetFile.Dataset.DatasetId, dto.Dataset);
            Assert.AreEqual(DatasetFile.SchemaRevision.SchemaRevision_Id, dto.SchemaRevision);
            Assert.AreEqual(DatasetFile.DatasetFileConfig.ConfigId, dto.DatasetFileConfig);
            Assert.AreEqual(DatasetFile.UploadUserName, dto.UploadUserName);
            Assert.AreEqual(DatasetFile.CreateDTM, dto.CreateDTM);
            Assert.AreEqual(DatasetFile.ModifiedDTM, dto.ModifiedDTM);
            Assert.AreEqual(DatasetFile.FileLocation, dto.FileLocation);
            Assert.AreEqual(DatasetFile.ParentDatasetFileId, dto.ParentDatasetFileId);
            Assert.AreEqual(DatasetFile.VersionId, dto.VersionId);
            Assert.AreEqual(DatasetFile.Information, dto.Information);
            Assert.AreEqual(DatasetFile.Size, dto.Size);
            Assert.AreEqual(DatasetFile.FlowExecutionGuid, dto.FlowExecutionGuid);
            Assert.AreEqual(DatasetFile.RunInstanceGuid, dto.RunInstanceGuid);
            Assert.AreEqual(DatasetFile.FileExtension, dto.FileExtension);

        }

        [TestMethod]
        public void DatasetFileService_GetAllDatasetFilesBySchema_Schema_Without_DataFlies()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var datasetFileArray = new List<DatasetFile>();
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            datasetFileArray.Add(MockClasses.MockDatasetFile(ds, dfc, user1.Object));

            context.Setup(f => f.DatasetFile).Returns(datasetFileArray.AsQueryable());

            var datasetFileService = new DatasetFileService(context.Object);

            // Act
            var result = datasetFileService.GetAllDatasetFilesBySchema(11);

            // Assert
            Assert.AreEqual(false, result.Any());
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void DatasetFileService_GetAllDatasetFilesBySchema_Schema_With_DataFlies()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var datasetFileArray = new List<DatasetFile>();
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            datasetFileArray.Add(MockClasses.MockDatasetFile(ds, dfc, user1.Object));

            context.Setup(f => f.DatasetFile).Returns(datasetFileArray.AsQueryable());

            var datasetFileService = new DatasetFileService(context.Object);

            // Act
            var result = datasetFileService.GetAllDatasetFilesBySchema(23);

            // Assert
            Assert.AreEqual(true, result.Any());
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(23, result.First().Schema);
        }
    }
}
