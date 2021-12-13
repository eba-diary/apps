using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Exceptions;
using System;
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

            var datasetFileService = new DatasetFileService(context.Object, null, null);

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

            var datasetFileService = new DatasetFileService(context.Object, null, null);

            // Act
            var result = datasetFileService.GetAllDatasetFilesBySchema(23);

            // Assert
            Assert.AreEqual(true, result.Any());
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(23, result.First().Schema);
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_UnauthorizedException()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(false);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            var datasetFileService = new DatasetFileService(null, null, userService.Object);

            //Assert
            Assert.ThrowsException<DataFileUnauthorizedException>(() => datasetFileService.UpdateAndSave(null));
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_DataseNotFound()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            var context = new Mock<IDatasetContext>();

            var datasetFileDto = MockClasses.MockDatasetFileDto();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            // Assert
            Assert.ThrowsException<DatasetNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateDataFile()
        {
            // Arrage
            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.FileLocation = "target/location.txt";

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);

            datasetFile.DatasetFileId = 98;
            datasetFile.FileLocation = "original/location.zip";
            datasetFile.Dataset.DatasetId = 82;
            datasetFile.FileName = "location.zip";
            datasetFile.VersionId = "qwerty-asdf9320123n90afs";
            datasetFile.Information = "Austin is the Man!";
            datasetFile.Size = 123456;

            DateTime dtm = DateTime.Now;
            datasetFile.CreateDTM = dtm;
            datasetFile.ModifiedDTM = dtm;
            datasetFile.DatasetFileConfig.ConfigId = 193;
            datasetFile.FlowExecutionGuid = "12340945576";
            datasetFile.RunInstanceGuid = "12345697853";
            datasetFile.FileExtension = "csv";
            datasetFile.Schema.SchemaId = 264;

            var datasetFileService = new DatasetFileService(null, null, null);

            // Act
            datasetFileService.UpdateDataFile(datasetFileDto, datasetFile);

            // Assert
            Assert.AreEqual("target/location.txt", datasetFile.FileLocation);
            Assert.AreEqual(98, datasetFile.DatasetFileId);
            Assert.AreEqual("location.zip", datasetFile.FileName);
            Assert.AreEqual(82, datasetFile.Dataset.DatasetId);
            Assert.AreEqual(dtm, datasetFile.CreateDTM);
            Assert.AreEqual(dtm, datasetFile.ModifiedDTM);
            Assert.AreEqual(193, datasetFile.DatasetFileConfig.ConfigId);
            Assert.AreEqual(false, datasetFile.IsBundled);
            Assert.AreEqual(23, datasetFile.ParentDatasetFileId);
            Assert.AreEqual("qwerty-asdf9320123n90afs", datasetFile.VersionId);
            Assert.AreEqual("Austin is the Man!", datasetFile.Information);
            Assert.AreEqual(123456, datasetFile.Size);
            Assert.AreEqual("12340945576", datasetFile.FlowExecutionGuid);
            Assert.AreEqual("12345697853", datasetFile.RunInstanceGuid);
            Assert.AreEqual("csv", datasetFile.FileExtension);
            Assert.AreEqual(264, datasetFile.Schema.SchemaId);

        }
    }
}
