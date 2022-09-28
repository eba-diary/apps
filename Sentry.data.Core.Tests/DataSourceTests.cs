using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhino.Mocks.Constraints;
using Sentry.Core;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.DataSource;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataSourceTests
    {
        [TestMethod]
        public void HttpsDataSourceValidations()
        {
            HTTPSSource httpsSource = new HTTPSSource();
            RetrieverJob retrieverJob = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"

                },
                RelativeUri = "relative_uri_value"
            };

            RetrieverJob retrieverJob_No_Relative_Uri = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.none,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = null
            };
            
            RetrieverJob retrieverJob_No_Target_Filename = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.none
                    },
                    TargetFileName = null
                },
                RelativeUri = "relative_uri_value"
            };

            Sentry.Core.ValidationResults results = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoRelativeUri = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoTargetFileName = new Sentry.Core.ValidationResults();

            //Act
            httpsSource.Validate(retrieverJob, results);
            httpsSource.Validate(retrieverJob_No_Relative_Uri, results_NoRelativeUri);
            httpsSource.Validate(retrieverJob_No_Target_Filename, results_NoTargetFileName);

            //
            Assert.IsFalse(results.GetAll().Any());
            Assert.AreEqual(1, results_NoRelativeUri.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.relativeUriNotSpecified, results_NoRelativeUri.GetAll().First().Id);
            Assert.AreEqual(1, results_NoTargetFileName.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsTargetFileNameIsBlank, results_NoTargetFileName.GetAll().First().Id);
        }

        [TestMethod]
        public void GoogleApiDataSourceValidations()
        {
            Mock<GoogleApiSource> googleApiSource = new Mock<GoogleApiSource>() { CallBase = true };
            RetrieverJob retrieverJob = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"

                },
                RelativeUri = "relative_uri_value",
                Schedule = "*****"
            };
            RetrieverJob retrieverJob_No_Request_Method = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the Body",
                        RequestMethod = HttpMethods.none,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = "relative_uri_value",
                Schedule = "*****"
            };
            RetrieverJob retrieverJob_No_Body_Format = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.none
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = "relative_uri_value"
            };
            RetrieverJob retrieverJob_No_Body = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = null,
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = "relative_uri_value"
            };

            googleApiSource.Setup(s => s.ValidateBase(It.IsAny<RetrieverJob>(), It.IsAny<Sentry.Core.ValidationResults>()));

            Sentry.Core.ValidationResults results = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoRequestMethod = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoBodyFormat = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoBody = new Sentry.Core.ValidationResults();

            //Act
            googleApiSource.Object.Validate(retrieverJob, results);
            googleApiSource.Object.Validate(retrieverJob_No_Request_Method, results_NoRequestMethod);
            googleApiSource.Object.Validate(retrieverJob_No_Body_Format, results_NoBodyFormat);
            googleApiSource.Object.Validate(retrieverJob_No_Body, results_NoBody);

            //
            googleApiSource.Verify(v => v.ValidateBase(It.IsAny<RetrieverJob>(), It.IsAny<Sentry.Core.ValidationResults>()), Times.Exactly(4));
            Assert.IsFalse(results.GetAll().Any());
            Assert.AreEqual(1, results_NoRequestMethod.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsRequestMethodNotSelected, results_NoRequestMethod.GetAll().First().Id);
            Assert.AreEqual(1, results_NoBodyFormat.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsRequestDataFormatNotSelected, results_NoBodyFormat.GetAll().First().Id);
            Assert.AreEqual(1, results_NoBody.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsRequestBodyIsBlank, results_NoBody.GetAll().First().Id);
        }

        [TestMethod]
        public void FtpSourceValidations()
        {
            Mock<FtpSource> googleApiSource = new Mock<FtpSource>() { CallBase = true };
            RetrieverJob retrieverJob = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {                    
                    FtpPattern = FtpPattern.RegexFileSinceLastExecution
                },
                RelativeUri = "relative_uri_value"
            };
            RetrieverJob retrieverJob_No_FtpPattern = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.None
                },
                RelativeUri = "relative_uri_value"
            };
            RetrieverJob retrieverJob_No_Relative_Uri = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.RegexFileSinceLastExecution
                },
                RelativeUri = null
            };

            Sentry.Core.ValidationResults results = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoFtpPattern = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoRelativeUri = new Sentry.Core.ValidationResults();

            //Act
            googleApiSource.Object.Validate(retrieverJob, results);
            googleApiSource.Object.Validate(retrieverJob_No_FtpPattern, results_NoFtpPattern);
            googleApiSource.Object.Validate(retrieverJob_No_Relative_Uri, results_NoRelativeUri);

            //
            Assert.IsFalse(results.GetAll().Any());
            Assert.AreEqual(1, results_NoFtpPattern.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.ftpPatternNotSelected, results_NoFtpPattern.GetAll().First().Id);
            Assert.AreEqual(1, results_NoRelativeUri.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.relativeUriNotSpecified, results_NoRelativeUri.GetAll().First().Id);
        }

        [TestMethod]
        public void GoogleBigQueryApiSource_Validate_Success()
        {
            ValidationResults validationResults = new ValidationResults();

            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "RelativeUri",
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        RequestMethod = HttpMethods.get
                    },
                    TargetFileName = "FileName"
                }
            };

            GoogleBigQueryApiSource source = new GoogleBigQueryApiSource();

            source.Validate(job, validationResults);

            Assert.IsTrue(validationResults.IsValid());
        }

        [TestMethod]
        public void GoogleBigQueryApiSource_Validate_Fail()
        {
            ValidationResults validationResults = new ValidationResults();

            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "/RelativeUri",
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        RequestMethod = HttpMethods.none
                    }
                }
            };

            GoogleBigQueryApiSource source = new GoogleBigQueryApiSource();

            source.Validate(job, validationResults);

            Assert.IsFalse(validationResults.IsValid());

            List<ValidationResult> results = validationResults.GetAll();
            Assert.AreEqual(3, results.Count);

            ValidationResult result = results.First();
            Assert.AreEqual(ValidationErrors.httpsTargetFileNameIsBlank, result.Id);
            Assert.AreEqual("Target file name is required for HTTPS data sources", result.Description);

            result = results[1];
            Assert.AreEqual(ValidationErrors.relativeUriStartsWithForwardSlash, result.Id);
            Assert.AreEqual("Relative Uri cannot start with '/' for HTTPS data sources", result.Description);

            result = results.Last();
            Assert.AreEqual(ValidationErrors.httpsRequestMethodNotSelected, result.Id);
            Assert.AreEqual("Request method is required", result.Description);
        }
    }
}
