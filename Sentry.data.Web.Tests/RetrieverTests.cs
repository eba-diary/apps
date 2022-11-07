using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Configuration;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class RetrieverTests
    {
        [TestMethod]
        public void DfsBasic_SourceType_Set_Correctly()
        {
            /// Arrange ///
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, new DfsBasic(), new AnonymousAuthentication());
            var vr = rtjob.DataSource.SourceType;
            Assert.IsTrue(vr == "DFSBasic");
        }

        [TestMethod]
        public void DfsBasic_RetrieverJob_GetUri_Returns_Only_BaseUri()
        {
            /// Arrange ///
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, new DfsBasic(), new AnonymousAuthentication());

            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            
            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();
            context.SetupGet(x => x.Datasets).Returns(new List<Dataset>().AsQueryable());

            Mock<IDataFeatures> features = mockRepository.Create<IDataFeatures>();
            features.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            IJobService jobService = new JobService(context.Object, null, null, features.Object, null);

            var vr = jobService.GetDataSourceUri(rtjob).ToString();

            mockRepository.VerifyAll();

            Assert.IsTrue(vr == new Uri(Path.Combine("file:///", Config.GetHostSetting("FileShare"), "DatasetLoader/")).ToString()
                + rtjob.DatasetConfig.ParentDataset.DatasetCategories.First().Name.ToLower() + "/" 
                + rtjob.DatasetConfig.ParentDataset.DatasetName.ToLower().Replace(' ', '_') + "/default");
        }

        [TestMethod]
        public void DfsCustom_SourceType_Set_Correctly()
        {
            /// Arrange ///
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, new DfsCustom(), new AnonymousAuthentication());
            var vr = rtjob.DataSource.SourceType;
            Assert.IsTrue(vr == "DFSCustom");
        }

        [TestMethod]
        public void DfsCustom_RetrieverJob_GetUri_Returns_BaseUri_With_RelativeUri()
        {
            /// Arrange ///
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, new DfsCustom(), new AnonymousAuthentication());
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();
            context.SetupGet(x => x.Datasets).Returns(new List<Dataset>().AsQueryable());

            Mock<IDataFeatures> features = mockRepository.Create<IDataFeatures>();
            features.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            IJobService jobService = new JobService(context.Object, null, null, features.Object, null);

            var vr = jobService.GetDataSourceUri(rtjob).ToString();

            mockRepository.VerifyAll();
            Assert.IsTrue(vr == new Uri(Path.Combine("file:///", Config.GetHostSetting("FileShare"), "Custom/Directory/")).ToString());
        }

        [TestMethod]
        public void FtpSource_RetrieverJob_GetUri_Return_BaseUri_With_RealtiveUri()
        {
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, new FtpSource(), new AnonymousAuthentication());
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mockRepository.Create<IDatasetContext>();
            context.SetupGet(x => x.Datasets).Returns(new List<Dataset>().AsQueryable());

            Mock<IDataFeatures> features = mockRepository.Create<IDataFeatures>();
            features.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            IJobService jobService = new JobService(context.Object, null, null, features.Object, null);

            var vr = jobService.GetDataSourceUri(rtjob).ToString();

            mockRepository.VerifyAll();
            Assert.IsTrue(vr == @"ftp://ftp.sentry.com/SourceFolder/CurrentDirectory/");
        }

        [TestMethod]
        public void RetrieverJob_Enabled_By_Default()
        {
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, null, null);
            var vr = rtjob.IsEnabled;
            Assert.IsTrue(vr == true);
        }
    }
}
