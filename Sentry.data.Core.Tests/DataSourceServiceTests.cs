using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataSourceServiceTests
    {
        [TestMethod]
        public void GetDataSourceTypeDtosForDropdown_DataSourceTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();

            List<DataSourceType> dataSourceTypes = new List<DataSourceType>
            {
                new DataSourceType { Name = "FTP", Description = "FTP Description", DiscrimatorValue = DataSourceDiscriminator.FTP_SOURCE },
                new DataSourceType { Name = "SFTP", Description = "SFTP Description", DiscrimatorValue = DataSourceDiscriminator.SFTP_SOURCE },
                new DataSourceType { Name = "DFS Custom", Description = "DFS Custom Description", DiscrimatorValue = DataSourceDiscriminator.DFS_CUSTOM },
                new DataSourceType { Name = "HTTPS", Description = "HTTPS Description", DiscrimatorValue = DataSourceDiscriminator.HTTPS_SOURCE },
                new DataSourceType { Name = "Google API", Description = "Google API Description", DiscrimatorValue = DataSourceDiscriminator.GOOGLE_API_SOURCE },
                new DataSourceType { Name = "Google BigQuery API", Description = "Google BigQuery API Description", DiscrimatorValue = DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE },
                new DataSourceType { Name = "DFS", Description = "DFS Description", DiscrimatorValue = DataSourceDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION },
                new DataSourceType { Name = "S3", Description = "S3 Description", DiscrimatorValue = DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION }
            };

            datasetContext.SetupGet(x => x.DataSourceTypes).Returns(dataSourceTypes.AsQueryable());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<DataSourceTypeDto> results = dataSourceService.GetDataSourceTypeDtosForDropdown();

            Assert.AreEqual(6, results.Count);

            DataSourceTypeDto dto = results.First();
            Assert.AreEqual("FTP", dto.Name);
            Assert.AreEqual("FTP Description", dto.Description);
            Assert.AreEqual(DataSourceDiscriminator.FTP_SOURCE, dto.DiscrimatorValue);

            dto = results[1];
            Assert.AreEqual("SFTP", dto.Name);
            Assert.AreEqual("SFTP Description", dto.Description);
            Assert.AreEqual(DataSourceDiscriminator.SFTP_SOURCE, dto.DiscrimatorValue);

            dto = results[2];
            Assert.AreEqual("DFS Custom", dto.Name);
            Assert.AreEqual("DFS Custom Description", dto.Description);
            Assert.AreEqual(DataSourceDiscriminator.DFS_CUSTOM, dto.DiscrimatorValue);

            dto = results[3];
            Assert.AreEqual("HTTPS", dto.Name);
            Assert.AreEqual("HTTPS Description", dto.Description);
            Assert.AreEqual(DataSourceDiscriminator.HTTPS_SOURCE, dto.DiscrimatorValue);

            dto = results[4];
            Assert.AreEqual("Google API", dto.Name);
            Assert.AreEqual("Google API Description", dto.Description);
            Assert.AreEqual(DataSourceDiscriminator.GOOGLE_API_SOURCE, dto.DiscrimatorValue);

            dto = results.Last();
            Assert.AreEqual("Google BigQuery API", dto.Name);
            Assert.AreEqual("Google BigQuery API Description", dto.Description);
            Assert.AreEqual(DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE, dto.DiscrimatorValue);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetValidAuthenticationTypeDtosByType_FTP_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetValidAuthenticationTypeDtosByType(DataSourceDiscriminator.FTP_SOURCE);

            Assert.AreEqual(2, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(1, dto.AuthID);
            Assert.AreEqual("Anon", dto.AuthName);

            dto = results.Last();
            Assert.AreEqual(2, dto.AuthID);
            Assert.AreEqual("Basic", dto.AuthName);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetValidAuthenticationTypeDtosByType_SFTP_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetValidAuthenticationTypeDtosByType(DataSourceDiscriminator.SFTP_SOURCE);

            Assert.AreEqual(1, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(2, dto.AuthID);
            Assert.AreEqual("Basic", dto.AuthName);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetValidAuthenticationTypeDtosByType_DFSCustom_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetValidAuthenticationTypeDtosByType(DataSourceDiscriminator.DFS_CUSTOM);

            Assert.AreEqual(1, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(2, dto.AuthID);
            Assert.AreEqual("Basic", dto.AuthName);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetValidAuthenticationTypeDtosByType_HTTPS_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetValidAuthenticationTypeDtosByType(DataSourceDiscriminator.HTTPS_SOURCE);

            Assert.AreEqual(3, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(1, dto.AuthID);
            Assert.AreEqual("Anon", dto.AuthName);

            dto = results[1];
            Assert.AreEqual(3, dto.AuthID);
            Assert.AreEqual("OAuth", dto.AuthName);

            dto = results.Last();
            Assert.AreEqual(4, dto.AuthID);
            Assert.AreEqual("Token", dto.AuthName);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetValidAuthenticationTypeDtosByType_GoogleApi_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetValidAuthenticationTypeDtosByType(DataSourceDiscriminator.GOOGLE_API_SOURCE);

            Assert.AreEqual(1, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(3, dto.AuthID);
            Assert.AreEqual("OAuth", dto.AuthName);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetValidAuthenticationTypeDtosByType_GoogleBigQueryApi_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetValidAuthenticationTypeDtosByType(DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE);

            Assert.AreEqual(1, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(3, dto.AuthID);
            Assert.AreEqual("OAuth", dto.AuthName);

            mock.VerifyAll();
        }

        [TestMethod]
        public void GetAuthenticationTypeDtos_AuthenticationTypeDtos()
        {
            MockRepository mock = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mock.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.AuthTypes).Returns(GetAuthenticationTypes());

            DataSourceService dataSourceService = new DataSourceService(datasetContext.Object, null, null);

            List<AuthenticationTypeDto> results = dataSourceService.GetAuthenticationTypeDtos();

            Assert.AreEqual(4, results.Count);

            AuthenticationTypeDto dto = results.First();
            Assert.AreEqual(1, dto.AuthID);
            Assert.AreEqual("Anon", dto.AuthName);

            dto = results[1];
            Assert.AreEqual(2, dto.AuthID);
            Assert.AreEqual("Basic", dto.AuthName);

            dto = results[2];
            Assert.AreEqual(3, dto.AuthID);
            Assert.AreEqual("OAuth", dto.AuthName);

            dto = results.Last();
            Assert.AreEqual(4, dto.AuthID);
            Assert.AreEqual("Token", dto.AuthName);

            mock.VerifyAll();
        }

        #region Helpers
        private IQueryable<AuthenticationType> GetAuthenticationTypes()
        {
            return new List<AuthenticationType>
            {
                new AnonymousAuthentication { AuthID = 1, AuthName = "Anon" },
                new BasicAuthentication { AuthID = 2, AuthName = "Basic" },
                new OAuthAuthentication { AuthID = 3, AuthName = "OAuth" },
                new TokenAuthentication { AuthID = 4, AuthName = "Token" }
            }.AsQueryable();
        }
        #endregion
    }
}
