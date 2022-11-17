using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ProdRetrieverJobProviderTests : BaseRetrieverJobProviderTests
    {
        [TestMethod]
        public void AcceptedNamedEnvironments_PROD_PRODNP()
        {
            ProdRetrieverJobProvider provider = new ProdRetrieverJobProvider(null);

            List<string> results = provider.AcceptedNamedEnvironments;

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(DLPPEnvironments.PROD, results.First());
            Assert.AreEqual(DLPPEnvironments.PRODNP, results.Last());
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_PROD_RetrieverJobs()
        {
            Mock<IDatasetContext> datasetContext = GetDatasetContextForDfsRetrieverJobs();

            ProdRetrieverJobProvider provider = new ProdRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(DLPPEnvironments.PROD);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(7, results.First().Id);
            Assert.AreEqual(4, results.Last().Id);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_PRODNP_RetrieverJobs()
        {
            Mock<IDatasetContext> datasetContext = GetDatasetContextForDfsRetrieverJobs();

            ProdRetrieverJobProvider provider = new ProdRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(DLPPEnvironments.PRODNP);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(5, results.First().Id);
            Assert.AreEqual(1, results.Last().Id);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_Null_RetrieverJobs()
        {
            Mock<IDatasetContext> datasetContext = GetDatasetContextForDfsRetrieverJobs();

            ProdRetrieverJobProvider provider = new ProdRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(null);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(7, results.First().Id);
            Assert.AreEqual(4, results.Last().Id);

            datasetContext.VerifyAll();
        }
    }
}
