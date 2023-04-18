using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ReindexServiceTests
    {
        [TestMethod]
        public async Task ReindexAsync_GlobalDatasets()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IReindexProvider> reindexProvider = mr.Create<IReindexProvider>();
            reindexProvider.Setup(x => x.GetCurrentIndexVersionAsync<GlobalDataset>()).ReturnsAsync("index-v1");
            reindexProvider.Setup(x => x.CreateNewIndexVersionAsync("index-v1")).ReturnsAsync("index-v2");

            Mock<IReindexSource<GlobalDataset>> reindexSource = mr.Create<IReindexSource<GlobalDataset>>();
            List<GlobalDataset> globalDatasets = new List<GlobalDataset>();
            reindexSource.SetupSequence(x => x.TryGetNextDocuments(out globalDatasets)).Returns(true).Returns(true).Returns(false);

            reindexProvider.Setup(x => x.IndexDocumentsAsync(globalDatasets, "index-v2")).Returns(Task.CompletedTask);
            reindexProvider.Setup(x => x.ChangeToNewIndexAsync<GlobalDataset>("index-v1", "index-v2")).Returns(Task.CompletedTask);

            ReindexService<GlobalDataset> reindexService = new ReindexService<GlobalDataset>(reindexSource.Object, reindexProvider.Object);

            await reindexService.ReindexAsync();

            reindexSource.Verify(x => x.TryGetNextDocuments(out globalDatasets), Times.Exactly(3));
            reindexProvider.Verify(x => x.IndexDocumentsAsync(globalDatasets, "index-v2"), Times.Exactly(2));

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task ReindexAsync_GlobalDatasets_CurrentIndexNotExist()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IReindexProvider> reindexProvider = mr.Create<IReindexProvider>();
            string index = null;
            reindexProvider.Setup(x => x.GetCurrentIndexVersionAsync<GlobalDataset>()).ReturnsAsync(index);

            ReindexService<GlobalDataset> reindexService = new ReindexService<GlobalDataset>(null, reindexProvider.Object);

            await reindexService.ReindexAsync();

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task ReindexAsync_GlobalDatasets_Exception()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IReindexProvider> reindexProvider = mr.Create<IReindexProvider>();
            reindexProvider.Setup(x => x.GetCurrentIndexVersionAsync<GlobalDataset>()).ReturnsAsync("index-v1");
            reindexProvider.Setup(x => x.CreateNewIndexVersionAsync("index-v1")).ThrowsAsync(new Exception());

            ReindexService<GlobalDataset> reindexService = new ReindexService<GlobalDataset>(null, reindexProvider.Object);

            await Assert.ThrowsExceptionAsync<Exception>(() => reindexService.ReindexAsync());

            mr.VerifyAll();
        }
    }
}
