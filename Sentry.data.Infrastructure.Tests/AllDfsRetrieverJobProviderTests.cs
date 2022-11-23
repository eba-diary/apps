using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class AllDfsRetrieverJobProviderTests
    {
        [TestMethod]
        public void AcceptedNamedEnvironments_TEST_NRTEST()
        {
            AllDfsRetrieverJobProvider provider = new AllDfsRetrieverJobProvider(null);

            List<string> results = provider.AcceptedNamedEnvironments;

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(DLPPEnvironments.TEST, results.First());
            Assert.AreEqual(DLPPEnvironments.NRTEST, results.Last());
        }

        [TestMethod]
        public void GetDfsRetrieverJobs_TEST_RetrieverJobs()
        {
            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsDataFlowBasic()
                },
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsDataFlowBasic()
                },
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new HTTPSSource()
                },
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsNonProdSource()
                },
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsProdSource()
                },
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsNonProdSource()
                },
                new RetrieverJob()
                {
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsProdSource()
                }
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());

            AllDfsRetrieverJobProvider provider = new AllDfsRetrieverJobProvider(datasetContext.Object);

            List<RetrieverJob> results = provider.GetDfsRetrieverJobs(DLPPEnvironments.TEST);

            Assert.AreEqual(3, results.Count);

            datasetContext.VerifyAll();
        }
    }
}
