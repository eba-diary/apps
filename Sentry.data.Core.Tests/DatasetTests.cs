using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sentry.data.Core;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetTests
    {
        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_get_fileExtention_with_index_of_dot_less_than_3()
        {
            //// Arrange ////
            Dataset dataset1 = CreateDataset();
            dataset1.DatasetName = "ABC.txt";

            //// Act ////
            var vr = dataset1.FileExtension;

            //// Asert ////

            Assert.AreEqual("", vr);
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_matched_catetory_and_s3Key()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.S3Key = "Claim/s3/key";
            var vr = dataset1.ValidateForSave();
            Assert.IsFalse(vr.Contains(Dataset.ValidationErrors.s3KeyCategoryMismatch));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_mismatched_catetory_and_s3Key()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.S3Key = "s3/key";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.s3KeyCategoryMismatch));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_s3Key()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.S3Key = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.s3keyIsBlank));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_Category()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.Category = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.categoryIsBlank));
        }

        [TestCategory("DatasetTests")]
        [TestMethod()]
        public void Can_prevent_dataset_with_no_description()
        {
            // //// Arrange ////
            Dataset dataset1 = CreateDataset();
            dataset1.DatasetDesc = "";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDescIsBlank));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_DatasetName()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.DatasetName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.nameIsBlank));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_CreationUser()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.CreationUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.creationUserNameIsBlank));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_UploadUser()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.UploadUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.uploadUserNameIsBlank));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_old_Dataset_Date()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.DatasetDtm = new DateTime(1799, 1, 1);
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDateIsOld));
        }

        [TestCategory("DatasetTests")]
        [TestMethod]
        public void Can_prevent_dataset_with_old_Upload_Date()
        {
            Dataset dataset1 = CreateDataset();
            dataset1.UploadDtm = new DateTime(1799, 1, 1);
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.uploadDateIsOld));
        }

        public Dataset CreateDataset()
        {
            Dataset dataset1 = new Dataset(0,
                                            "Claim",
                                            "ClaimDataset.txt",
                                            "Test Claim Datasaet",
                                            "Creater_User",
                                            "Sentry_Owner_User",
                                            "Upload_User",
                                            "Origination_Code",
                                            System.DateTime.Now.AddYears(-13),
                                            System.DateTime.Now.AddYears(-12),
                                            System.DateTime.Now.AddYears(-11),
                                            "Create_Frequency",
                                            999,
                                            9999,
                                            "Claim/S3/Key",
                                            true,
                                            null,
                                            null);
            return dataset1;
        }

    }
}
