using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
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
            Assert.AreEqual(DatasetFile.CreatedDTM, dto.CreateDTM);
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
        public void DatasetFileService_GetAllDatasetFileBySchema_PageParameters_Ordering_Is_Descending()
        {
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");


            var userSecurity = new UserSecurity();
            userSecurity.CanViewFullDataset = true;
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(r => r.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(userSecurity);

            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user1.Object);

            PageParameters pageParams = new PageParameters(1, 5, true); // descending

            var context = new Mock<IDatasetContext>();
            var datasetFileArray = new List<DatasetFile>();
            //Add multiple files to array
            for (int i = 1; (i - 1) < 10; i++)
            {
                DatasetFile file = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
                file.DatasetFileId = i;
                file.Schema = schema;
                datasetFileArray.Add(file);
            }
            context.Setup(f => f.DatasetFileStatusActive).Returns(datasetFileArray.AsQueryable());

            IQueryable<DatasetFileConfig> configQueryable = new List<DatasetFileConfig>() { dfc }.AsQueryable();
            context.Setup(f => f.DatasetFileConfigs).Returns(configQueryable);

            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object);

            // Act
            PagedList<DatasetFileDto> dtoList = datasetFileService.GetAllDatasetFileDtoBySchema(23, pageParams);

            // Assert
            List<int> excludedIdList = Enumerable.Range(1, 5).ToList();
            List<int> includedIdList = Enumerable.Range(6, 5).ToList();

            Assert.AreEqual(false, dtoList.All(w => excludedIdList.Contains(w.DatasetFileId)));
            Assert.AreEqual(true, dtoList.All(w => includedIdList.Contains(w.DatasetFileId)));
        }

        [TestMethod]
        public void DatasetFileService_GetAllDaatsetFileBySchema_PageParameters_Ordering_Is_Ascending()
        {
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");
            

            var userSecurity = new UserSecurity();
            userSecurity.CanViewFullDataset = true;
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(r => r.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(userSecurity);

            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user1.Object);

            PageParameters pageParams = new PageParameters(1, 5, false); // ascending

            var context = new Mock<IDatasetContext>();
            var datasetFileArray = new List<DatasetFile>();
            //Add multiple files to array
            for (int i = 1; (i - 1) < 10; i++)
            {
                DatasetFile file = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
                file.DatasetFileId = i;
                file.Schema = schema;
                datasetFileArray.Add(file);
            }
            context.Setup(f => f.DatasetFileStatusActive).Returns(datasetFileArray.AsQueryable());

            IQueryable<DatasetFileConfig> configQueryable = new List<DatasetFileConfig>() { dfc }.AsQueryable();
            context.Setup(f => f.DatasetFileConfigs).Returns(configQueryable);

            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object);

            // Act
            PagedList<DatasetFileDto> dtoList = datasetFileService.GetAllDatasetFileDtoBySchema(23, pageParams);

            // Assert
            List<int> excludedIdList = Enumerable.Range(6, 5).ToList();
            List<int> includedIdList = Enumerable.Range(1, 5).ToList();

            Assert.AreEqual(false, dtoList.All(w => excludedIdList.Contains(w.DatasetFileId)));
            Assert.AreEqual(true, dtoList.All(w => includedIdList.Contains(w.DatasetFileId)));
        }
        
        
        

        [TestMethod]
        public void DatasetFileService_GetAllDatsetFileBySchema_DatasetUnauthorisedAccessException()
        {
            // Arrange
            //setup user
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            //setup userService
            var userService = new Mock<IUserService>();            
            userService.Setup(s => s.GetCurrentUser()).Returns(user1.Object);

            //setup securityService
            var userSecurity = new UserSecurity
            {
                CanViewFullDataset = false
            };
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(r => r.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(userSecurity);

            //setup Entity objects 
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;


            //setup db context with DatasetFile, DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusActive).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            IQueryable<DatasetFileConfig> configQueryable = new List<DatasetFileConfig>() { dfc }.AsQueryable();
            context.Setup(f => f.DatasetFileConfigs).Returns(configQueryable);

            PageParameters pageParams = new PageParameters(1, 5);

            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object);

            // Assert
            Assert.ThrowsException<DatasetUnauthorizedAccessException>(() => datasetFileService.GetAllDatasetFileDtoBySchema(23, pageParams));
        }



        [TestMethod]
        public void DatasetFileService_GetAllDatasetFilesBySchema_Schema_Without_DataFlies()
        {
            // Arrange
            //setup user
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            //setup userService
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user1.Object);

            //setup securityService
            var userSecurity = new UserSecurity
            {
                CanViewFullDataset = true
            };
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(r => r.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(userSecurity);

            //setup Entity objects
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object);

            // Act
            var result = datasetFileService.GetAllDatasetFileDtoBySchema(23, pageParams);

            // Assert
            Assert.AreEqual(false, result.Any());
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void DatasetFileService_GetAllDatasetFilesBySchema_Schema_With_DataFlies()
        {
            // Arrange
            //setup user
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            //setup userService
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user1.Object);

            //setup securityService
            var userSecurity = new UserSecurity
            {
                CanViewFullDataset = true
            };
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(r => r.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(userSecurity);

            //setup Entity objects
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusActive).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object);

            // Act
            var result = datasetFileService.GetAllDatasetFileDtoBySchema(23, pageParams);

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
        public void DatasetFileService_UpdateAndSave_DataFileNotFoundException()
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
            Assert.ThrowsException<DataFileNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_DataseNotFound()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);


            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.Dataset = 93;

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            // Assert
            Assert.ThrowsException<DatasetNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_SchemaNotFoundException_Incorrect_SchemaId()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.Schema = 39;

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            // Assert
            Assert.ThrowsException<SchemaNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_SchemaNotFoundException_Null_Schema_On_DataFile()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            dataFile.Schema = null;
            
            var datasetFileDto = MockClasses.MockDatasetFileDto();            

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            // Assert
            Assert.ThrowsException<SchemaNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_SchemaRevisionNotFoundException_Null_SchemaRevision_On_DataFile()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            dataFile.SchemaRevision = null;

            var datasetFileDto = MockClasses.MockDatasetFileDto();

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            // Assert
            Assert.ThrowsException<SchemaRevisionNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_SchemaRevisionNotFoundException_Null_SchemaRevision_On_DataFile_and_Dto()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IUserService> userService = mr.Create<IUserService>();
            Mock<IApplicationUser> user1 = mr.Create<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            user1.SetupGet(x => x.AssociateId).Returns("000000");
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            dataFile.SchemaRevision = null;

            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.SchemaRevision = 0;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);
            context.Setup(x => x.SaveChanges(true)).Verifiable();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            //Act
            datasetFileService.UpdateAndSave(datasetFileDto);

            // Assert
            mr.VerifyAll();
        }

        [TestMethod]
        public void DatasetFileService_UpdateAndSave_SchemaRevisionNotFoundException_Incorrect_SchemaRevisionID_On_Dto()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);

            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.SchemaRevision = 97;

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object);

            // Assert
            Assert.ThrowsException<SchemaRevisionNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DatasetFileService_UpdateDataFile()
        {
            // Arrage
            DatasetFileDto datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.FileLocation = "my/target/location.txt";
            datasetFileDto.VersionId = "my_New_versionid_12312445";
            datasetFileDto.FileKey = "my/target/key/location.txt";
            datasetFileDto.FileBucket = "my-bucket-name";
            datasetFileDto.ETag = "my-etag-string";

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(ds);
            Mock<IApplicationUser> user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            DateTime dtm = DateTime.Now;
            DatasetFile datasetFile_To_Update = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile_To_Update.CreatedDTM = dtm;
            datasetFile_To_Update.ModifiedDTM = dtm;

            DatasetFile datasetFile_Original_Values = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile_Original_Values.CreatedDTM = dtm;
            datasetFile_Original_Values.ModifiedDTM = dtm;

            DatasetFileService datasetFileService = new DatasetFileService(null, null, null);

            // Act
            datasetFileService.UpdateDataFile(datasetFileDto, datasetFile_To_Update);

            // Assert
            //These properties values should remaing intact
            Assert.AreEqual(datasetFile_Original_Values.DatasetFileId,              datasetFile_To_Update.DatasetFileId);
            Assert.AreEqual(datasetFile_Original_Values.FileName,                   datasetFile_To_Update.FileName);
            Assert.AreEqual(datasetFile_Original_Values.Dataset.DatasetId,          datasetFile_To_Update.Dataset.DatasetId);
            Assert.AreEqual(datasetFile_Original_Values.CreatedDTM,                 datasetFile_To_Update.CreatedDTM);
            Assert.AreEqual(datasetFile_Original_Values.ModifiedDTM,                datasetFile_To_Update.ModifiedDTM);
            Assert.AreEqual(datasetFile_Original_Values.DatasetFileConfig.ConfigId, datasetFile_To_Update.DatasetFileConfig.ConfigId);
            Assert.AreEqual(datasetFile_Original_Values.IsBundled,                  datasetFile_To_Update.IsBundled);
            Assert.AreEqual(datasetFile_Original_Values.ParentDatasetFileId,        datasetFile_To_Update.ParentDatasetFileId);
            Assert.AreEqual(datasetFile_Original_Values.Information,                datasetFile_To_Update.Information);
            Assert.AreEqual(datasetFile_Original_Values.Size,                       datasetFile_To_Update.Size);
            Assert.AreEqual(datasetFile_Original_Values.FlowExecutionGuid,          datasetFile_To_Update.FlowExecutionGuid);
            Assert.AreEqual(datasetFile_Original_Values.RunInstanceGuid,            datasetFile_To_Update.RunInstanceGuid);
            Assert.AreEqual(datasetFile_Original_Values.FileExtension,              datasetFile_To_Update.FileExtension);
            Assert.AreEqual(datasetFile_Original_Values.Schema.SchemaId,            datasetFile_To_Update.Schema.SchemaId);
            Assert.AreEqual(datasetFile_Original_Values.ETag,                       datasetFile_To_Update.ETag);

            //These values should be updated to value of incoming Dto Object
            Assert.AreEqual(datasetFileDto.FileLocation,    datasetFile_To_Update.FileLocation);
            Assert.AreEqual(datasetFileDto.FileKey,         datasetFile_To_Update.FileKey);
            Assert.AreEqual(datasetFileDto.FileBucket,      datasetFile_To_Update.FileBucket);
            Assert.AreEqual(datasetFileDto.VersionId,       datasetFile_To_Update.VersionId);
        }
    }
}
