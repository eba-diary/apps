using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sentry.data.Core.Interfaces;
using System.Linq.Expressions;
using System.Text;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetFileServiceTests : BaseCoreUnitTest
    {
        [TestMethod]
        public void DatasetFileExtention_ToDto()
        {
            // Arrange
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
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
            Assert.AreEqual(DatasetFile.ObjectStatus, dto.ObjectStatus);

        }

        /*
         * Test method 1 seeing if files are in descending order 
         */
        [TestMethod]
        public void DatasetFileService_GetAllNonDeletedDatasetFileBySchema_PageParameters_Ordering_Is_Descending()
        {
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            var user1 = new Mock<IApplicationUser>();  // creates a user
            user1.Setup(f => f.AssociateId).Returns("123456");  // sets up a user with a specified associative id


            var userSecurity = new UserSecurity();
            userSecurity.CanViewFullDataset = true;
            var securityService = new Mock<ISecurityService>();
            securityService.Setup(r => r.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(userSecurity);

            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user1.Object);

            // creation of the PageParameters object
            PageParameters pageParams = new PageParameters(1, 5, true); // descending case

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

            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, null, null, null, null, null);

            // Act
            PagedList<DatasetFileDto> dtoList = datasetFileService.GetNonDeletedDatasetFileDtoBySchema(23, pageParams);

            // Assert
            List<int> excludedIdList = Enumerable.Range(1, 5).ToList();
            List<int> includedIdList = Enumerable.Range(6, 5).ToList();

            Assert.AreEqual(false, dtoList.All(w => excludedIdList.Contains(w.DatasetFileId)));
            Assert.AreEqual(true, dtoList.All(w => includedIdList.Contains(w.DatasetFileId)));
        }

        /*
         * Test method 1 to see if files are in ascending order
         */
        [TestMethod]
        public void DatasetFileService_GetAllNonDeletedDatasetFileBySchema_PageParameters_Ordering_Is_Ascending()
        {
            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
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

            var messagePublisher = new Mock<IMessagePublisher>();


            PageParameters pageParams = new PageParameters(1, 5, false); // ascending case

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

            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, messagePublisher.Object, null, null, null, null);

            // Act
            PagedList<DatasetFileDto> dtoList = datasetFileService.GetNonDeletedDatasetFileDtoBySchema(23, pageParams);

            // Assert
            List<int> excludedIdList = Enumerable.Range(5, 9).ToList();
            List<int> includedIdList = Enumerable.Range(1, 5).ToList();

            Assert.AreEqual(false, dtoList.All(w => excludedIdList.Contains(w.DatasetFileId)));
            Assert.AreEqual(true, dtoList.All(w => includedIdList.Contains(w.DatasetFileId)));
        }




        [TestMethod]
        public void DatasetFileService_GetAllNonDeletedDatsetFileBySchema_DatasetUnauthorisedAccessException()
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
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

            var messagePublisher = new Mock<IMessagePublisher>();

            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, messagePublisher.Object, null, null, null, null);

            // Assert
            Assert.ThrowsException<DatasetUnauthorizedAccessException>(() => datasetFileService.GetNonDeletedDatasetFileDtoBySchema(23, pageParams));
        }

        [TestMethod]
        public void DatasetFileService_GetAllNonDeletedDatasetFilesBySchema_Schema_With_DataFiles()
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusActive).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            var messagePublisher = new Mock<IMessagePublisher>();


            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object,messagePublisher.Object, null, null, null, null);

            // Act
            var result = datasetFileService.GetNonDeletedDatasetFileDtoBySchema(23, pageParams);

            // Assert
            userService.VerifyAll();
            securityService.VerifyAll();
            context.VerifyAll();

            Assert.AreEqual(true, result.Any());
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(23, result.First().Schema);
        }

        [TestMethod]
        public void DatasetFileService_GetAllNonDeletedDatasetFilesBySchema_Schema_With_Pending_DataFiles()
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object, GlobalEnums.ObjectStatusEnum.Pending_Delete);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusActive).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            var messagePublisher = new Mock<IMessagePublisher>();


            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, messagePublisher.Object, null, null, null, null);

            // Act
            var result = datasetFileService.GetNonDeletedDatasetFileDtoBySchema(23, pageParams);

            // Assert
            userService.VerifyAll();
            securityService.VerifyAll();
            context.VerifyAll();

            Assert.AreEqual(true, result.Any());
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(23, result.First().Schema);
        }

        [TestMethod]
        public void DatasetFileService_GetAllActiveDatasetFilesBySchema_Schema_With_Active_DataFiles()
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusAll).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            var messagePublisher = new Mock<IMessagePublisher>();


            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, messagePublisher.Object, null, null, null, null);

            // Act
            var result = datasetFileService.GetActiveDatasetFileDtoBySchema(23, pageParams);

            // Assert
            userService.VerifyAll();
            securityService.VerifyAll();
            context.VerifyAll();

            Assert.AreEqual(true, result.Any());
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(23, result.First().Schema);
        }

        [TestMethod]
        public void DatasetFileService_GetAllActiveDatasetFilesBySchema_Schema_With_Deleted_DataFiles()
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object, GlobalEnums.ObjectStatusEnum.Deleted);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusAll).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            var messagePublisher = new Mock<IMessagePublisher>();


            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, messagePublisher.Object, null, null, null, null);

            // Act
            var result = datasetFileService.GetActiveDatasetFileDtoBySchema(23, pageParams);

            // Assert
            userService.VerifyAll();
            securityService.VerifyAll();
            context.VerifyAll();

            Assert.AreEqual(false, result.Any());
        }

        [TestMethod]
        public void DatasetFileService_GetAllDatasetFilesBySchema_Schema_With_DataFiles()
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            FileSchema schema = MockClasses.MockFileSchema();
            dfc.Schema = schema;

            //setup db context with DatasetFileConfig
            var context = new Mock<IDatasetContext>();
            context.Setup(f => f.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { dfc }.AsQueryable());

            var datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object, GlobalEnums.ObjectStatusEnum.Deleted);
            datasetFile.Schema = schema;
            context.Setup(f => f.DatasetFileStatusAll).Returns(new List<DatasetFile>() { datasetFile }.AsQueryable());

            PageParameters pageParams = new PageParameters(1, 5);

            var messagePublisher = new Mock<IMessagePublisher>();


            //Initialize Service
            var datasetFileService = new DatasetFileService(context.Object, securityService.Object, userService.Object, messagePublisher.Object, null, null, null, null);

            // Act
            var result = datasetFileService.GetAllDatasetFileDtoBySchema(23, pageParams);

            // Assert
            userService.VerifyAll();
            securityService.VerifyAll();
            context.VerifyAll();

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

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(null, null, userService.Object, messagePublisher.Object, null, null, null, null);

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

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object, messagePublisher.Object, null, null, null, null);

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.Dataset = 93;

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object,messagePublisher.Object, null, null, null, null);

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.Schema = 39;

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object,messagePublisher.Object, null, null, null, null);

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            dataFile.Schema = null;
            
            var datasetFileDto = MockClasses.MockDatasetFileDto();            

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object,messagePublisher.Object, null, null, null, null);

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            dataFile.SchemaRevision = null;

            var datasetFileDto = MockClasses.MockDatasetFileDto();

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object, messagePublisher.Object, null, null, null, null);

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            dataFile.SchemaRevision = null;

            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.SchemaRevision = 0;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);
            context.Setup(x => x.SaveChanges(true)).Verifiable();

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object, messagePublisher.Object, null, null, null, null);

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);

            var datasetFileDto = MockClasses.MockDatasetFileDto();
            datasetFileDto.SchemaRevision = 97;

            var context = new Mock<IDatasetContext>();
            context.Setup(d => d.GetById<DatasetFile>(3000)).Returns(dataFile);

            var messagePublisher = new Mock<IMessagePublisher>();

            var datasetFileService = new DatasetFileService(context.Object, null, userService.Object,messagePublisher.Object, null, null, null, null);

            // Assert
            Assert.ThrowsException<SchemaRevisionNotFoundException>(() => datasetFileService.UpdateAndSave(datasetFileDto));
        }

        [TestMethod]
        public void DeleteParquetFileByDatsetFile_DeleteDatasetFile()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Dataset ds = MockClasses.MockDataset();

            DatasetFileConfig dfc = new DatasetFileConfig()
            {
                ConfigId = 1,
                Schema = new FileSchema()
                {
                    SchemaId = 1
                }
            };

            Mock<IApplicationUser> user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            DateTime dtm = DateTime.Now;
            DatasetFile datasetFile = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile.FileKey = "test/rawquery";
            datasetFile.CreatedDTM = dtm;
            datasetFile.ModifiedDTM = dtm;

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>(MockBehavior.Strict);

            Mock<IS3ServiceProvider> s3ServiceProvider = mockRepository.Create<IS3ServiceProvider>();
            s3ServiceProvider.Setup(x => x.DeleteS3Prefix(It.IsAny<string>(), It.IsAny<string>()));

            DatasetFileService datasetFileService = new DatasetFileService(datasetContext.Object, null, null, null, s3ServiceProvider.Object, null, null, null);

            datasetFileService.DeleteParquetFileByDatsetFile(datasetFile);

            mockRepository.VerifyAll();
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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            Mock<IApplicationUser> user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.AssociateId).Returns("123456");

            DateTime dtm = DateTime.Now;
            DatasetFile datasetFile_To_Update = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile_To_Update.CreatedDTM = dtm;
            datasetFile_To_Update.ModifiedDTM = dtm;

            DatasetFile datasetFile_Original_Values = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            datasetFile_Original_Values.CreatedDTM = dtm;
            datasetFile_Original_Values.ModifiedDTM = dtm;

            var messagePublisher = new Mock<IMessagePublisher>();

            DatasetFileService datasetFileService = new DatasetFileService(null, null, null, messagePublisher.Object, null, null, null, null);

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

        [TestMethod]
        public void DatasetFileService_Delete()
        {
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFileA = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            DatasetFile dataFileB = MockClasses.MockDatasetFileB(ds, dfc, user1.Object);
            DatasetFile dataFileC = MockClasses.MockDatasetFileC(ds, dfc, user1.Object);
            Schema schema = MockClasses.MockFileSchema();

            var context = new Mock<IDatasetContext>();
            context.SetupGet(d => d.DatasetFileStatusAll).Returns(new List<DatasetFile>() { dataFileA, dataFileB,dataFileC }.AsQueryable);
            var messagePublisher = new Mock<IMessagePublisher>();

            //SETUP EVENT SERVICE CALLS
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);
            var eventService = new Mock<IEventService>();
            eventService.Setup(e => e.PublishEventByDatasetFileDelete(null, null, null));
            Mock<IDataFeatures> features = mockRepository.Create<IDataFeatures>();
            features.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("NonProd");

            var datasetFileService = new DatasetFileService(context.Object, null, null, messagePublisher.Object, null, eventService.Object, null, features.Object);

            DeleteFilesParamDto dto = new DeleteFilesParamDto();
            dto.UserFileIdList = new int[] { 3000 };

            //ENSURE ONLY A IS MARKED PENDING_DELETED
            datasetFileService.Delete(ds.DatasetId, schema.SchemaId, dto);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Pending_Delete, dataFileA.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileB.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileC.ObjectStatus);

            //ENSURE NOW C IS MARKED PENDING_DELETED
            dto.UserFileIdList = null;
            dto.UserFileNameList = new string[] { "c" };
            datasetFileService.Delete(ds.DatasetId, schema.SchemaId, dto);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileB.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Pending_Delete, dataFileC.ObjectStatus);


        }

        [TestMethod]
        public void DatasetFileService_UpdateObjectStatus()
        {
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFileA = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            DatasetFile dataFileB = MockClasses.MockDatasetFileB(ds, dfc, user1.Object);


            var context = new Mock<IDatasetContext>();
            var messagePublisher = new Mock<IMessagePublisher>();


            //SETUP EVENT SERVICE CALLS
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);
            var eventService = new Mock<IEventService>();
            eventService.Setup(e => e.PublishEventByDatasetFileDelete(null, null, null));

            var datasetFileService = new DatasetFileService(context.Object, null, null, messagePublisher.Object, null, eventService.Object, null, null);

            List<DatasetFile> dbList = new List<DatasetFile>();
            dbList.Add(dataFileA);

            //ENSURE MARKING PENDING DELETE WORKS
            datasetFileService.UpdateObjectStatus(dbList, Core.GlobalEnums.ObjectStatusEnum.Pending_Delete);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Pending_Delete, dataFileA.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileB.ObjectStatus);

            //ENSURE MARKING DELETE WORKS
            datasetFileService.UpdateObjectStatus(dbList, Core.GlobalEnums.ObjectStatusEnum.Deleted);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Deleted, dataFileA.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileB.ObjectStatus);
        }

        [TestMethod]
        public void UploadDatasetFileToS3_UploadDatasetFileDto_Success()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig datasetFileConfig = new DatasetFileConfig()
            {
                ConfigId = 1,
                Schema = new FileSchema()
                {
                    SchemaId = 2
                }
            };

            DataFlow dataFlow = new DataFlow()
            {
                DatasetId = 3,
                SchemaId = 2,
                ObjectStatus = GlobalEnums.ObjectStatusEnum.Active,
                Steps = new List<DataFlowStep>()
                {
                    new DataFlowStep()
                    {
                        DataAction_Type_Id = DataActionType.ProducerS3Drop,
                        TriggerBucket = "TriggerBucket",
                        TriggerKey = "TriggerKey/",
                    }
                }
            };

            Mock<Stream> stream = mockRepository.Create<Stream>();

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DatasetFileConfig>(1)).Returns(datasetFileConfig);
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());

            Mock<IS3ServiceProvider> s3ServiceProvider = mockRepository.Create<IS3ServiceProvider>();
            s3ServiceProvider.Setup(x => x.UploadDataFile(stream.Object, "TriggerBucket", "TriggerKey/FileName.json")).Returns("");

            DatasetFileService datasetFileService = new DatasetFileService(datasetContext.Object, null, null, null, s3ServiceProvider.Object, null, null, null);

            UploadDatasetFileDto dto = new UploadDatasetFileDto()
            {
                DatasetId = 3,
                ConfigId = 1,
                FileName = "FileName.json",
                FileInputStream = stream.Object
            };

            datasetFileService.UploadDatasetFileToS3(dto);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void UploadDatasetFileToS3_UploadDatasetFileDto_DatasetFileConfigNotFound()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DatasetFileConfig>(1)).Returns<DatasetFileConfig>(null);

            DatasetFileService datasetFileService = new DatasetFileService(datasetContext.Object, null, null, null, null, null, null, null);

            UploadDatasetFileDto dto = new UploadDatasetFileDto()
            {
                ConfigId = 1
            };

            Assert.ThrowsException<DataFlowNotFound>(() => datasetFileService.UploadDatasetFileToS3(dto), "Dataset File Config with Id: 1 not found while attempting to upload file to S3");

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void UploadDatasetFileToS3_UploadDatasetFileDto_DataFlowNotFound()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig datasetFileConfig = new DatasetFileConfig()
            {
                ConfigId = 1,
                Schema = new FileSchema()
                {
                    SchemaId = 2
                }
            };

            DataFlow dataFlow = new DataFlow()
            {
                DatasetId = 3,
                SchemaId = 2,
                ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted
            };

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.GetById<DatasetFileConfig>(1)).Returns(datasetFileConfig);
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());

            DatasetFileService datasetFileService = new DatasetFileService(datasetContext.Object, null, null, null, null, null, null, null);

            UploadDatasetFileDto dto = new UploadDatasetFileDto()
            {
                DatasetId = 3,
                ConfigId = 1
            };

            Assert.ThrowsException<DataFlowNotFound>(() => datasetFileService.UploadDatasetFileToS3(dto), "Data Flow for dataset: 3 and schema: 2 not found while attempting to upload file to S3");

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void DatasetFileService_UpdateObjectStatus_SingleFile()
        {
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFileA = MockClasses.MockDatasetFile(ds, dfc, user1.Object);
            DatasetFile dataFileB = MockClasses.MockDatasetFileB(ds, dfc, user1.Object);
            DatasetFile dataFileC = MockClasses.MockDatasetFileC(ds, dfc, user1.Object);


            var context = new Mock<IDatasetContext>();
            context.SetupGet(d => d.DatasetFileStatusAll).Returns(new List<DatasetFile>() { dataFileA, dataFileB, dataFileC }.AsQueryable);
            var messagePublisher = new Mock<IMessagePublisher>();

            //SETUP EVENT SERVICE CALLS
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);
            var eventService = new Mock<IEventService>();
            eventService.Setup(e => e.PublishEventByDatasetFileDelete(null, null, null));


            var datasetFileService = new DatasetFileService(context.Object, null, null, messagePublisher.Object,null, eventService.Object, null, null);


            //ENSURE MARKING Deleted WORKS
            datasetFileService.UpdateObjectStatus(new int[] {3000}, Core.GlobalEnums.ObjectStatusEnum.Deleted);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Deleted, dataFileA.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileB.ObjectStatus);


            //ENSURE MARKING Pending_Delete_Failure WORKS
            datasetFileService.UpdateObjectStatus(new int[] { 4000 }, Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Deleted, dataFileA.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure, dataFileB.ObjectStatus);

            //ENSURE DatasetFile Not found WORKS
            datasetFileService.UpdateObjectStatus(new int[] { 7000 }, Core.GlobalEnums.ObjectStatusEnum.Deleted);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Deleted, dataFileA.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure, dataFileB.ObjectStatus);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Active, dataFileC.ObjectStatus);

        }

        [TestMethod]
        public void DatasetFileService_EnsureDuplicatesWork()
        {
            var userService = new Mock<IUserService>();
            var user1 = new Mock<IApplicationUser>();
            user1.Setup(f => f.IsAdmin).Returns(true);
            userService.Setup(u => u.GetCurrentUser()).Returns(user1.Object);

            Dataset ds = MockClasses.MockDataset();
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(ds);
            DatasetFile dataFileA = MockClasses.MockDatasetFile(ds, dfc, user1.Object);


            var context = new Mock<IDatasetContext>();
            context.SetupGet(d => d.DatasetFileStatusAll).Returns(new List<DatasetFile>() { dataFileA}.AsQueryable);


            //SETUP EVENT SERVICE CALLS
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            Mock<IInstanceGenerator> contextGenerator = mockRepository.Create<IInstanceGenerator>();
            contextGenerator.Setup(x => x.GenerateInstance<IDatasetContext>()).Returns(context.Object);
            var eventService = new Mock<IEventService>();
            eventService.Setup(e => e.PublishEventByDatasetFileDelete(null,null,null));


            var messagePublisher = new Mock<IMessagePublisher>();
            var datasetFileService = new DatasetFileService(context.Object, null, null, messagePublisher.Object, null, eventService.Object, null, null);


            //ENSURE MARKING Deleted WORKS
            datasetFileService.UpdateObjectStatus(new int[] { 3000,3000 }, Core.GlobalEnums.ObjectStatusEnum.Deleted);
            Assert.AreEqual(Core.GlobalEnums.ObjectStatusEnum.Deleted, dataFileA.ObjectStatus);
        }

        [TestMethod]
        public void CheckHangFireDelayedJob_ReprocessingDataFiles()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IUserService> userService = mr.Create<IUserService>();
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            List<DatasetFile> datasetFileList = new List<DatasetFile>();
            List<DataFlowStep> dataFlowStepList = new List<DataFlowStep>();

            int stepId = 2;
            List<int> datasetFileIds = new List<int> { 3000, 3001};

            Dataset dataset = new Dataset();

            FileSchema fileSchema = MockClasses.MockFileSchema();

            DatasetFileConfig datasetFileConfig = MockClasses.MockDatasetFileConfig(schema: fileSchema);

            DatasetFile datasetFile = MockClasses.MockDatasetFile(dataset, datasetFileConfig, user.Object);
            datasetFile.FileKey = "rawquery/CRVS/PROD/8921001/2022/7/5/Structured_AgentEvent_20220705031726670_20220705201728000.json";
            datasetFile.FileBucket = "my-bucket-name1";
            datasetFile.FlowExecutionGuid = "202435326745400";
            datasetFile.OriginalFileName = "zzztest1853.csv";
            datasetFile.DatasetFileId = 3000;

            DatasetFile datasetFile2 = MockClasses.MockDatasetFile(dataset, datasetFileConfig, user.Object);
            datasetFile2.FileKey = "rawquery/CRVS/PROD/8921001/2022/7/5/Structured_AgentEvent_20220705031726670_20220705201728000.json";
            datasetFile2.FileBucket = "my-bucket-name2";
            datasetFile2.FlowExecutionGuid = "2435756478523456";
            datasetFile2.OriginalFileName = "zzztest4568.csv";
            datasetFile2.DatasetFileId = 3001;

            
            DataFlowStep dataFlowStep = new DataFlowStep()
            {
                TriggerKey = "TriggerKey/",
                Id = 2
            };

            
            var context = new Mock<IDatasetContext>();
            var scheduler = new Mock<IJobScheduler>();
            var s3serviceprovider = new Mock<IS3ServiceProvider>();

            datasetFileList.Add(datasetFile);
            datasetFileList.Add(datasetFile2);
            dataFlowStepList.Add(dataFlowStep);

            context.Setup(d => d.DatasetFileStatusActive).Returns(datasetFileList.AsQueryable());
            context.Setup(d => d.DataFlowStep).Returns(dataFlowStepList.AsQueryable());

            var datasetFileService = new DatasetFileService(context.Object, null, null, null, s3serviceprovider.Object, null, scheduler.Object, null);

            scheduler.Setup(d => d.Schedule<DatasetFileService>(It.IsAny<Expression<Action<DatasetFileService>>>(), It.Is<TimeSpan>((q) => q.Seconds == 30))).Returns(" ").Callback<Expression<Action<DatasetFileService>>, TimeSpan>(
                (w, t) =>
                {
                    Action<DatasetFileService> action = w.Compile();
                    action.Invoke(datasetFileService);
                });

            // Act
            bool result = datasetFileService.ScheduleReprocessing(stepId, datasetFileIds);
            
            // Assert
            context.VerifyAll();
            scheduler.VerifyAll();

            s3serviceprovider.Verify(d => d.UploadDataFile(It.IsAny<MemoryStream>(), It.IsAny<string>(), "TriggerKey/202435326745400/zzztest1853.csv.trg", It.IsAny<List<KeyValuePair<string, string>>>()), Times.Once());
            s3serviceprovider.Verify(d => d.UploadDataFile(It.IsAny<MemoryStream>(), It.IsAny<string>(), "TriggerKey/2435756478523456/zzztest4568.csv.trg", It.IsAny<List<KeyValuePair<string, string>>>()), Times.Once());

            Assert.IsTrue(result);
        }

        
        [TestMethod]
        public void CheckingBatchImplementation()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IUserService> userService = mr.Create<IUserService>();
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            List<DatasetFile> datasetFileList = new List<DatasetFile>();
            List<DataFlowStep> dataFlowStepList = new List<DataFlowStep>();

            int stepId = 2;

            
            Dataset dataset = new Dataset();

            FileSchema fileSchema = MockClasses.MockFileSchema();

            DatasetFileConfig datasetFileConfig = MockClasses.MockDatasetFileConfig(schema: fileSchema);

            // list of 101 datasetFileIds to test batch logic in schedule reprocessing
            List<int> datasetFileIds = new List<int>();
            for (int i = 3000; i <= 3100; i++)
            {
                datasetFileIds.Add(i);

                DatasetFile datasetFile = MockClasses.MockDatasetFile(dataset, datasetFileConfig, user.Object);
                datasetFile.FileKey = "rawquery/CRVS/PROD/8921001/2022/7/5/Structured_AgentEvent_20220705031726670_20220705201728000.json";
                datasetFile.FileBucket = "my-bucket-name2";
                datasetFile.FlowExecutionGuid = "2435756478523456";
                datasetFile.OriginalFileName = "zzztest4568.csv";
                datasetFile.DatasetFileId = i;
                datasetFileList.Add(datasetFile);
            }

            DataFlowStep dataFlowStep = new DataFlowStep()
            {
                TriggerKey = "TriggerKey/",
                Id = 2
            };


            var context = new Mock<IDatasetContext>();
            var scheduler = new Mock<IJobScheduler>();
            var s3serviceprovider = new Mock<IS3ServiceProvider>();

            dataFlowStepList.Add(dataFlowStep);

            context.Setup(d => d.DatasetFileStatusActive).Returns(datasetFileList.AsQueryable());
            context.Setup(d => d.DataFlowStep).Returns(dataFlowStepList.AsQueryable());

            var datasetFileService = new DatasetFileService(context.Object, null, null, null, s3serviceprovider.Object, null, scheduler.Object, null);
            
            scheduler.Setup(d => d.Schedule<DatasetFileService>(It.IsAny<Expression<Action<DatasetFileService>>>(), It.IsAny<TimeSpan>())).Returns(" ").Callback<Expression<Action<DatasetFileService>>, TimeSpan>(
                (w, t) =>
                {
                    Action<DatasetFileService> action = w.Compile();
                    action.Invoke(datasetFileService);
                });
            
            // Act
            bool result = datasetFileService.ScheduleReprocessing(stepId, datasetFileIds);

            // Assert
            context.VerifyAll();
            scheduler.VerifyAll();
            scheduler.Verify(d => d.Schedule<DatasetFileService>(It.IsAny<Expression<Action<DatasetFileService>>>(), It.Is<TimeSpan>((q) => q.TotalSeconds == 30)), Times.Exactly(100));
            scheduler.Verify(d => d.Schedule<DatasetFileService>(It.IsAny<Expression<Action<DatasetFileService>>>(), It.Is<TimeSpan>((q) => q.TotalSeconds == 60)), Times.Once());
            
            Assert.IsTrue(result);
        }
        
        
    }
}
