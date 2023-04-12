using Elasticsearch.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using NHibernate.Util;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ElasticReindexProviderTests
    {
        [TestMethod]
        public async Task GetCurrentIndexVersionAsync_IndexV1()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticIndexClient> elasticIndexClient = mr.Create<IElasticIndexClient>();

            string alias = "index";
            elasticIndexClient.Setup(x => x.TryGetAlias<GlobalDataset>(out alias)).Returns(true);
            elasticIndexClient.Setup(x => x.GetIndexNameByAliasAsync("index")).ReturnsAsync("index-v1");

            ElasticReindexProvider provider = new ElasticReindexProvider(elasticIndexClient.Object, null);

            string result = await provider.GetCurrentIndexVersionAsync<GlobalDataset>();

            Assert.AreEqual("index-v1", result);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task GetCurrentIndexVersionAsync_Null()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticIndexClient> elasticIndexClient = mr.Create<IElasticIndexClient>();

            string alias;
            elasticIndexClient.Setup(x => x.TryGetAlias<GlobalDataset>(out alias)).Returns(false);

            ElasticReindexProvider provider = new ElasticReindexProvider(elasticIndexClient.Object, null);

            string result = await provider.GetCurrentIndexVersionAsync<GlobalDataset>();

            Assert.IsNull(result);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CreateNewIndexVersionAsync_IndexV1_IndexV2()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticIndexClient> elasticIndexClient = mr.Create<IElasticIndexClient>();
            elasticIndexClient.Setup(x => x.CreateIndexAsync("index-v2")).Returns(Task.CompletedTask);

            ElasticReindexProvider provider = new ElasticReindexProvider(elasticIndexClient.Object, null);

            string result = await provider.CreateNewIndexVersionAsync("index-v1");

            Assert.AreEqual("index-v2", result);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CreateNewIndexVersionAsync_Index_IndexV1()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticIndexClient> elasticIndexClient = mr.Create<IElasticIndexClient>();
            elasticIndexClient.Setup(x => x.CreateIndexAsync("index-v1")).Returns(Task.CompletedTask);

            ElasticReindexProvider provider = new ElasticReindexProvider(elasticIndexClient.Object, null);

            string result = await provider.CreateNewIndexVersionAsync("index");

            Assert.AreEqual("index-v1", result);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task IndexDocumentsAsync_GlobalDatasets_Index()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<GlobalDataset> globalDatasets = new List<GlobalDataset> { new GlobalDataset() };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.IndexManyAsync(globalDatasets, "index")).Returns(Task.CompletedTask);

            ElasticReindexProvider provider = new ElasticReindexProvider(null, elasticDocumentClient.Object);

            await provider.IndexDocumentsAsync(globalDatasets, "index");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task ChangeToNewIndexAsync_IndexV1_IndexV2()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticIndexClient> elasticIndexClient = mr.Create<IElasticIndexClient>();
            string alias = "index";
            elasticIndexClient.Setup(x => x.TryGetAlias<GlobalDataset>(out alias)).Returns(true);
            elasticIndexClient.Setup(x => x.AddAliasAsync("index-v2", "index")).Returns(Task.CompletedTask);
            elasticIndexClient.Setup(x => x.DeleteIndexAsync("index-v1")).Returns(Task.CompletedTask);

            ElasticReindexProvider provider = new ElasticReindexProvider(elasticIndexClient.Object, null);

            await provider.ChangeToNewIndexAsync<GlobalDataset>("index-v1", "index-v2");

            mr.VerifyAll();
        }
    }
}
