using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.Core;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class RetrieverJobEntityTests
    {
        [TestMethod]
        public void ValidateForSave_Returns_ValidationError_For_Null_RelativeUri_When_DataSource_Is_FTPSource()
        {
            // Arrange
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = null,
                Schedule = "Yay",
                DataSource = new FtpSource()
                {
                    Id = 1
                }
            };

            // Act
            ValidationResults result = job.ValidateForSave();
            List<ValidationResult> resultList = result.GetAll();
            ValidationResult validationResult = resultList[0];

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(RetrieverJob.ValidationErrors.relativeUriNotSpecified, validationResult.Id);
        }

        [TestMethod]
        public void ValidateForSave_Returns_ValidationError_For_Null_Schedule_When_DataSource_Is_FTPSource()
        {
            // Arrange
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "Yay",
                Schedule = null,
                DataSource = new FtpSource()
                {
                    Id = 1
                }
            };

            // Act
            ValidationResults result = job.ValidateForSave();
            List<ValidationResult> resultList = result.GetAll();
            ValidationResult validationResult = resultList[0];

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(RetrieverJob.ValidationErrors.scheduleIsNull, validationResult.Id);
        }
    }
}
