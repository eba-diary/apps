using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class RetrieverJobEntityTests
    {
        [TestMethod]
        public void ValidateForSave_Returns_ValidationError_For_Null_Schedule_When_DataSource_Is_FTPSource()
        {
            // Arrange
            Mock<FtpSource> ftpSource = new Mock<FtpSource>() { CallBase = true };
            ftpSource.Setup(s => s.Validate(It.IsAny<RetrieverJob>(), It.IsAny<ValidationResults>()));
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "Uri_Value",
                Schedule = null,
                DataSource = ftpSource.Object,
                DataFlow = new Entities.DataProcessing.DataFlow()
                {
                    Id = 2,
                    IngestionType = (int)IngestionType.DSC_Pull
                },
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.None
                }
            };

            // Act
            ValidationResults result = job.ValidateForSave();
            List<ValidationResult> resultList = result.GetAll();
            ValidationResult validationResult = resultList[0];

            // Assert
            ftpSource.Verify(v => v.Validate(It.IsAny<RetrieverJob>(), It.IsAny<Sentry.Core.ValidationResults>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(RetrieverJob.ValidationErrors.scheduleIsNull, validationResult.Id);
        }
    }
}
