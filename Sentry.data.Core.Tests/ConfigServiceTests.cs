using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.DTO.Retriever;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class ConfigServiceTests
    {
        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Does_Not_Call_Save_Changes()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(dfc.ParentDataset, user.Object));

            var configService = new ConfigService(context.Object,null,null,null,null,null,null,null,null,null,null,null,null);

            //Act
            configService.Delete(dfc.ConfigId, user.Object, true);

            //Assert
            context.Verify(x => x.SaveChanges(true), Times.Never);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Object_Marked_PendingDelete_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, true);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Does_Not_Call_SaveChanges_When_Incoming_Object_Marked_PendingDelete_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            configService.Delete(dfc.ConfigId, user.Object, true);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Object_Marked_Deleted_And_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Deleted;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Deleted;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Does_Not_Call_SaveChanges_When_Incoming_Object_Marked_Deleted_And_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Deleted;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Deleted;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Passes_Incoming_User_Info_To_DataFlowService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            var dataflows = new[] { MockClasses.MockDataFlow() };
            var schemaMaps = new[] { new SchemaMap() {
                Id = 1,
                DataFlowStepId = new DataFlowStep() 
                    { 
                        Id = 1, 
                        Action = new SchemaLoadAction() { Name = "Schema Load Action" },
                        DataFlow = MockClasses.MockDataFlow()
                    },
                Dataset = dfc.ParentDataset,
                MappedSchema = schema
                } 
            };

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.DataFlow).Returns(dataflows.AsQueryable());
            context.Setup(s => s.SchemaMap).Returns(schemaMaps.AsQueryable());

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);
            dataFlowService.Setup(s => s.GetDataFlowNameForFileSchema(It.IsAny<FileSchema>())).Returns("DataflowName");

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, dataFlowService.Object, null, null);

            // Act
            configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), user.Object, false), Times.Once);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Passes_Null_User_Info_To_DataFlowService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            var dataflows = new[] { MockClasses.MockDataFlow() };
            var schemaMaps = new[] { new SchemaMap() {
                Id = 1,
                DataFlowStepId = new DataFlowStep()
                    {
                        Id = 1,
                        Action = new SchemaLoadAction() { Name = "Schema Load Action" },
                        DataFlow = MockClasses.MockDataFlow()
                    },
                Dataset = dfc.ParentDataset,
                MappedSchema = schema
                }
            };

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.DataFlow).Returns(dataflows.AsQueryable());
            context.Setup(s => s.SchemaMap).Returns(schemaMaps.AsQueryable());

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);
            dataFlowService.Setup(s => s.GetDataFlowNameForFileSchema(It.IsAny<FileSchema>())).Returns("DataflowName");

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, dataFlowService.Object, null, null);

            // Act
            configService.Delete(dfc.ConfigId, null, false);

            // Assert
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), null, false), Times.Once);
        }

        [TestMethod]
        public void CreateAndSaveNewDataSource_DataSourceDto_True()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();

            AuthenticationType authType = new OAuthAuthentication();
            datasetContext.Setup(x => x.GetById<AuthenticationType>(2)).Returns(authType);

            DataSource dataSource = null;
            datasetContext.Setup(x => x.Add(It.IsAny<DataSource>())).Callback<DataSource>(x => dataSource = x);

            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.iss && c.Value == "ClientId")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.scope && c.Value == "token.scope")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.aud && c.Value == "https://www.token.com")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.exp)));

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IEncryptionService> encryptionService = mock.Create<IEncryptionService>();
            encryptionService.Setup(x => x.GenerateNewIV()).Returns("IVKey");
            encryptionService.Setup(x => x.EncryptString("DecryptedClientPrivateId", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedClientPrivateId", ""));
            encryptionService.Setup(x => x.EncryptString("DecryptedCurrentToken", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedCurrentToken", ""));
            encryptionService.Setup(x => x.EncryptString("DecryptedRefreshToken", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedRefreshToken", ""));

            Mock<IUserService> userService = mock.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("000000");

            Mock<IEventService> eventService = mock.Create<IEventService>();
            eventService.Setup(x => x.PublishSuccessEvent(GlobalConstants.EventType.CREATED_DATASOURCE, "DataFlowName was created.", null)).Returns<Task>(null);

            ConfigService configService = new ConfigService(datasetContext.Object, userService.Object, eventService.Object, null, encryptionService.Object, null, null, null, null, null, null, null, null);

            DataSourceDto dataSourceDto = new DataSourceDto
            {
                Name = "DataFlowName",
                AuthID = "2",
                SourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                HasPaging = true,
                RequestHeaders = new List<RequestHeader>
                {
                    new RequestHeader { Index = "0", Key = "HeaderKey", Value = "HeaderValue" }
                },
                ClientId = "ClientId",
                ClientPrivateId = "DecryptedClientPrivateId",
                Tokens = new List<DataSourceTokenDto>
                {
                    new DataSourceTokenDto
                    {
                        CurrentToken = "DecryptedCurrentToken",
                        RefreshToken = "DecryptedRefreshToken",
                        CurrentTokenExp = new DateTime(2022, 11, 18, 8, 0, 0),
                        TokenExp = 100,
                        TokenName = "TokenName",
                        TokenUrl = "https://www.token.com",
                        Scope = "token.scope"
                    }
                },
                GrantType = OAuthGrantType.JwtBearer,
                Description = "Data Flow Description",
                IsUserPassRequired = false,
                BaseUri = new Uri("https://www.google.com"),
                PortNumber = 443,
                IsSecured = true,
                PrimaryContactId = "000000"
            };

            bool result = configService.CreateAndSaveNewDataSource(dataSourceDto);

            Assert.IsTrue(result);

            Assert.AreEqual("DataFlowName", dataSource.Name);
            Assert.AreEqual("Data Flow Description", dataSource.Description);
            Assert.AreEqual(authType, dataSource.SourceAuthType);
            Assert.IsFalse(dataSource.IsUserPassRequired);
            Assert.AreEqual("https://www.google.com/", dataSource.BaseUri.ToString());
            Assert.AreEqual(443, dataSource.PortNumber);
            Assert.IsTrue(dataSource.IsSecured);
            Assert.AreEqual("000000", dataSource.PrimaryContactId);
            Assert.IsNotNull(dataSource.Security);
            Assert.AreEqual("000000", dataSource.Security.CreatedById);

            Assert.IsInstanceOfType(dataSource, typeof(HTTPSSource));

            HTTPSSource http = (HTTPSSource)dataSource;
            Assert.AreEqual("IVKey", http.IVKey);
            Assert.IsTrue(http.HasPaging);
            Assert.AreEqual(1, http.RequestHeaders.Count);

            RequestHeader header = http.RequestHeaders.First();
            Assert.AreEqual("0", header.Index);
            Assert.AreEqual("HeaderKey", header.Key);
            Assert.AreEqual("HeaderValue", header.Value);

            Assert.AreEqual("ClientId", http.ClientId);
            Assert.AreEqual("EncryptedClientPrivateId", http.ClientPrivateId);
            Assert.AreEqual(OAuthGrantType.JwtBearer, http.GrantType);
            Assert.AreEqual(1, http.Tokens.Count);
            
            DataSourceToken dataSourceToken = http.Tokens.First();
            Assert.AreEqual("EncryptedCurrentToken", dataSourceToken.CurrentToken);
            Assert.AreEqual("EncryptedRefreshToken", dataSourceToken.RefreshToken);
            Assert.AreEqual(new DateTime(2022, 11, 18, 8, 0, 0), dataSourceToken.CurrentTokenExp);
            Assert.AreEqual(100, dataSourceToken.TokenExp);
            Assert.AreEqual("TokenName", dataSourceToken.TokenName);
            Assert.AreEqual("https://www.token.com", dataSourceToken.TokenUrl);
            Assert.AreEqual("token.scope", dataSourceToken.Scope);
            Assert.IsNotNull(dataSourceToken.ParentDataSource);
            Assert.AreEqual(4, dataSourceToken.Claims.Count);

            OAuthClaim claim = dataSourceToken.Claims.First();
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.iss, claim.Type);
            Assert.AreEqual("ClientId", claim.Value);

            claim = dataSourceToken.Claims[1];
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.scope, claim.Type);
            Assert.AreEqual("token.scope", claim.Value);

            claim = dataSourceToken.Claims[2];
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.aud, claim.Type);
            Assert.AreEqual("https://www.token.com", claim.Value);

            claim = dataSourceToken.Claims.Last();
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.exp, claim.Type);

            mock.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveNewDataSource_ThrowsException_False()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.Setup(x => x.Clear());

            Mock<IEncryptionService> encryptionService = mock.Create<IEncryptionService>();
            encryptionService.Setup(x => x.GenerateNewIV()).Throws<Exception>();

            ConfigService configService = new ConfigService(datasetContext.Object, null, null, null, encryptionService.Object, null, null, null, null, null, null, null, null);

            bool result = configService.CreateAndSaveNewDataSource(null);

            Assert.IsFalse(result);

            mock.VerifyAll();
        }
    }
}
