using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class QualRetrieverJobProviderTests : BaseRetrieverJobProviderTests
    {
        [TestMethod]
        public void AcceptedNamedEnvironments_QUAL_QUALNP()
        {
            QualRetrieverJobProvider provider = new QualRetrieverJobProvider(null);

            List<string> results = provider.AcceptedNamedEnvironments;

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(DLPPEnvironments.QUAL, results.First());
            Assert.AreEqual(DLPPEnvironments.QUALNP, results.Last());
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_QUAL_RetrieverJobs()
        {
            Mock<IDatasetContext> datasetContext = GetDatasetContextForDfsRetrieverJobs();

            QualRetrieverJobProvider provider = new QualRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(DLPPEnvironments.QUAL);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(7, results.First().Id);
            Assert.AreEqual(4, results.Last().Id);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_QUALNP_RetrieverJobs()
        {
            Mock<IDatasetContext> datasetContext = GetDatasetContextForDfsRetrieverJobs();

            QualRetrieverJobProvider provider = new QualRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(DLPPEnvironments.QUALNP);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(5, results.First().Id);
            Assert.AreEqual(1, results.Last().Id);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_Null_RetrieverJobs()
        {
            Mock<IDatasetContext> datasetContext = GetDatasetContextForDfsRetrieverJobs();

            QualRetrieverJobProvider provider = new QualRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(null);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(5, results.First().Id);
            Assert.AreEqual(1, results.Last().Id);

            datasetContext.VerifyAll();
        }
    }
}
