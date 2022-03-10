using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
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
                Schedule = "Schedule_Value",
                DataSource = new FtpSource()
                {
                    Id = 1
                },
                DataFlow = new Entities.DataProcessing.DataFlow()
                {
                    Id = 2,
                    IngestionType = (int)IngestionType.DSC_Pull
                },
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.NewFilesSinceLastexecution
                }
            };

            // Act
            ValidationResults result = job.ValidateForSave();
            List<ValidationResult> resultList = result.GetAll();
            ValidationResult validationResult = null;
            if (resultList != null && resultList.Count > 0)
            {
                validationResult = resultList[0];
            }

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(validationResult);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(RetrieverJob.ValidationErrors.relativeUriNotSpecified, validationResult.Id);
        }

        [TestMethod]
        public void ValidateForSave_Returns_ValidationError_For_Null_Schedule_When_DataSource_Is_FTPSource()
        {
            // Arrange
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "Uri_Value",
                Schedule = null,
                DataSource = new FtpSource()
                {
                    Id = 1
                },
                DataFlow = new Entities.DataProcessing.DataFlow()
                {
                    Id = 2,
                    IngestionType = (int)IngestionType.DSC_Pull
                },
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.NewFilesSinceLastexecution
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

        [TestMethod]
        public void ValidateForSave_Returns_ValidationError_For_No_FTPPattern_Selection_When_DataSource_Is_FTPSource()
        {
            // Arrange
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "Uri_Value",
                Schedule = "Schedule",
                DataSource = new FtpSource()
                {
                    Id = 1
                },
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
            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(RetrieverJob.ValidationErrors.ftpPatternNotSelected, validationResult.Id);
        }
    }
}
