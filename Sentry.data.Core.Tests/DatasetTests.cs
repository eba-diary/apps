using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetTests
    {

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_Category()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetCategories = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetCategoryRequired));
        }

        [TestCategory("DatasetTests")]
        [TestMethod()]
        public void Can_prevent_dataset_with_no_description()
        {
            // //// Arrange ////
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetDesc = "";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDescriptionRequired));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_DatasetName()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(GlobalConstants.ValidationErrors.NAME_IS_BLANK));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_CreationUser()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.CreationUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetCreatedByRequired));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_UploadUser()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.UploadUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetUploadedByRequired));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_old_Dataset_Date()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.DatasetDtm = new DateTime(1799, 1, 1);
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDateRequired));
        }


        public Dataset GetMockDatasetData()
        {
            Dataset ds = new Dataset()
            {
                DatasetId = 0,
                DatasetCategories = new List<Category>() { new Category() { Name = "Claim" } },
                DatasetName = "Claim Dataset",
                DatasetDesc = "Test Claim Datasaet",
                DatasetInformation = "Specific Information regarding datasetfile consumption",
                CreationUserName = "Creater_User",
                UploadUserName = "Upload_User",
                OriginationCode = "Internal",
                DatasetDtm = System.DateTime.Now.AddYears(-13),
                ChangedDtm = System.DateTime.Now.AddYears(-12),
                S3Key = null,
                DatasetFiles = null,
                DatasetFileConfigs = null
            };

            return ds;
        }

    }
}
