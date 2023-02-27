using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.DTO.Retriever;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;
using static System.Net.WebRequestMethods;

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
            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(dfc.ParentDataset, user.Object));

            var configService = new ConfigService(context.Object,null,null,null,null,null,null,null,null,null,null,null);

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

            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

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

            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

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

            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Deleted;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

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

            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Deleted;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

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

            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
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

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, dataFlowService.Object, null, null);

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

            DatasetFileConfig dfc = MockClasses.MockDatasetFileConfig(null, schema);
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

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, dataFlowService.Object, null, null);

            // Act
            configService.Delete(dfc.ConfigId, null, false);

            // Assert
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), null, false), Times.Once);
        }

        [TestMethod]
        public void Create()
        {
            //Arange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            var mockSchema = MockClasses.MockFileSchema();
            var mockDataset = MockClasses.MockDataset();
            var DatasetFileConfig = MockClasses.MockDatasetFileConfig(mockDataset,mockSchema);
            DatasetFileConfigDto dto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { DatasetFileConfig }).First();

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.Add(It.IsAny<DatasetFileConfig>()));
            context.Setup(s => s.GetByIdAsync<Dataset>(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockDataset);

            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

            //Act
            _ = configService.Create(dto);

            //Assert
            mr.VerifyAll();
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
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.aud && c.Value == "https://www.token.com")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.exp)));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.scope && c.Value == "token.scope")));

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IEncryptionService> encryptionService = mock.Create<IEncryptionService>();
            encryptionService.Setup(x => x.GenerateNewIV()).Returns("IVKey");
            encryptionService.Setup(x => x.EncryptString("DecryptedClientPrivateId", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedClientPrivateId", ""));
            encryptionService.Setup(x => x.IsEncrypted("DecryptedCurrentToken")).Returns(false);
            encryptionService.Setup(x => x.EncryptString("DecryptedCurrentToken", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedCurrentToken", ""));
            encryptionService.Setup(x => x.IsEncrypted("DecryptedRefreshToken")).Returns(false);
            encryptionService.Setup(x => x.EncryptString("DecryptedRefreshToken", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedRefreshToken", ""));

            Mock<IUserService> userService = mock.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("000000");

            Mock<IEventService> eventService = mock.Create<IEventService>();
            eventService.Setup(x => x.PublishSuccessEvent(GlobalConstants.EventType.CREATED_DATASOURCE, "DataFlowName was created.", null)).Returns<Task>(null);

            ConfigService configService = new ConfigService(datasetContext.Object, userService.Object, eventService.Object, null, encryptionService.Object, null, null, null, null, null, null, null);

            DataSourceDto dataSourceDto = new DataSourceDto
            {
                Name = "DataFlowName",
                AuthID = "2",
                SourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SupportsPaging = true,
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
            Assert.IsTrue(http.SupportsPaging);
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
            Assert.AreEqual(OAuthClaims.aud, claim.Type);
            Assert.AreEqual("https://www.token.com", claim.Value);

            claim = dataSourceToken.Claims[2];
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.exp, claim.Type);

            claim = dataSourceToken.Claims.Last();
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.scope, claim.Type);
            Assert.AreEqual("token.scope", claim.Value);

            mock.VerifyAll();
        }

        [TestMethod]
        public void CreateAndSaveNewDataSource_ThrowsException_False()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.Setup(x => x.GetById<AuthenticationType>(2)).Throws<Exception>();
            datasetContext.Setup(x => x.Clear());

            ConfigService configService = new ConfigService(datasetContext.Object, null, null, null, null, null, null, null, null, null, null, null);
            
            DataSourceDto dataSourceDto = new DataSourceDto { AuthID = "2" };

            bool result = configService.CreateAndSaveNewDataSource(dataSourceDto);

            Assert.IsFalse(result);

            mock.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveDataSource_DataSourceDto_True()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();

            DataSource dataSource = new HTTPSSource()
            {
                Name = "DataSourceName",
                Description = "Data Source Description",
                SourceAuthType = new AnonymousAuthentication(),
                IsUserPassRequired = true,
                PortNumber = 442,
                BaseUri = new Uri("https://www.google.com"),
                ClientId = "ClientId",
                AuthenticationHeaderName = "AuthHeaderName",
                RequestHeaders = new List<RequestHeader>(),
                SupportsPaging = false,
                ClientPrivateId = "EncryptedClientPrivateId",
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                Tokens = new List<DataSourceToken>
                {
                    new DataSourceToken
                    {
                        Id = 3,
                        TokenName = "TokenName",
                        RefreshToken = "EncryptedRefreshToken",
                        CurrentToken = "EncryptedCurrentToken",
                        CurrentTokenExp = new DateTime(2022, 11, 21, 8, 0 ,0),
                        TokenExp = 100,
                        TokenUrl = "https://www.token.com",
                        Scope = "token.scope",
                        Claims = new List<OAuthClaim>
                        {
                            new OAuthClaim() { Type = OAuthClaims.iss, Value = "ClientId" },
                            new OAuthClaim() { Type = OAuthClaims.aud, Value = "https://www.token.com" },
                            new OAuthClaim() { Type = OAuthClaims.exp, Value = "100" },
                            new OAuthClaim() { Type = OAuthClaims.scope, Value = "token.scope" }
                        }
                    }
                },
                PrimaryContactId = "000000",
                IsSecured = true,
                Security = new Security()
            };

            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(dataSource);
            datasetContext.Setup(x => x.SaveChanges(true));

            AuthenticationType authType = new OAuthAuthentication();
            datasetContext.Setup(x => x.GetById<AuthenticationType>(2)).Returns(authType);

            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.iss && c.Value == "ClientIdUpdate")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.scope && c.Value == "token.scope2")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.aud && c.Value == "https://www.token2.com")));
            datasetContext.Setup(x => x.Add(It.Is<OAuthClaim>(c => c.Type == OAuthClaims.exp)));

            Mock<IEncryptionService> encryptionService = mock.Create<IEncryptionService>();
            encryptionService.Setup(x => x.IsEncrypted("DecryptedClientPrivateId")).Returns(false);
            encryptionService.Setup(x => x.EncryptString("DecryptedClientPrivateId", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedClientPrivateIdUpdate", ""));
            encryptionService.Setup(x => x.IsEncrypted("DecryptedCurrentToken")).Returns(false);
            encryptionService.Setup(x => x.EncryptString("DecryptedCurrentToken", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedCurrentTokenUpdate", ""));
            encryptionService.Setup(x => x.IsEncrypted("<--!-->EncryptedRefreshToken<--!-->")).Returns(true);
            encryptionService.Setup(x => x.IsEncrypted("DecryptedCurrentToken2")).Returns(false);
            encryptionService.Setup(x => x.EncryptString("DecryptedCurrentToken2", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedCurrentToken2", ""));
            encryptionService.Setup(x => x.IsEncrypted("DecryptedRefreshToken2")).Returns(false);
            encryptionService.Setup(x => x.EncryptString("DecryptedRefreshToken2", "ENCRYPT", "IVKey")).Returns(Tuple.Create("EncryptedRefreshToken2", ""));

            Mock<IEventService> eventService = mock.Create<IEventService>();
            eventService.Setup(x => x.PublishSuccessEvent(GlobalConstants.EventType.UPDATED_DATASOURCE, "DataSourceNameUpdate was updated.", null)).Returns<Task>(null);

            Mock<IUserService> userService = mock.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("000001");

            ConfigService configService = new ConfigService(datasetContext.Object, userService.Object, eventService.Object, null, encryptionService.Object, null, null, null, null, null, null, null);

            DataSourceDto dataSourceDto = new DataSourceDto
            {
                OriginatingId = 1,
                Name = "DataSourceNameUpdate",
                AuthID = "2",
                SourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SupportsPaging = true,
                RequestHeaders = new List<RequestHeader>
                {
                    new RequestHeader { Index = "0", Key = "HeaderKey", Value = "HeaderValue" }
                },
                ClientId = "ClientIdUpdate",
                ClientPrivateId = "DecryptedClientPrivateId",
                Tokens = new List<DataSourceTokenDto>
                {
                    new DataSourceTokenDto
                    {
                        Id = 3,
                        CurrentToken = "DecryptedCurrentToken",
                        RefreshToken = "<--!-->EncryptedRefreshToken<--!-->",
                        CurrentTokenExp = new DateTime(2022, 11, 21, 9, 0, 0),
                        TokenExp = 200,
                        TokenName = "TokenNameUpdate",
                        TokenUrl = "https://www.tokenupdate.com",
                        Scope = "token.scope.update"
                    },
                    new DataSourceTokenDto
                    {
                        CurrentToken = "DecryptedCurrentToken2",
                        RefreshToken = "DecryptedRefreshToken2",
                        CurrentTokenExp = new DateTime(2022, 11, 22, 12, 0, 0),
                        TokenExp = 300,
                        TokenName = "TokenName2",
                        TokenUrl = "https://www.token2.com",
                        Scope = "token.scope2"
                    }
                },
                GrantType = OAuthGrantType.JwtBearer,
                Description = "Data Source Description Update",
                IsUserPassRequired = false,
                BaseUri = new Uri("https://www.googleupdate.com"),
                PortNumber = 443,
                IsSecured = false,
                PrimaryContactId = "000001"
            };

            bool result = configService.UpdateAndSaveDataSource(dataSourceDto);

            Assert.IsTrue(result);

            Assert.AreEqual("DataSourceNameUpdate", dataSource.Name);
            Assert.AreEqual("Data Source Description Update", dataSource.Description);
            Assert.AreEqual(authType, dataSource.SourceAuthType);
            Assert.IsFalse(dataSource.IsUserPassRequired);
            Assert.AreEqual("https://www.googleupdate.com/", dataSource.BaseUri.ToString());
            Assert.AreEqual(443, dataSource.PortNumber);
            Assert.IsFalse(dataSource.IsSecured);
            Assert.AreEqual("000001", dataSource.PrimaryContactId);
            Assert.IsNotNull(dataSource.Security);
            Assert.AreEqual("000001", dataSource.Security.UpdatedById);
            Assert.IsNotNull(dataSource.Security.RemovedDate);

            Assert.IsInstanceOfType(dataSource, typeof(HTTPSSource));

            HTTPSSource http = (HTTPSSource)dataSource;
            Assert.AreEqual("IVKey", http.IVKey);
            Assert.IsTrue(http.SupportsPaging);
            Assert.AreEqual(1, http.RequestHeaders.Count);

            RequestHeader header = http.RequestHeaders.First();
            Assert.AreEqual("0", header.Index);
            Assert.AreEqual("HeaderKey", header.Key);
            Assert.AreEqual("HeaderValue", header.Value);

            Assert.AreEqual("ClientIdUpdate", http.ClientId);
            Assert.AreEqual("EncryptedClientPrivateIdUpdate", http.ClientPrivateId);
            Assert.AreEqual(OAuthGrantType.JwtBearer, http.GrantType);
            Assert.AreEqual(2, http.Tokens.Count);

            DataSourceToken dataSourceToken = http.Tokens.First();
            Assert.AreEqual("EncryptedCurrentTokenUpdate", dataSourceToken.CurrentToken);
            Assert.AreEqual("EncryptedRefreshToken", dataSourceToken.RefreshToken);
            Assert.AreEqual(new DateTime(2022, 11, 21, 9, 0, 0), dataSourceToken.CurrentTokenExp);
            Assert.AreEqual(200, dataSourceToken.TokenExp);
            Assert.AreEqual("TokenNameUpdate", dataSourceToken.TokenName);
            Assert.AreEqual("https://www.tokenupdate.com", dataSourceToken.TokenUrl);
            Assert.AreEqual("token.scope.update", dataSourceToken.Scope);
            Assert.AreEqual(http, dataSourceToken.ParentDataSource);
            Assert.AreEqual(4, dataSourceToken.Claims.Count);

            OAuthClaim claim = dataSourceToken.Claims.First();
            Assert.AreEqual(OAuthClaims.iss, claim.Type);
            Assert.AreEqual("ClientIdUpdate", claim.Value);

            claim = dataSourceToken.Claims[1];
            Assert.AreEqual(OAuthClaims.aud, claim.Type);
            Assert.AreEqual("https://www.tokenupdate.com", claim.Value);

            claim = dataSourceToken.Claims[2];
            Assert.AreEqual(OAuthClaims.exp, claim.Type);

            claim = dataSourceToken.Claims.Last();
            Assert.AreEqual(OAuthClaims.scope, claim.Type);
            Assert.AreEqual("token.scope.update", claim.Value);

            dataSourceToken = http.Tokens.Last();
            Assert.AreEqual("EncryptedCurrentToken2", dataSourceToken.CurrentToken);
            Assert.AreEqual("EncryptedRefreshToken2", dataSourceToken.RefreshToken);
            Assert.AreEqual(new DateTime(2022, 11, 22, 12, 0, 0), dataSourceToken.CurrentTokenExp);
            Assert.AreEqual(300, dataSourceToken.TokenExp);
            Assert.AreEqual("TokenName2", dataSourceToken.TokenName);
            Assert.AreEqual("https://www.token2.com", dataSourceToken.TokenUrl);
            Assert.AreEqual("token.scope2", dataSourceToken.Scope);
            Assert.AreEqual(http, dataSourceToken.ParentDataSource);
            Assert.AreEqual(4, dataSourceToken.Claims.Count);

            claim = dataSourceToken.Claims.First();
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.iss, claim.Type);
            Assert.AreEqual("ClientIdUpdate", claim.Value);

            claim = dataSourceToken.Claims[1];
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.aud, claim.Type);
            Assert.AreEqual("https://www.token2.com", claim.Value);

            claim = dataSourceToken.Claims[2];
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.exp, claim.Type);

            claim = dataSourceToken.Claims.Last();
            Assert.AreEqual(http, claim.DataSourceId);
            Assert.AreEqual(OAuthClaims.scope, claim.Type);
            Assert.AreEqual("token.scope2", claim.Value);

            mock.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveDataSource_ThrowsException_False()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Throws<Exception>();
            datasetContext.Setup(x => x.Clear());

            ConfigService configService = new ConfigService(datasetContext.Object, null, null, null, null, null, null, null, null, null, null, null);

            DataSourceDto dataSourceDto = new DataSourceDto { OriginatingId = 1 };

            bool result = configService.UpdateAndSaveDataSource(dataSourceDto);

            Assert.IsFalse(result);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetDataSourceDto_1_DataSourceDto()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();

            DataSource httpsSource = new HTTPSSource()
            {
                Id = 1,
                Name = "DataSourceName",
                Description = "Data Source Description",
                SourceAuthType = new OAuthAuthentication() { AuthID = 2 },
                IsUserPassRequired = false,
                PortNumber = 443,
                BaseUri = new Uri("https://www.google.com"),
                ClientId = "ClientId",
                AuthenticationHeaderName = "AuthHeaderName",
                RequestHeaders = new List<RequestHeader>
                {
                    new RequestHeader { Index = "0", Key = "HeaderKey", Value = "HeaderValue" }
                },
                SupportsPaging = true,
                ClientPrivateId = "EncryptedClientPrivateId",
                IVKey = "IVKey",
                GrantType = OAuthGrantType.JwtBearer,
                Tokens = new List<DataSourceToken>
                {
                    new DataSourceToken
                    {
                        Id = 3,
                        TokenName = "TokenName",
                        RefreshToken = "EncryptedRefreshToken",
                        CurrentToken = "EncryptedCurrentToken",
                        CurrentTokenExp = new DateTime(2022, 11, 21, 8, 0 ,0),
                        TokenExp = 100,
                        TokenUrl = "https://www.token.com",
                        Scope = "token.scope",
                        Claims = new List<OAuthClaim>
                        {
                            new OAuthClaim() { Type = OAuthClaims.iss, Value = "ClientId" },
                            new OAuthClaim() { Type = OAuthClaims.aud, Value = "https://www.token.com" },
                            new OAuthClaim() { Type = OAuthClaims.exp, Value = "100" },
                            new OAuthClaim() { Type = OAuthClaims.scope, Value = "token.scope" }
                        }
                    }
                },
                PrimaryContactId = "000001",
                IsSecured = true
            };
            datasetContext.Setup(x => x.GetById<DataSource>(1)).Returns(httpsSource);

            Mock<IApplicationUser> appUser = mock.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000001");
            appUser.SetupGet(x => x.DisplayName).Returns("User Name");
            appUser.SetupGet(x => x.EmailAddress).Returns("username@sentry.com");

            Mock<IApplicationUser> appUser2 = mock.Create<IApplicationUser>();

            Mock<IUserService> userService = mock.Create<IUserService>();
            userService.Setup(x => x.GetByAssociateId("000001")).Returns(appUser.Object);
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser2.Object);

            Mock<ISecurityService> securityService = mock.Create<ISecurityService>();

            UserSecurity userSecurity = new UserSecurity();
            securityService.Setup(x => x.GetUserSecurity(It.IsAny<DataSource>(), appUser2.Object)).Returns(userSecurity);

            Mock<IEncryptionService> encryptionService = mock.Create<IEncryptionService>();
            encryptionService.Setup(x => x.PrepEncryptedForDisplay("EncryptedClientPrivateId")).Returns<string>(x => Indicators.ENCRYPTIONINDICATOR + x + Indicators.ENCRYPTIONINDICATOR);
            encryptionService.Setup(x => x.PrepEncryptedForDisplay("EncryptedCurrentToken")).Returns<string>(x => Indicators.ENCRYPTIONINDICATOR + x + Indicators.ENCRYPTIONINDICATOR);
            encryptionService.Setup(x => x.PrepEncryptedForDisplay("EncryptedRefreshToken")).Returns<string>(x => Indicators.ENCRYPTIONINDICATOR + x + Indicators.ENCRYPTIONINDICATOR);

            ConfigService configService = new ConfigService(datasetContext.Object, userService.Object, null, null, encryptionService.Object, securityService.Object, null, null, null, null, null, null);

            DataSourceDto dataSource = configService.GetDataSourceDto(1);

            Assert.AreEqual(1, dataSource.OriginatingId);
            Assert.AreEqual("DataSourceName", dataSource.Name);
            Assert.AreEqual("Data Source Description", dataSource.Description);
            Assert.IsNull(dataSource.ReturnUrl);
            Assert.AreEqual(DataSourceDiscriminator.HTTPS_SOURCE, dataSource.SourceType);
            Assert.AreEqual("2", dataSource.AuthID);
            Assert.IsFalse(dataSource.IsUserPassRequired);
            Assert.AreEqual(443, dataSource.PortNumber);
            Assert.AreEqual("https://www.google.com/", dataSource.BaseUri.ToString());
            Assert.IsTrue(dataSource.IsSecured);
            Assert.AreEqual("000001", dataSource.PrimaryContactId);
            Assert.AreEqual("User Name", dataSource.PrimaryContactName);
            Assert.AreEqual("username@sentry.com", dataSource.PrimaryContactEmail);
            Assert.AreEqual(userSecurity, dataSource.Security);
            Assert.AreEqual($"mailto:username@sentry.com?Subject=Data%20Source%20Inquiry%20-%20DataSourceName", dataSource.MailToLink);
            Assert.AreEqual("ClientId", dataSource.ClientId);
            Assert.AreEqual($"{Indicators.ENCRYPTIONINDICATOR}EncryptedClientPrivateId{Indicators.ENCRYPTIONINDICATOR}", dataSource.ClientPrivateId);
            Assert.IsTrue(dataSource.SupportsPaging);
            Assert.AreEqual("AuthHeaderName", dataSource.TokenAuthHeader);
            Assert.AreEqual(OAuthGrantType.JwtBearer, dataSource.GrantType);
            Assert.AreEqual(1, dataSource.RequestHeaders.Count);

            RequestHeader header = dataSource.RequestHeaders.First();
            Assert.AreEqual("0", header.Index);
            Assert.AreEqual("HeaderKey", header.Key);
            Assert.AreEqual("HeaderValue", header.Value);

            Assert.AreEqual(1, dataSource.Tokens.Count);

            DataSourceTokenDto dataSourceToken = dataSource.Tokens.First();
            Assert.AreEqual(3, dataSourceToken.Id);
            Assert.AreEqual($"{Indicators.ENCRYPTIONINDICATOR}EncryptedCurrentToken{Indicators.ENCRYPTIONINDICATOR}", dataSourceToken.CurrentToken);
            Assert.AreEqual($"{Indicators.ENCRYPTIONINDICATOR}EncryptedRefreshToken{Indicators.ENCRYPTIONINDICATOR}", dataSourceToken.RefreshToken);
            Assert.AreEqual(new DateTime(2022, 11, 21, 8, 0, 0), dataSourceToken.CurrentTokenExp);
            Assert.AreEqual(100, dataSourceToken.TokenExp);
            Assert.AreEqual("TokenName", dataSourceToken.TokenName);
            Assert.AreEqual("https://www.token.com", dataSourceToken.TokenUrl);
            Assert.AreEqual("token.scope", dataSourceToken.Scope);

            mock.VerifyAll();
        }
    }
}
