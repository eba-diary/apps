using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.data.Common;

namespace Sentry.data.Common.Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Yearly_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 1));
            Assert.AreEqual("yrly", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Quarterly_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 2));
            Assert.AreEqual("qrtly", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Montly_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 3));
            Assert.AreEqual("mntly", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Weekly_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 4));
            Assert.AreEqual("wkly", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Daily_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 5));
            Assert.AreEqual("dly", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_NonSchedule_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 7));
            Assert.AreEqual("nskd", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Transaction_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName(Enum.GetName(typeof(DatasetFrequency), 6));
            Assert.AreEqual("trn", result);
        }
        [TestMethod]
        public void GenerateDatasetFrequencyLocationName_Returns_Valid_Abbreviation_Default_Frequency()
        {
            var result = Utilities.GenerateDatasetFrequencyLocationName("ABC");
            Assert.AreEqual("dflt", result);
        }
        [TestMethod]
        public void FormatDatasetName_Replaces_Spaces_With_Underscores()
        {
            var result = Utilities.FormatDatasetName("Dataset Name  1 ");
            Assert.AreEqual("dataset_name__1_", result);
        }
        [TestMethod]
        public void FormatDatasetName_Returns_All_LowerCase_Letters()
        {
            var result = Utilities.FormatDatasetName("Data_SeT_BlAH");
            Assert.AreEqual("data_set_blah", result);
        }
        [TestMethod]
        public void GenerateLocationKey_Returns_Correct_Format()
        {
            var result = Utilities.GenerateLocationKey("Yearly", "Claim", "Dataset");
            Assert.AreEqual("data/claim/dataset/yrly/", result);
        }
        private Dataset GetMockDatasetData()
        {

            Category cat = new Category("Claim");
            Dataset ds = new Dataset();
            ds.DatasetId = 1;
            ds.Category = "Claim";
            ds.DatasetName = "Test Dataset";
            ds.DatasetDesc = "Testing Dataset Description";
            ds.CreationUserName = "Creation User";
            ds.SentryOwnerName = "072985";
            ds.UploadUserName = "072984";
            ds.OriginationCode = "Internal";
            ds.DatasetDtm = System.DateTime.Now.AddYears(-13);
            ds.ChangedDtm = System.DateTime.Now.AddYears(-13);
            ds.CreationFreqDesc = "NonSchedule";
            ds.S3Key = "";
            ds.IsSensitive = false;
            ds.CanDisplay = true;
            ds.RawMetadata = null;
            ds.DatasetCategory = cat;
            ds.DatasetFiles = null;

            return ds;
        }
    }

   
}
