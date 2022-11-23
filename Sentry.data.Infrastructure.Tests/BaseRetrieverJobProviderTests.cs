using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class BaseRetrieverJobProviderTests
    {
        protected Mock<IDatasetContext> GetDatasetContextForDfsRetrieverJobs()
        {
            List<RetrieverJob> jobs = new List<RetrieverJob>()
            {
                new RetrieverJob()
                {
                    Id = 1,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsDataFlowBasic() { Id = 1 },
                    DataFlow = new DataFlow() { Id = 10 }
                },
                new RetrieverJob()
                {
                    Id = 2,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsDataFlowBasic() { Id = 1 },
                    DataFlow = new DataFlow() { Id = 10 }
                },
                new RetrieverJob()
                {
                    Id = 3,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsDataFlowBasic() { Id = 2 },
                    DataFlow = new DataFlow() { Id = 10 }
                },
                new RetrieverJob()
                {
                    Id = 4,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsDataFlowBasic() { Id = 1 },
                    DataFlow = new DataFlow() { Id = 11 }
                },
                new RetrieverJob()
                {
                    Id = 9,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsDataFlowBasic() { Id = 1 },
                    DataFlow = new DataFlow() { Id = 11 }
                },
                new RetrieverJob()
                {
                    Id = 5,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsNonProdSource() { Id = 3 },
                    DataFlow = new DataFlow() { Id = 10 }
                },
                new RetrieverJob()
                {
                    Id = 6,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsNonProdSource() { Id = 3 },
                    DataFlow = new DataFlow() { Id = 10 }
                },
                new RetrieverJob()
                {
                    Id = 7,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsProdSource() { Id = 4 },
                    DataFlow = new DataFlow() { Id = 11 }
                },
                new RetrieverJob()
                {
                    Id = 8,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DataSource = new DfsProdSource() { Id = 4 },
                    DataFlow = new DataFlow() { Id = 11 }
                }
            };

            List<DataFlow> dataFlows = new List<DataFlow>()
            {
                new DataFlow()
                {
                    Id = 10,
                    DatasetId = 20
                },
                new DataFlow()
                {
                    Id = 11,
                    DatasetId = 21
                }
            };

            List<DataSource> dataSources = new List<DataSource>()
            {
                new DfsDataFlowBasic() { Id = 1 },
                new HTTPSSource() { Id = 2 },
                new DfsNonProdSource() { Id = 3 },
                new DfsProdSource() { Id = 4 }
            };

            List<Dataset> datasets = new List<Dataset>()
            {
                new Dataset()
                {
                    DatasetId = 20,
                    NamedEnvironmentType = NamedEnvironmentType.NonProd
                },
                new Dataset()
                {
                    DatasetId = 21,
                    NamedEnvironmentType = NamedEnvironmentType.Prod
                }
            };

            Mock<IDatasetContext> context = new Mock<IDatasetContext>(MockBehavior.Strict);
            context.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            context.SetupGet(x => x.DataFlow).Returns(dataFlows.AsQueryable());
            context.SetupGet(x => x.DataSources).Returns(dataSources.AsQueryable());
            context.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            return context;
        }
    }
}
