using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Common;
using Sentry.data.Core;

namespace Sentry.data.Web.Tests
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
    }   
}
