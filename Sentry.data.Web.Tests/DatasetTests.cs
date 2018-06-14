using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sentry.data.Core;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetTests
    {
        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_s3Key()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.S3Key = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.s3keyIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_Category()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.Category = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.categoryIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod()]
        public void Can_prevent_dataset_with_no_description()
        {
            // //// Arrange ////
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetDesc = "";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDescIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_DatasetName()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.nameIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_CreationUser()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.CreationUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.creationUserNameIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_UploadUser()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.UploadUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.uploadUserNameIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_old_Dataset_Date()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetDtm = new DateTime(1799, 1, 1);
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDateIsOld));
        }

        public DatasetScopeType GetMockDatasetScopeData()
        {
            DatasetScopeType dst = new DatasetScopeType(
                "Point-in-Time",
                "Transactional data",
                true);

            return dst;
        }

        public Dataset GetMockDatasetData()
        {
            Dataset ds = new Dataset()
            {
                DatasetId = 0,
                Category = "Claim",
                DatasetCategory = new Category("Claim"),
                DatasetName = "Claim Dataset",
                DatasetDesc = "Test Claim Datasaet",
                DatasetInformation = "Specific Information regarding datasetfile consumption",
                CreationUserName = "Creater_User",
                SentryOwnerName = "072984",
                UploadUserName = "Upload_User",
                OriginationCode = "Internal",
                DatasetDtm = System.DateTime.Now.AddYears(-13),
                ChangedDtm = System.DateTime.Now.AddYears(-12),
                S3Key = null,
                IsSensitive = true,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null
            };

            return ds;
        }

    }
}
