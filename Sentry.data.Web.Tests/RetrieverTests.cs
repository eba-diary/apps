using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Sentry.data.Web.Tests;
using Sentry.Configuration;
using System.IO;
using Sentry.data.Core;

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
            var vr = rtjob.GetUri().ToString();

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
            var vr = rtjob.GetUri().ToString();
            Assert.IsTrue(vr == new Uri(Path.Combine("file:///", Config.GetHostSetting("FileShare"), "Custom/Directory/")).ToString());
        }

        [TestMethod]
        public void FtpSource_RetrieverJob_GetUri_Return_BaseUri_With_RealtiveUri()
        {
            RetrieverJob rtjob = MockClasses.GetMockRetrieverJob(null, new FtpSource(), new AnonymousAuthentication());
            var vr = rtjob.GetUri().ToString();
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
