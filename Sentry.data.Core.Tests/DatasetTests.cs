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

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_ShortName()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.ShortName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetShortNameRequired));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_invalid_chars_in_ShortName()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.ShortName = "Cool dataset";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetShortNameInvalid));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_long_ShortName()
        {
            Dataset dataset1 = GetMockDatasetData();
            dataset1.ShortName = "ThisShortDatasetNameIsTooLong";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetShortNameInvalid));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Report_Type_Returns_IsHrData_False()
        {
            //Arrange
            var vr = GetMockDatasetData();
            vr.DatasetType = GlobalConstants.DataEntityCodes.REPORT;

            //Assert
            Assert.IsFalse(vr.AdminDataPermissionsAreExplicit);
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Dataset_Type_Returns_IsHrData_False_When_Not_HR_Category()
        {
            //Arrange
            var vr = GetMockDatasetData();
            vr.DatasetType = GlobalConstants.DataEntityCodes.DATASET;

            //Assert
            Assert.IsFalse(vr.AdminDataPermissionsAreExplicit);
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Dataset_Type_Returns_IsHrData_True_When_HR_Category()
        {
            //Arrange
            var vr = GetMockDatasetData();
            vr.DatasetType = GlobalConstants.DataEntityCodes.DATASET;
            vr.DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources" } };

            //Assert
            Assert.IsTrue(vr.AdminDataPermissionsAreExplicit);
        }

        public Dataset GetMockDatasetData()
        {
            Dataset ds = new Dataset()
            {
                DatasetId = 0,
                DatasetCategories = new List<Category>() { new Category() { Name = "Claim" } },
                DatasetName = "Claim Dataset",
                ShortName = "ClaimDataset",
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
