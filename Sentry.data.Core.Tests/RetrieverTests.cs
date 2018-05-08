using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Sentry.data.Core.Tests
{
    [TestClass][Ignore]
    public class RetrieverTests
    {
        [TestMethod]
        public void DfsBasic_SourceType_Set_Correctly()
        {
            /// Arrange ///
            RetrieverJob rtjob = GetMockRetrieverJob(new DfsBasic(), new AnonymousAuthentication());
            var vr = rtjob.DataSource.SourceType;
            Assert.IsTrue(vr == "DFSBasic");
        }

        [TestMethod]
        public void DfsBasic_RetrieverJob_GetUri_Returns_Only_BaseUri()
        {
            /// Arrange ///
            RetrieverJob rtjob = GetMockRetrieverJob(new DfsBasic(), new AnonymousAuthentication());
            var vr = rtjob.GetUri().ToString();
            Assert.IsTrue(vr == @"file:///c:/tmp/DatasetLoader/claim/dataset_name/default");
        }

        [TestMethod]
        public void DfsCustom_SourceType_Set_Correctly()
        {
            /// Arrange ///
            RetrieverJob rtjob = GetMockRetrieverJob(new DfsCustom(), new AnonymousAuthentication());
            var vr = rtjob.DataSource.SourceType;
            Assert.IsTrue(vr == "DFSCustom");
        }

        [TestMethod]
        public void DfsCustom_RetrieverJob_GetUri_Returns_BaseUri_With_RelativeUri()
        {
            /// Arrange ///
            RetrieverJob rtjob = GetMockRetrieverJob(new DfsCustom(), new AnonymousAuthentication());
            var vr = rtjob.GetUri().ToString();
            Assert.IsTrue(vr == @"file:///c:/tmp/Custom/Directory/");
        }

        [TestMethod]
        public void FtpSource_RetrieverJob_GetUri_Return_BaseUri_With_RealtiveUri()
        {
            RetrieverJob rtjob = GetMockRetrieverJob(GetMockFtpSource(), new AnonymousAuthentication());
            var vr = rtjob.GetUri().ToString();
            Assert.IsTrue(vr == @"ftp://ftp.sentry.com/SourceFolder/CurrentDirectory/");
        }


        private Category GetMockCategoryData()
        {
            Category cat = new Category("Claim");
            return cat;
        }
        private Dataset GetMockDatasetData()
        {
            Dataset dataset1 = new Dataset(0,
                                            "Claim",
                                            "Dataset_Name",
                                            "Description_Test",
                                            "Datasetfile_Consumption_Information",
                                            "Dataset_Creator",
                                            "072984",
                                            "Upload_User",
                                            "Internal",
                                            System.DateTime.Now.AddYears(-13),
                                            System.DateTime.Now.AddYears(-12),
                                            null,
                                            true,
                                            true,
                                            GetMockCategoryData(),
                                            null,
                                            null,
                                            null);

            return dataset1;
        }
        private DatasetFileConfig GetMockDatasetFileConfig(Dataset ds)
        {
            DatasetFileConfig dfc = new DatasetFileConfig()                    
            {
                ConfigId = 0,
                Name = "Default",
                Description = "Test_Description",
                SearchCriteria = @"\.",
                DropPath = @"c:\tmp\default\",
                IsRegexSearch = true,
                OverwriteDatafile = true,
                FileTypeId = (int)FileType.DataFile,
                IsGeneric = true,
                ParentDataset = ds,
                DatasetScopeType = GetMockDatasetScopeType()
            };

            return dfc;
        }

        private DatasetScopeType GetMockDatasetScopeType()
        {
            return new DatasetScopeType("MockScopeType", "Mock Scope Type", true);
        }

        private RetrieverJob GetMockRetrieverJob(DataSource dsrc, AuthenticationType authType)
        {
            throw new NotImplementedException();
            RetrieverJob rtjob = new RetrieverJob();

            //rtjob.DatasetConfig = GetMockDatasetFileConfig(GetMockDatasetData());
            //switch (dsrc)
            //{
            //    case DfsBasic b:
            //        b.Name = "Basic_Name";
            //        b.Description = "Basic_Description";
            //        rtjob.DataSource = b;
            //        return rtjob;

            //    case DfsCustom c:
            //        c.Name = "Custom_Name";
            //        c.Description = "Custom_Description";
            //        rtjob.DataSource = c;
            //        rtjob.RelativeUri = @"Custom\Directory\";
            //        return rtjob;

            //    case FtpSource f:
            //        f.SourceAuthType = authType;
            //        f.BaseUri = new Uri(@"ftp://ftp.Sentry.com/");
            //        rtjob.DataSource = f;
            //        rtjob.RelativeUri = @"SourceFolder\CurrentDirectory\";
            //        return rtjob;

            //    default:
            //        throw new NotImplementedException();
            //        break;
            //}         
        }
        private FtpSource GetMockFtpSource()
        {

            var source = new FtpSource();

            return source;
        }
    }
}
