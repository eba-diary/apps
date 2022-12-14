using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class RetrieverJobTests
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
            ftpSource.Verify(v => v.Validate(It.IsAny<RetrieverJob>(), It.IsAny<ValidationResults>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(RetrieverJob.ValidationErrors.scheduleIsNull, validationResult.Id);
        }

        [TestMethod]
        public void TryIncrementRequestVariables_AllIncrement_True()
        {
            RetrieverJob job = new RetrieverJob
            {
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableValue = "2021-01-01",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    },
                    new RequestVariable
                    {
                        VariableValue = "2021-01-02",
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            bool result = job.TryIncrementRequestVariables();

            Assert.IsTrue(result);
            Assert.AreEqual("2021-01-02", job.RequestVariables.First().VariableValue);
            Assert.AreEqual("2021-01-03", job.RequestVariables.Last().VariableValue);
        }

        [TestMethod]
        public void TryIncrementRequestVariables_OneFalseIncrement_False()
        {
            string firstDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            string lastDate = DateTime.Today.ToString("yyyy-MM-dd");

            RetrieverJob job = new RetrieverJob
            {
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableValue = firstDate,
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    },
                    new RequestVariable
                    {
                        VariableValue = lastDate,
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            bool result = job.TryIncrementRequestVariables();

            Assert.IsFalse(result);
            Assert.AreEqual(firstDate, job.RequestVariables.First().VariableValue);
            Assert.AreEqual(lastDate, job.RequestVariables.Last().VariableValue);
        }
    }
}
